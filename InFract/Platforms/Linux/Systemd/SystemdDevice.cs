using System.Collections;
using InFract.Platforms.Linux.Native;
using InFract.Platforms.Linux.Native.LibC;
using InFract.Platforms.Linux.Native.LibSystemd;
using static InFract.Platforms.Linux.Native.LibSystemd.LibSystemd;

namespace InFract.Platforms.Linux.Systemd;

public unsafe class SystemdDevice : IDisposable
{
	public Utf8String SysPath
	{
		get
		{
			byte* pointer;
			SystemdException.ThrowIfError(sd_device_get_syspath(device, &pointer));
			return pointer;
		}
	}

	public Utf8String SysName
	{
		get
		{
			byte* pointer;
			SystemdException.ThrowIfError(sd_device_get_sysname(device, &pointer));
			return pointer;
		}
	}

	public Utf8String SysNum
	{
		get
		{
			byte* pointer;
			SystemdException.ThrowIfError(sd_device_get_sysnum(device, &pointer));
			return pointer;
		}
	}

	public Utf8String DevPath
	{
		get
		{
			byte* pointer;
			SystemdException.ThrowIfError(sd_device_get_devpath(device, &pointer));
			return pointer;
		}
	}

	public Utf8String DevName
	{
		get
		{
			byte* pointer;
			SystemdException.ThrowIfError(sd_device_get_devname(device, &pointer));
			return pointer;
		}
	}

	public Utf8String DevType
	{
		get
		{
			byte* pointer;
			SystemdException.ThrowIfError(sd_device_get_devtype(device, &pointer));
			return pointer;
		}
	}

	public dev_t DevNum
	{
		get
		{
			dev_t num;
			SystemdException.ThrowIfError(sd_device_get_devnum(device, &num));
			return num;
		}
	}

	private readonly sd_device* device;

	internal SystemdDevice(sd_device* device) => this.device = device;

	public static SystemdDevice FromSysPath(Utf8String sysPath)
	{
		sd_device* device;
		SystemdException.ThrowIfError(sd_device_new_from_syspath(&device, sysPath));
		return new(device);
	}

	public static SystemdDevice FromDevNum(byte type, dev_t devNum)
	{
		sd_device* device;
		SystemdException.ThrowIfError(sd_device_new_from_devnum(&device, type, devNum));
		return new(device);
	}
	
	public static SystemdDevice FromSubSystemSysName(Utf8String subSystem, Utf8String sysName)
	{
		sd_device* device;
		SystemdException.ThrowIfError(sd_device_new_from_subsystem_sysname(&device, subSystem, sysName));
		return new(device);
	}
	
	public static SystemdDevice FromDevName(Utf8String devName)
	{
		sd_device* device;
		SystemdException.ThrowIfError(sd_device_new_from_devname(&device, devName));
		return new(device);
	}

	public bool TryGetProperty(Utf8String key, out Utf8String value)
	{
		byte* pointer;
		if (sd_device_get_property_value(device, key, &pointer) < 0)
		{
			value = default;
			return false;
		}

		value = pointer;
		return true;
	}

	public bool TryGetSysAttribute(Utf8String key, out Utf8String value)
	{
		byte* pointer;
		if (sd_device_get_sysattr_value(device, key, &pointer) < 0)
		{
			value = default;
			return false;
		}

		value = pointer;
		return true;
	}

	public PropertyEnumerator EnumerateProperties() => new(device);

	public SystemdDeviceEnumerator EnumerateChildren()
	{
		sd_device_enumerator* enumerator;
		SystemdException.ThrowIfError(sd_device_enumerator_new(&enumerator));
		SystemdException.ThrowIfError(sd_device_enumerator_add_match_parent(enumerator, device));
		return new(enumerator);
	}

	private void ReleaseUnmanagedResources() => sd_device_unref(device);

	public void Dispose()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~SystemdDevice() => ReleaseUnmanagedResources();

	public struct PropertyEnumerator : IEnumerator<Utf8String>, IEnumerable<Utf8String>
	{
		private readonly sd_device* device;
		private byte* first;
		private byte* current;

		public Utf8String Current => current;
		object IEnumerator.Current => (nint)current;

		internal PropertyEnumerator(sd_device* device)
		{
			this.device = device;
			first = sd_device_get_property_first(device, null);
		}

		public bool MoveNext()
		{
			if (current == null)
			{
				current = first;
				return current != null;
			}

			byte* next = sd_device_get_property_next(device, null);
			if (next == null) return false;

			current = next;
			return true;
		}

		public void Reset() => current = first;

		public IEnumerator<Utf8String> GetEnumerator() => new PropertyEnumerator(device);
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		void IDisposable.Dispose()
		{
		}
	}
}
