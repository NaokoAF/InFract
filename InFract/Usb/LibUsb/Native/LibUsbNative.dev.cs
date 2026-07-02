using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace InFract.Usb.LibUsb.Native;

public enum libusb_speed
{
	LIBUSB_SPEED_UNKNOWN = 0,
	LIBUSB_SPEED_LOW = 1,
	LIBUSB_SPEED_FULL = 2,
	LIBUSB_SPEED_HIGH = 3,
	LIBUSB_SPEED_SUPER = 4,
	LIBUSB_SPEED_SUPER_PLUS = 5,
	LIBUSB_SPEED_SUPER_PLUS_X2 = 6
}

public struct libusb_device;

public struct libusb_device_handle;

public static unsafe partial class LibUsbNative
{
	[LibraryImport(LibraryName)]
	public static partial nint libusb_get_device_list(libusb_context* ctx, libusb_device*** list);

	[LibraryImport(LibraryName)]
	public static partial void libusb_free_device_list(libusb_device** list, int unref_devices);

	[LibraryImport(LibraryName)]
	public static partial ulong libusb_get_session_data(libusb_device* dev);

	[LibraryImport(LibraryName)]
	public static partial byte libusb_get_bus_number(libusb_device* dev);

	[LibraryImport(LibraryName)]
	public static partial byte libusb_get_port_number(libusb_device* dev);

	[LibraryImport(LibraryName)]
	public static partial int libusb_get_port_numbers(libusb_device* dev, byte* port_numbers, int port_numbers_len);

	[LibraryImport(LibraryName)]
	public static partial libusb_device* libusb_get_parent(libusb_device* dev);

	[LibraryImport(LibraryName)]
	public static partial byte libusb_get_device_address(libusb_device* dev);

	[LibraryImport(LibraryName)]
	public static partial libusb_speed libusb_get_device_speed(libusb_device* dev);

	[LibraryImport(LibraryName)]
	public static partial int libusb_get_max_packet_size(libusb_device* dev, byte endpoint);

	[LibraryImport(LibraryName)]
	public static partial int libusb_get_max_iso_packet_size(libusb_device* dev, byte endpoint);

	[LibraryImport(LibraryName)]
	public static partial int libusb_get_max_alt_packet_size(
		libusb_device* dev,
		int interface_number,
		int alternate_setting,
		byte endpoint
	);

	[LibraryImport(LibraryName)]
	public static partial libusb_device* libusb_ref_device(libusb_device* dev);

	[LibraryImport(LibraryName)]
	public static partial void libusb_unref_device(libusb_device* dev);

	[LibraryImport(LibraryName)]
	public static partial int libusb_wrap_sys_device(
		libusb_context* ctx,
		nint sys_dev,
		libusb_device_handle** dev_handle
	);

	[LibraryImport(LibraryName)]
	public static partial int libusb_open(libusb_device* dev, libusb_device_handle** dev_handle);

	[LibraryImport(LibraryName)]
	public static partial libusb_device_handle* libusb_open_device_with_vid_pid(
		libusb_context* ctx,
		ushort vendor_id,
		ushort product_id
	);

	[LibraryImport(LibraryName)]
	public static partial void libusb_close(libusb_device_handle* dev_handle);

	[LibraryImport(LibraryName)]
	public static partial libusb_device* libusb_get_device(libusb_device_handle* dev_handle);

	[LibraryImport(LibraryName)]
	public static partial int libusb_get_configuration(libusb_device_handle* dev_handle, int* config);

	[LibraryImport(LibraryName)]
	public static partial int libusb_set_configuration(libusb_device_handle* dev_handle, int configuration);

	[LibraryImport(LibraryName)]
	public static partial int libusb_claim_interface(libusb_device_handle* dev_handle, int interface_number);

	[LibraryImport(LibraryName)]
	public static partial int libusb_release_interface(libusb_device_handle* dev_handle, int interface_number);

	[LibraryImport(LibraryName)]
	public static partial int libusb_set_interface_alt_setting(
		libusb_device_handle* dev_handle,
		int interface_number,
		int alternate_setting
	);

	[LibraryImport(LibraryName)]
	public static partial int libusb_clear_halt(libusb_device_handle* dev_handle, byte endpoint);

	[LibraryImport(LibraryName)]
	public static partial int libusb_reset_device(libusb_device_handle* dev_handle);

	[LibraryImport(LibraryName)]
	public static partial int libusb_kernel_driver_active(libusb_device_handle* dev_handle, int interface_number);

	[LibraryImport(LibraryName)]
	public static partial int libusb_detach_kernel_driver(libusb_device_handle* dev_handle, int interface_number);

	[LibraryImport(LibraryName)]
	public static partial int libusb_attach_kernel_driver(libusb_device_handle* dev_handle, int interface_number);

	[LibraryImport(LibraryName)]
	public static partial int libusb_set_auto_detach_kernel_driver(libusb_device_handle* dev_handle, int enable);

	[LibraryImport(LibraryName)]
	public static partial int libusb_endpoint_supports_raw_io(libusb_device_handle* dev_handle, byte endpoint);

	[LibraryImport(LibraryName)]
	public static partial int libusb_endpoint_set_raw_io(libusb_device_handle* dev_handle, byte endpoint, int enable);

	[LibraryImport(LibraryName)]
	public static partial int libusb_get_max_raw_io_transfer_size(libusb_device_handle* dev_handle, byte endpoint);
}
