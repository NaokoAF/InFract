using System.Collections;
using InFract.Platforms.Linux.Native;
using InFract.Platforms.Linux.Native.LibSystemd;
using static InFract.Platforms.Linux.Native.LibSystemd.LibSystemd;

namespace InFract.Platforms.Linux.Systemd;

public unsafe class SystemdDeviceEnumerator : IDisposable
{
	private readonly sd_device_enumerator* enumerator;

	internal SystemdDeviceEnumerator(sd_device_enumerator* enumerator) => this.enumerator = enumerator;

	public static SystemdDeviceEnumerator AllParents()
	{
		sd_device_enumerator* enumerator;
		SystemdException.ThrowIfError(sd_device_enumerator_new(&enumerator));
		SystemdException.ThrowIfError(sd_device_enumerator_add_all_parents(enumerator));
		return new(enumerator);
	}

	public SystemdDeviceEnumerator MatchSubSystem(Utf8String subSystem, bool match = true)
	{
		SystemdException.ThrowIfError(sd_device_enumerator_add_match_subsystem(enumerator, subSystem, match ? 1 : 0));
		return this;
	}

	public SystemdDeviceEnumerator MatchSysAttr(Utf8String sysAttr, Utf8String value, bool match = true)
	{
		SystemdException.ThrowIfError(sd_device_enumerator_add_match_sysattr(enumerator, sysAttr, value, match ? 1 : 0));
		return this;
	}

	public SystemdDeviceEnumerator MatchProperty(Utf8String property, Utf8String value)
	{
		SystemdException.ThrowIfError(sd_device_enumerator_add_match_property(enumerator, property, value));
		return this;
	}

	public SystemdDeviceEnumerator MatchPropertyRequired(Utf8String property, Utf8String value)
	{
		SystemdException.ThrowIfError(sd_device_enumerator_add_match_property_required(enumerator, property, value));
		return this;
	}

	public SystemdDeviceEnumerator AllowUninitialized()
	{
		SystemdException.ThrowIfError(sd_device_enumerator_allow_uninitialized(enumerator));
		return this;
	}

	public SystemdDeviceEnumerator MatchSysName(Utf8String sysName, bool match = true)
	{
		SystemdException.ThrowIfError(
			match
				? sd_device_enumerator_add_match_sysname(enumerator, sysName)
				: sd_device_enumerator_add_nomatch_sysname(enumerator, sysName)
		);
		return this;
	}

	public SystemdDeviceEnumerator MatchTag(Utf8String tag)
	{
		SystemdException.ThrowIfError(sd_device_enumerator_add_match_tag(enumerator, tag));
		return this;
	}

	public DeviceEnumerator GetDevices() => new(enumerator);

	private void ReleaseUnmanagedResources() => sd_device_enumerator_unref(enumerator);

	public void Dispose()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~SystemdDeviceEnumerator() => ReleaseUnmanagedResources();

	public struct DeviceEnumerator : IEnumerator<SystemdDevice>, IEnumerable<SystemdDevice>
	{
		private readonly sd_device_enumerator* enumerator;
		private sd_device* first;
		private sd_device* current;

		public SystemdDevice Current => new(current);
		object IEnumerator.Current => Current;

		internal DeviceEnumerator(sd_device_enumerator* enumerator)
		{
			this.enumerator = enumerator;
			first = sd_device_enumerator_get_device_first(enumerator);
		}

		public bool MoveNext()
		{
			if (current == null)
			{
				current = first;
				return current != null;
			}

			sd_device* next = sd_device_enumerator_get_device_next(enumerator);
			if (next == null) return false;

			current = next;
			return true;
		}

		public void Reset() => current = first;

		public IEnumerator<SystemdDevice> GetEnumerator() => new DeviceEnumerator(enumerator);
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		void IDisposable.Dispose()
		{
		}
	}
}
