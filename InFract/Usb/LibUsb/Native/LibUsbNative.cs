using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace InFract.Usb.LibUsb.Native;

public struct libusb_context
{
}

public enum libusb_log_level
{
	LIBUSB_LOG_LEVEL_NONE = 0,
	LIBUSB_LOG_LEVEL_ERROR = 1,
	LIBUSB_LOG_LEVEL_WARNING = 2,
	LIBUSB_LOG_LEVEL_INFO = 3,
	LIBUSB_LOG_LEVEL_DEBUG = 4,
}

[Flags]
public enum libusb_log_cb_mode
{
	LIBUSB_LOG_CB_GLOBAL = 1 << 0,
	LIBUSB_LOG_CB_CONTEXT = 1 << 1,
}

public enum libusb_option
{
	LIBUSB_OPTION_LOG_LEVEL = 0,
	LIBUSB_OPTION_USE_USBDK = 1,
	LIBUSB_OPTION_NO_DEVICE_DISCOVERY = 2,
	LIBUSB_OPTION_LOG_CB = 3,
	LIBUSB_OPTION_MAX = 4,
}

public static unsafe partial class LibUsbNative
{
	private const string LibraryName = "libusb-1.0.so.0";

	[LibraryImport(LibraryName)]
	public static partial void libusb_set_debug(libusb_context* ctx, libusb_log_level level);

	[LibraryImport(LibraryName)]
	public static partial void libusb_set_log_cb(
		libusb_context* ctx,
		delegate* unmanaged<libusb_context*, libusb_log_level, byte*, void> cb,
		int mode
	);

	[LibraryImport(LibraryName)]
	public static partial int libusb_init(libusb_context** ctx);

	// [LibraryImport(LibraryName)]
	// public static partial int libusb_init_context(libusb_context** ctx, libusb_init_option** options, int num_options);

	[LibraryImport(LibraryName)]
	public static partial void libusb_exit(libusb_context* ctx);
}
