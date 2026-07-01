using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using InFract.Usb.LibUsb;
using InFract.Usb.LibUsb.Native;
using static InFract.Usb.LibUsb.Native.libusb_class_code;
using static InFract.Usb.LibUsb.Native.libusb_descriptor_type;
using static InFract.Usb.LibUsb.Native.libusb_endpoint_direction;
using static InFract.Usb.LibUsb.Native.libusb_endpoint_transfer_type;
using static InFract.Usb.LibUsb.Native.libusb_error;
using static InFract.Usb.LibUsb.Native.libusb_transfer_status;

namespace InFract.Usb.XUsb;

public unsafe class XUsbDriver : IDisposable
{
	public event Action<XUsbInputReport>? InputReceived;

	private readonly LibUsbDeviceHandle handle;
	private readonly byte interfaceNumber;
	private readonly nint gcHandle;
	private LibUsbTransfer inputTransfer;
	private LibUsbTransfer rumbleTransfer;

	private const libusb_class_code UsbClass = LIBUSB_CLASS_VENDOR_SPEC;
	private const byte UsbSubClass = 0x5D;
	private const byte UsbProtocolWired = 0x01;
	private const byte UsbProtocolWireless = 0x81;

	private const int ReportIdStateInput = 0x00;
	private const int ReportIdStateLed = 0x01;
	private const int ReportIdStateRumble = 0x03;
	private const int ReportIdStateBattery = 0x04;
	private const int ReportIdStateConnection = 0x08;
	private const int ReportIdControlRumble = 0x00;
	private const int ReportIdControlLed = 0x01;
	private const int ReportIdControlMasterRumble = 0x02;

	public XUsbDriver(
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

		rumbleTransfer = LibUsbTransfer.Allocate(0, endpointOutSize);
		rumbleTransfer.UserData = gcHandle;
		rumbleTransfer.FillInterrupt(handle, endpointOut, 100);
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

	public bool Rumble(byte leftRumble, byte rightRumble)
	{
		Span<byte> buffer = rumbleTransfer.WriteBuffer;
		buffer[0] = ReportIdControlRumble;
		buffer[1] = (byte)(Unsafe.SizeOf<XUsbRumbleReport>() + 2);
		Unsafe.As<byte, XUsbRumbleReport>(ref buffer[2]) = new()
		{
			LeftRumble = leftRumble,
			RightRumble = rightRumble,
		};

		libusb_error error = rumbleTransfer.Submit();
		if (error == LIBUSB_ERROR_BUSY) return false;

		LibUsbException.ThrowIfError(error);
		return true;
	}

	[UnmanagedCallersOnly]
	private static void OnInputTransferred(libusb_transfer* ptr)
	{
		LibUsbTransfer transfer = new(ptr);
		if (transfer.Status != LIBUSB_TRANSFER_CANCELLED) transfer.Submit();
		if (transfer.Status != LIBUSB_TRANSFER_COMPLETED) return;

		if (transfer.ReadLength < 2) return;

		// parse report
		ReadOnlySpan<byte> buffer = transfer.ReadBuffer;
		byte reportId = buffer[0];
		byte size = buffer[1];
		if (transfer.ReadLength < size) return;

		XUsbDriver self = (XUsbDriver)GCHandle.FromIntPtr(transfer.UserData).Target!;
		switch (reportId)
		{
			case ReportIdStateInput:
				if (size < Unsafe.SizeOf<XUsbInputReport>() + 2) break;

				ref XUsbInputReport input = ref Unsafe.As<byte, XUsbInputReport>(ref Unsafe.AsRef(in buffer[2]));
				self.InputReceived?.Invoke(input);
				break;
		}
	}

	public static IEnumerable<XUsbDriver> TryOpen(LibUsbDeviceHandle handle)
	{
		using LibUsbConfigDescriptor config = handle.Device.GetActiveConfigDescriptor();
		foreach (var interfaces in config.Interfaces)
		{
			if (interfaces.Length != 1) continue; // skip interfaces with alt settings

			LibUsbInterfaceDescriptor itf = interfaces[0];
			if (itf.Endpoints.Length < 2) continue; // XUSB must have at least 2 endpoints

			if (itf.InterfaceClass != UsbClass) continue;
			if (itf.InterfaceSubClass != UsbSubClass) continue;
			if (itf.InterfaceProtocol != UsbProtocolWireless && itf.InterfaceProtocol != UsbProtocolWired) continue;

			bool wireless;
			switch (itf.InterfaceProtocol)
			{
				case UsbProtocolWireless: wireless = true; break;
				case UsbProtocolWired: wireless = false; break;
				default: continue;
			}

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

			yield return new(
				handle,
				itf.InterfaceNumber,
				endpointIn.EndpointAddress,
				endpointOut.EndpointAddress,
				endpointIn.MaxPacketSize,
				endpointOut.MaxPacketSize
			);
		}
	}

	public void Dispose()
	{
		inputTransfer.Dispose();
		rumbleTransfer.Dispose();
		GCHandle.FromIntPtr(gcHandle).Free();
	}
}
