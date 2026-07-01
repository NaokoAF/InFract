using System.Runtime.InteropServices;
using InFract.Usb.LibUsb;
using InFract.Usb.LibUsb.Native;
using static InFract.Usb.LibUsb.Native.libusb_error;
using static InFract.Usb.LibUsb.Native.libusb_transfer_status;

namespace InFract.Usb.Hid;

public unsafe class HidDriver : IDisposable
{
	public event Action<ReadOnlySpan<byte>>? InputReceived;

	private readonly LibUsbDeviceHandle handle;
	private readonly byte interfaceNumber;
	private readonly nint gcHandle;
	private LibUsbTransfer inputTransfer;
	private LibUsbTransfer outputTransfer;
	private bool outputLock;

	public HidDriver(
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

	public bool Write(ReadOnlySpan<byte> buffer)
	{
		if (!Interlocked.CompareExchange(ref outputLock, true, false)) return false;

		// write to transfer buffer
		outputTransfer.WriteLength = buffer.Length;
		buffer.CopyTo(outputTransfer.WriteBuffer);

		libusb_error error = outputTransfer.Submit();
		if (error == LIBUSB_ERROR_BUSY)
		{
			outputLock = false;
			return false;
		}

		LibUsbException.ThrowIfError(error);
		return true;
	}

	[UnmanagedCallersOnly]
	private static void OnInputTransferred(libusb_transfer* ptr)
	{
		LibUsbTransfer transfer = new(ptr);
		if (transfer.Status != LIBUSB_TRANSFER_CANCELLED) transfer.Submit();
		if (transfer.Status != LIBUSB_TRANSFER_COMPLETED) return;

		HidDriver self = (HidDriver)GCHandle.FromIntPtr(transfer.UserData).Target!;
		self.InputReceived?.Invoke(transfer.ReadBuffer);
	}

	[UnmanagedCallersOnly]
	private static void OnOutputTransferred(libusb_transfer* ptr)
	{
		LibUsbTransfer transfer = new(ptr);

		HidDriver self = (HidDriver)GCHandle.FromIntPtr(transfer.UserData).Target!;
		self.outputLock = false;
	}

	public void Dispose()
	{
		inputTransfer.Dispose();
		outputTransfer.Dispose();
		GCHandle.FromIntPtr(gcHandle).Free();
	}
}
