using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace InFract.Usb.LibUsb.Native;

public enum libusb_hotplug_callback_handle;

[Flags]
public enum libusb_hotplug_event
{
	LIBUSB_HOTPLUG_EVENT_DEVICE_ARRIVED = 1 << 0,
	LIBUSB_HOTPLUG_EVENT_DEVICE_LEFT = 1 << 1,
}

[Flags]
public enum libusb_hotplug_flag
{
	LIBUSB_HOTPLUG_ENUMERATE = 1 << 0,
}

public static unsafe partial class LibUsbNative
{
	public const int LIBUSB_HOTPLUG_NO_FLAGS = 0;
	public const int LIBUSB_HOTPLUG_MATCH_ANY = -1;

	[LibraryImport(LibraryName)]
	public static partial int libusb_hotplug_register_callback(
		libusb_context* ctx,
		libusb_hotplug_event events,
		libusb_hotplug_flag flags,
		int vendor_id,
		int product_id,
		int dev_class,
		delegate* unmanaged<libusb_context*, libusb_device*, libusb_hotplug_event, nint, int> cb_fn,
		nint user_data,
		libusb_hotplug_callback_handle* callback_handle
	);

	[LibraryImport(LibraryName)]
	public static partial void libusb_hotplug_deregister_callback(
		libusb_context* ctx,
		libusb_hotplug_callback_handle callback_handle
	);

	[LibraryImport(LibraryName)]
	public static partial nint libusb_hotplug_get_user_data(
		libusb_context* ctx,
		libusb_hotplug_callback_handle callback_handle
	);
}
