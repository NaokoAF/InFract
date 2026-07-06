using System.Runtime.InteropServices;
using InFract.Usb.LibUsb;
using InFract.Usb.LibUsb.Native;
using static InFract.Usb.LibUsb.Native.libusb_descriptor_type;
using static InFract.Usb.LibUsb.Native.libusb_endpoint_direction;
using static InFract.Usb.LibUsb.Native.libusb_endpoint_transfer_type;
using static InFract.Usb.LibUsb.Native.libusb_error;
using static InFract.Usb.LibUsb.Native.libusb_transfer_status;

namespace InFract.Usb.Hid;

public unsafe class HidLibUsbInterface : IHidInterface
{
	public event Action<ReadOnlySpan<byte>>? InputReceived;

	private readonly LibUsbDeviceHandle handle;
	private readonly byte interfaceNumber;
	private readonly nint gcHandle;
	private LibUsbTransfer inputTransfer;
	private LibUsbTransfer outputTransfer;
	private bool outputLock;

	public HidLibUsbInterface(
		LibUsbDeviceHandle handle,
		byte interfaceNumber,
		byte endpointIn,
		byte endpointOut,
		int endpointInSize,
		int endpointOutSize
	)
	{
		this.handle = handle;
		this.interfaceNumber = interfaceNumber;
		gcHandle = (nint)GCHandle.Alloc(this);

		inputTransfer = LibUsbTransfer.Allocate(0, endpointInSize);
		inputTransfer.UserData = gcHandle;
		inputTransfer.FillInterrupt(handle, endpointIn, 100);
		inputTransfer.SetCallback(&OnInputTransferred);

		outputTransfer = LibUsbTransfer.Allocate(0, endpointOutSize);
		outputTransfer.UserData = gcHandle;
		outputTransfer.FillInterrupt(handle, endpointOut, 100);
		outputTransfer.SetCallback(&OnOutputTransferred);
	}

	public void Open()
	{
		handle.ClaimInterface(interfaceNumber);
		inputTransfer.Submit();
	}

	public void Close()
	{
		inputTransfer.Cancel();
		handle.ReleaseInterface(interfaceNumber);
	}

	public int Write(ReadOnlySpan<byte> buffer)
	{
		if (!Interlocked.CompareExchange(ref outputLock, true, false)) return -1;

		// write to transfer buffer
		outputTransfer.WriteLength = buffer.Length;
		buffer.CopyTo(outputTransfer.WriteBuffer);

		libusb_error error = outputTransfer.Submit();
		if (error == LIBUSB_ERROR_BUSY)
		{
			outputLock = false;
			return -1;
		}

		LibUsbException.ThrowIfError(error);
		return buffer.Length;
	}

	[UnmanagedCallersOnly]
	private static void OnInputTransferred(libusb_transfer* ptr)
	{
		LibUsbTransfer transfer = new(ptr);
		if (transfer.Status != LIBUSB_TRANSFER_CANCELLED) transfer.Submit();
		if (transfer.Status != LIBUSB_TRANSFER_COMPLETED) return;

		HidLibUsbInterface self = (HidLibUsbInterface)GCHandle.FromIntPtr(transfer.UserData).Target!;
		self.InputReceived?.Invoke(transfer.ReadBuffer);
	}

	[UnmanagedCallersOnly]
	private static void OnOutputTransferred(libusb_transfer* ptr)
	{
		LibUsbTransfer transfer = new(ptr);

		HidLibUsbInterface self = (HidLibUsbInterface)GCHandle.FromIntPtr(transfer.UserData).Target!;
		self.outputLock = false;
	}
	
	public static HidLibUsbInterface Open(LibUsbDeviceHandle handle, byte interfaceNumber)
	{
		using LibUsbConfigDescriptor config = handle.Device.GetActiveConfigDescriptor();
		foreach (var interfaces in config.Interfaces)
		{
			if (interfaces.Length != 1) continue; // skip interfaces with alt settings

			LibUsbInterfaceDescriptor itf = interfaces[0];
			if (itf.InterfaceNumber != interfaceNumber) continue;
			if (itf.InterfaceClass != libusb_class_code.LIBUSB_CLASS_HID) continue;
			if (itf.Endpoints.Length < 2) continue;

			// find endpoints
			LibUsbEndpointDescriptor? endpointIn = null;
			LibUsbEndpointDescriptor? endpointOut = null;
			foreach (LibUsbEndpointDescriptor endpoint in itf.Endpoints)
			{
				if (endpoint.DescriptorType != LIBUSB_DT_ENDPOINT) continue;
				if (endpoint.TransferType != LIBUSB_ENDPOINT_TRANSFER_TYPE_INTERRUPT) continue;

				if (endpoint.Direction == LIBUSB_ENDPOINT_IN)
				{
					endpointIn ??= endpoint; // use first found endpoint
				}
				else
				{
					endpointOut ??= endpoint; // use first found endpoint
				}
			}

			if (endpointIn == null || endpointOut == null) continue;

			return new(
				handle,
				itf.InterfaceNumber,
				endpointIn.EndpointAddress,
				endpointOut.EndpointAddress,
				endpointIn.MaxPacketSize,
				endpointOut.MaxPacketSize
			);
		}

		throw new InvalidOperationException("No valid HID interface");
	}

	public void Dispose()
	{
		inputTransfer.Dispose();
		outputTransfer.Dispose();
		GCHandle.FromIntPtr(gcHandle).Free();
	}
}
