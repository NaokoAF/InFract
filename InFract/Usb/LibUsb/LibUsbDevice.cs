using System.Text;
using InFract.Usb.LibUsb.Native;
using static InFract.Usb.LibUsb.Native.LibUsbNative;

namespace InFract.Usb.LibUsb;

public unsafe class LibUsbDevice : IDisposable
{
	public ulong SessionData => libusb_get_session_data(device);
	public byte BusNumber => libusb_get_bus_number(device);
	public byte PortNumber => libusb_get_port_number(device);
	public byte DeviceAddress => libusb_get_device_address(device);
	public libusb_speed Speed => libusb_get_device_speed(device);

	private readonly libusb_device* device;
	private bool disposed;

	internal LibUsbDevice(libusb_device* device)
	{
		this.device = device;
		libusb_ref_device(device);
	}

	public LibUsbDeviceDescriptor GetDeviceDescriptor()
	{
		libusb_device_descriptor descriptor;
		LibUsbException.ThrowIfError(libusb_get_device_descriptor(device, &descriptor));
		return new(descriptor);
	}

	public LibUsbConfigDescriptor GetActiveConfigDescriptor()
	{
		libusb_config_descriptor* config;
		LibUsbException.ThrowIfError(libusb_get_active_config_descriptor(device, &config));
		return new(config);
	}

	public LibUsbConfigDescriptor GetConfigDescriptor(byte index)
	{
		libusb_config_descriptor* config;
		LibUsbException.ThrowIfError(libusb_get_config_descriptor(device, index, &config));
		return new(config);
	}

	public LibUsbConfigDescriptor GetConfigDescriptorByValue(byte configurationValue)
	{
		libusb_config_descriptor* config;
		LibUsbException.ThrowIfError(libusb_get_config_descriptor_by_value(device, configurationValue, &config));
		return new(config);
	}

	public int GetDeviceString(libusb_device_string_type type, Span<byte> data)
	{
		fixed (byte* ptr = data)
			return LibUsbException.ThrowIfError(libusb_get_device_string(device, type, ptr, data.Length));
	}

	public string GetDeviceString(libusb_device_string_type type)
	{
		Span<byte> data = stackalloc byte[LIBUSB_DEVICE_STRING_BYTES_MAX];
		int length = GetDeviceString(type, data);
		return Encoding.UTF8.GetString(data[..length]);
	}

	public LibUsbDeviceHandle Open()
	{
		libusb_device_handle* handle;
		LibUsbException.ThrowIfError(libusb_open(device, &handle));
		return new(handle);
	}

	private void ReleaseUnmanagedResources()
	{
		libusb_unref_device(device);
	}

	public void Dispose()
	{
		if (disposed) return;
		disposed = true;

		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~LibUsbDevice() => ReleaseUnmanagedResources();
}
