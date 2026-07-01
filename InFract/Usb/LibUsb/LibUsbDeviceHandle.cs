using System.Runtime.CompilerServices;
using InFract.Usb.LibUsb.Native;
using static InFract.Usb.LibUsb.Native.libusb_error;
using static InFract.Usb.LibUsb.Native.LibUsbNative;

namespace InFract.Usb.LibUsb;

public unsafe class LibUsbDeviceHandle : IDisposable
{
	public libusb_device_handle* Handle => handle;
	public LibUsbDevice Device => device;

	private readonly libusb_device_handle* handle;
	private readonly LibUsbDevice device;

	internal LibUsbDeviceHandle(libusb_device_handle* handle)
	{
		this.handle = handle;
		device = new(libusb_get_device(handle));
	}

	public byte GetConfiguration()
	{
		int configuration;
		LibUsbException.ThrowIfError(libusb_get_configuration(handle, &configuration));
		return (byte)configuration;
	}

	public void SetConfiguration(byte configuration)
	{
		LibUsbException.ThrowIfError(libusb_set_configuration(handle, configuration));
	}

	public void ClaimInterface(byte interfaceNumber)
	{
		LibUsbException.ThrowIfError(libusb_claim_interface(handle, interfaceNumber));
	}

	public void ReleaseInterface(byte interfaceNumber)
	{
		LibUsbException.ThrowIfError(libusb_release_interface(handle, interfaceNumber));
	}

	public void SetInterfaceAltSetting(byte interfaceNumber, byte alternateSetting)
	{
		LibUsbException.ThrowIfError(libusb_set_interface_alt_setting(handle, interfaceNumber, alternateSetting));
	}

	public void ClearHalt(byte endpoint)
	{
		LibUsbException.ThrowIfError(libusb_clear_halt(handle, endpoint));
	}

	public bool Reset()
	{
		libusb_error error = (libusb_error)libusb_reset_device(handle);
		if (error == LIBUSB_ERROR_NOT_FOUND) return false;
		return error == LIBUSB_SUCCESS ? true : throw new LibUsbException(error);
	}

	public bool SetAutoDetachKernelDriver(bool enable)
	{
		libusb_error error = (libusb_error)libusb_set_auto_detach_kernel_driver(handle, enable ? 1 : 0);
		if (error == LIBUSB_ERROR_NOT_SUPPORTED) return false;

		LibUsbException.ThrowIfError(error);
		return true;
	}

	public bool SupportsRawIo(byte endpoint)
	{
		return LibUsbException.ThrowIfError(libusb_endpoint_supports_raw_io(handle, endpoint)) == 1;
	}

	public void SetRawIo(byte endpoint, bool enable)
	{
		LibUsbException.ThrowIfError(libusb_endpoint_set_raw_io(handle, endpoint, enable ? 1 : 0));
	}

	public int GetMaxRawIoTransferSize(byte endpoint)
	{
		return LibUsbException.ThrowIfError(libusb_get_max_raw_io_transfer_size(handle, endpoint));
	}

	public int GetDescriptor(libusb_descriptor_type type, byte index, Span<byte> data)
	{
		fixed (byte* ptr = data)
			return LibUsbException.ThrowIfError(libusb_get_descriptor(handle, type, index, ptr, data.Length));
	}

	public int GetStringDescriptor(byte index, ushort language, Span<char> data)
	{
		fixed (char* ptr = data)
		{
			int length = LibUsbException.ThrowIfError(
				libusb_get_string_descriptor(handle, index, language, (byte*)ptr, data.Length * sizeof(char))
			);
			return length / sizeof(char);
		}
	}

	public string GetStringDescriptor(byte index, ushort language)
	{
		Span<char> data = stackalloc char[LIBUSB_DEVICE_STRING_BYTES_MAX / sizeof(char)];
		int length = GetStringDescriptor(index, language, data);
		return new string(data[..length]);
	}

	public bool IsKernelDriverActive(byte interfaceNumber)
	{
		return LibUsbException.ThrowIfError(libusb_kernel_driver_active(handle, interfaceNumber)) == 1;
	}

	public void DetachKernelDriver(byte interfaceNumber)
	{
		LibUsbException.ThrowIfError(libusb_detach_kernel_driver(handle, interfaceNumber));
	}

	public void AttachKernelDriver(byte interfaceNumber)
	{
		LibUsbException.ThrowIfError(libusb_attach_kernel_driver(handle, interfaceNumber));
	}

	public libusb_error TransferControl(
		byte requestType,
		byte request,
		ushort value,
		ushort index,
		Span<byte> data,
		uint timeout,
		out int transferred
	)
	{
		int ret;
		fixed (byte* ptr = data)
			ret = libusb_control_transfer(handle, requestType, request, value, index, ptr, (ushort)data.Length, timeout);

		transferred = ret > 0 ? ret : 0;
		return (libusb_error)ret;
	}

	public libusb_error TransferBulk(byte endpoint, Span<byte> data, uint timeout, out int transferred)
	{
		Unsafe.SkipInit(out transferred);
		int* transferredPtr = (int*)Unsafe.AsPointer(ref transferred);

		fixed (byte* ptr = data)
			return (libusb_error)libusb_bulk_transfer(handle, endpoint, ptr, data.Length, transferredPtr, timeout);
	}

	public libusb_error TransferInterrupt(byte endpoint, Span<byte> data, uint timeout, out int transferred)
	{
		Unsafe.SkipInit(out transferred);
		int* transferredPtr = (int*)Unsafe.AsPointer(ref transferred);

		fixed (byte* ptr = data)
			return (libusb_error)libusb_interrupt_transfer(handle, endpoint, ptr, data.Length, transferredPtr, timeout);
	}

	private void ReleaseUnmanagedResources()
	{
		libusb_close(handle);
	}

	public void Dispose()
	{
		device.Dispose();
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~LibUsbDeviceHandle() => ReleaseUnmanagedResources();
}
