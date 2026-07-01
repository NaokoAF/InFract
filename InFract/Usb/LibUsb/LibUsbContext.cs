using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using InFract.Usb.LibUsb.Native;
using static InFract.Usb.LibUsb.Native.libusb_error;
using static InFract.Usb.LibUsb.Native.LibUsbNative;

namespace InFract.Usb.LibUsb;

public unsafe class LibUsbContext : IDisposable
{
	private readonly libusb_context* context;

	public LibUsbContext()
	{
		libusb_context* context;
		LibUsbException.ThrowIfError(libusb_init(&context));
		this.context = context;
	}

	public void HandleEvents(uint timeout)
	{
		ToTimeVal(timeout, out timeval tv);
		libusb_error err = (libusb_error)libusb_handle_events_timeout(context, &tv);
		if (err == LIBUSB_ERROR_INTERRUPTED) return;
		LibUsbException.ThrowIfError(err);
	}

	public LibUsbDevice[] GetDeviceList()
	{
		libusb_device** list;
		nint count = LibUsbException.ThrowIfError(libusb_get_device_list(context, &list));

		LibUsbDevice[] devices = new LibUsbDevice[count];
		for (int i = 0; i < devices.Length; i++)
		{
			devices[i] = new(list[i]);
		}

		libusb_free_device_list(list, 1);
		return devices;
	}

	public LibUsbDeviceHandle WrapSystemDevice(nint systemHandle)
	{
		libusb_device_handle* handle;
		LibUsbException.ThrowIfError(libusb_wrap_sys_device(context, systemHandle, &handle));
		return new(handle);
	}

	public libusb_hotplug_callback_handle RegisterHotPlugCallback(
		libusb_hotplug_event events,
		libusb_hotplug_flag flags,
		LibUsbHotPlugCallback callback
	)
	{
		libusb_hotplug_callback_handle handle;
		LibUsbException.ThrowIfError(
			libusb_hotplug_register_callback(
				context,
				events,
				flags,
				LIBUSB_HOTPLUG_MATCH_ANY,
				LIBUSB_HOTPLUG_MATCH_ANY,
				LIBUSB_HOTPLUG_MATCH_ANY,
				&HotPlugCallbackWrapper,
				(nint)GCHandle.Alloc(callback),
				&handle
			)
		);
		return handle;
	}

	public void DeregisterHotPlugCallback(libusb_hotplug_callback_handle handle)
	{
		// release GCHandle
		GCHandle callbackHandle = GCHandle.FromIntPtr(libusb_hotplug_get_user_data(context, handle));
		callbackHandle.Free();

		libusb_hotplug_deregister_callback(context, handle);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ToTimeVal(uint timeout, out timeval tv)
	{
		Unsafe.SkipInit(out tv);
		tv.tv_sec = (nint)timeout / 1000;
		tv.tv_usec = ((nint)timeout % 1000) * 1000;
	}

	[UnmanagedCallersOnly]
	private static bool HotPlugCallbackWrapper(
		libusb_context* contextPtr,
		libusb_device* devicePtr,
		libusb_hotplug_event eventType,
		nint userData
	)
	{
		GCHandle callbackHandle = GCHandle.FromIntPtr(userData);
		LibUsbHotPlugCallback? callback = callbackHandle.Target as LibUsbHotPlugCallback;
		if (callback == null) return true;

		LibUsbDevice device = new(devicePtr);
		return callback(device, eventType);
	}

	private void ReleaseUnmanagedResources()
	{
		libusb_exit(context);
	}

	public void Dispose()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~LibUsbContext() => ReleaseUnmanagedResources();
}
