using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL;
using static SDL.SDL3;

namespace InFract.SDL3.HidApi;

public static unsafe class Hid
{
	public static uint DeviceChangeCount => SDL_hid_device_change_count();

	public static void Initialize()
	{
		SDL_SetHint(SDL_HINT_HIDAPI_ENUMERATE_ONLY_CONTROLLERS, "0");
		if (SDL_hid_init() < 0)
			throw new Exception($"Failed to initialize SDL HIDAPI: {SDL_GetError()}");
	}

	public static HidDevice Open(string path)
	{
		SDL_hid_device* devicePtr = SDL_hid_open_path(path);
		if (devicePtr == null) throw new Exception($"Failed to open HID device: {SDL_GetError()}");

		SDL_hid_device_info* infoPtr = SDL_hid_get_device_info(devicePtr);
		if (infoPtr == null) throw new Exception($"Failed to get HID device info: {SDL_GetError()}");

		return new(devicePtr, ConvertDeviceInfo(infoPtr));
	}

	public static DeviceEnumerator EnumerateDevices(ushort vendorId = 0, ushort productId = 0)
	{
		return new(SDL_hid_enumerate(vendorId, productId));
	}

	public static void Deinitialize()
	{
		SDL_hid_exit();
	}
	
	private static HidDeviceInfo ConvertDeviceInfo(SDL_hid_device_info* info) => new()
	{
		Path = PtrToStringUTF8(info->path)!,
		VendorId = info->vendor_id,
		ProductId = info->product_id,
		SerialNumber = PtrToStringUnicode(info->serial_number),
		ReleaseNumber = info->release_number,
		ManufacturerString = PtrToStringUnicode(info->manufacturer_string),
		ProductString = PtrToStringUnicode(info->product_string),
		UsagePage = info->usage_page,
		Usage = info->usage,
		InterfaceNumber = info->interface_number,
		InterfaceClass = info->interface_class,
		InterfaceSubClass = info->interface_subclass,
		InterfaceProtocol = info->interface_protocol,
	};

	private static string? PtrToStringUnicode(nint pointer)
	{
		if (pointer == 0) return null;
		
		// iterate through every char until we reach a null terminator
		ref char first = ref Unsafe.AsRef<char>((void*)pointer);
		ref char c = ref first;
		int count = 0;
		while (c != '\0')
		{
			c = ref Unsafe.Add(ref c, Unsafe.SizeOf<char>());
			count += Unsafe.SizeOf<char>();
		}

		return new string(MemoryMarshal.CreateReadOnlySpan(ref first, count));
	}

	public struct DeviceEnumerator : IEnumerator<HidDeviceInfo>, IEnumerable<HidDeviceInfo>
	{
		private SDL_hid_device_info* list;
		private SDL_hid_device_info* current;
		private bool started;

		public HidDeviceInfo Current => current != null ? ConvertDeviceInfo(current) : default;
		object IEnumerator.Current => Current;

		internal DeviceEnumerator(SDL_hid_device_info* list)
		{
			this.list = list;
		}

		public bool MoveNext()
		{
			current = started ? current->next : list;
			started = true;
			return current != null;
		}

		public void Reset()
		{
			current = null;
			started = false;
		}

		public void Dispose() => SDL_hid_free_enumeration(list);

		public DeviceEnumerator GetEnumerator() => this;
		IEnumerator<HidDeviceInfo> IEnumerable<HidDeviceInfo>.GetEnumerator() => GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
