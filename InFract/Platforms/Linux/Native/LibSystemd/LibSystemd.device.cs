using System.Runtime.InteropServices;
using InFract.Platforms.Linux.Native.LibC;

// ReSharper disable InconsistentNaming

namespace InFract.Platforms.Linux.Native.LibSystemd;

public struct sd_device;

public struct sd_device_enumerator;

public struct sd_device_monitor;

public enum sd_device_action_t : long
{
	SD_DEVICE_ADD,
	SD_DEVICE_REMOVE,
	SD_DEVICE_CHANGE,
	SD_DEVICE_MOVE,
	SD_DEVICE_ONLINE,
	SD_DEVICE_OFFLINE,
	SD_DEVICE_BIND,
	SD_DEVICE_UNBIND,
}

public static unsafe partial class LibSystemd
{
	[LibraryImport(LibraryName)]
	public static partial sd_device* sd_device_ref(sd_device* p);

	[LibraryImport(LibraryName)]
	public static partial sd_device* sd_device_unref(sd_device* p);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_new_from_syspath(sd_device** ret, byte* syspath);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_new_from_devnum(sd_device** ret, byte type, dev_t devnum);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_new_from_subsystem_sysname(sd_device** ret, byte* subsystem, byte* sysname);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_new_from_device_id(sd_device** ret, byte* id);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_new_from_stat_rdev(sd_device** ret, stat* st);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_new_from_devname(sd_device** ret, byte* devname);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_new_from_path(sd_device** ret, byte* path);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_new_from_ifname(sd_device** ret, byte* ifname);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_new_from_ifindex(sd_device** ret, int ifindex);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_new_child(sd_device** ret, sd_device* device, byte* suffix);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_parent(sd_device* device, sd_device** ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_parent_with_subsystem_devtype(
		sd_device* device,
		byte* subsystem,
		byte* devtype,
		sd_device** ret
	);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_syspath(sd_device* device, byte** ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_subsystem(sd_device* device, byte** ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_driver_subsystem(sd_device* device, byte** ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_devtype(sd_device* device, byte** ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_devnum(sd_device* device, dev_t* ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_ifindex(sd_device* device, int* ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_driver(sd_device* device, byte** ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_devpath(sd_device* device, byte** ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_devname(sd_device* device, byte** ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_sysname(sd_device* device, byte** ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_sysnum(sd_device* device, byte** ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_action(sd_device* device, sd_device_action_t* ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_seqnum(sd_device* device, ulong* ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_diskseq(sd_device* device, ulong* ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_device_id(sd_device* device, byte** ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_is_initialized(sd_device* device);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_usec_initialized(sd_device* device, ulong* ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_usec_since_initialized(sd_device* device, ulong* ret);

	[LibraryImport(LibraryName)]
	public static partial byte* sd_device_get_tag_first(sd_device* device);

	[LibraryImport(LibraryName)]
	public static partial byte* sd_device_get_tag_next(sd_device* device);

	[LibraryImport(LibraryName)]
	public static partial byte* sd_device_get_current_tag_first(sd_device* device);

	[LibraryImport(LibraryName)]
	public static partial byte* sd_device_get_current_tag_next(sd_device* device);

	[LibraryImport(LibraryName)]
	public static partial byte* sd_device_get_devlink_first(sd_device* device);

	[LibraryImport(LibraryName)]
	public static partial byte* sd_device_get_devlink_next(sd_device* device);

	[LibraryImport(LibraryName)]
	public static partial byte* sd_device_get_property_first(sd_device* device, byte** value);

	[LibraryImport(LibraryName)]
	public static partial byte* sd_device_get_property_next(sd_device* device, byte** value);

	[LibraryImport(LibraryName)]
	public static partial byte* sd_device_get_sysattr_first(sd_device* device);

	[LibraryImport(LibraryName)]
	public static partial byte* sd_device_get_sysattr_next(sd_device* device);

	[LibraryImport(LibraryName)]
	public static partial sd_device* sd_device_get_child_first(sd_device* device, byte** ret_suffix);

	[LibraryImport(LibraryName)]
	public static partial sd_device* sd_device_get_child_next(sd_device* device, byte** ret_suffix);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_has_tag(sd_device* device, byte* tag);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_has_current_tag(sd_device* device, byte* tag);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_property_value(sd_device* device, byte* key, byte** ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_trigger_uuid(sd_device* device, sd_id128_t* ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_sysattr_value_with_size(
		sd_device* device,
		byte* sysattr,
		byte** ret_value,
		nuint* ret_size
	);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_get_sysattr_value(sd_device* device, byte* sysattr, byte** ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_set_sysattr_value(sd_device* device, byte* sysattr, byte* value);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_trigger(sd_device* device, sd_device_action_t action);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_trigger_with_uuid(sd_device* device, sd_device_action_t action, sd_id128_t* ret_uuid);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_open(sd_device* device, int flags);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_enumerator_new(sd_device_enumerator** ret);

	[LibraryImport(LibraryName)]
	public static partial sd_device_enumerator* sd_device_enumerator_ref(sd_device_enumerator* p);

	[LibraryImport(LibraryName)]
	public static partial sd_device_enumerator* sd_device_enumerator_unref(sd_device_enumerator* p);

	[LibraryImport(LibraryName)]
	public static partial sd_device* sd_device_enumerator_get_device_first(sd_device_enumerator* enumerator);

	[LibraryImport(LibraryName)]
	public static partial sd_device* sd_device_enumerator_get_device_next(sd_device_enumerator* enumerator);

	[LibraryImport(LibraryName)]
	public static partial sd_device* sd_device_enumerator_get_subsystem_first(sd_device_enumerator* enumerator);

	[LibraryImport(LibraryName)]
	public static partial sd_device* sd_device_enumerator_get_subsystem_next(sd_device_enumerator* enumerator);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_enumerator_add_match_subsystem(
		sd_device_enumerator* enumerator,
		byte* subsystem,
		int match
	);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_enumerator_add_match_sysattr(
		sd_device_enumerator* enumerator,
		byte* sysattr,
		byte* value,
		int match
	);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_enumerator_add_match_property(
		sd_device_enumerator* enumerator,
		byte* property,
		byte* value
	);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_enumerator_add_match_property_required(
		sd_device_enumerator* enumerator,
		byte* property,
		byte* value
	);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_enumerator_add_match_sysname(sd_device_enumerator* enumerator, byte* sysname);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_enumerator_add_nomatch_sysname(sd_device_enumerator* enumerator, byte* sysname);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_enumerator_add_match_tag(sd_device_enumerator* enumerator, byte* tag);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_enumerator_add_match_parent(sd_device_enumerator* enumerator, sd_device* parent);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_enumerator_allow_uninitialized(sd_device_enumerator* enumerator);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_enumerator_add_all_parents(sd_device_enumerator* enumerator);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_new(sd_device_monitor** ret);

	[LibraryImport(LibraryName)]
	public static partial sd_device_monitor* sd_device_monitor_ref(sd_device_monitor* p);

	[LibraryImport(LibraryName)]
	public static partial sd_device_monitor* sd_device_monitor_unref(sd_device_monitor* p);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_get_fd(sd_device_monitor* m);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_get_events(sd_device_monitor* m);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_get_timeout(sd_device_monitor* m, ulong* ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_set_receive_buffer_size(sd_device_monitor* m, nuint size);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_attach_event(sd_device_monitor* m, sd_event* event_);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_detach_event(sd_device_monitor* m);

	[LibraryImport(LibraryName)]
	public static partial sd_event* sd_device_monitor_get_event(sd_device_monitor* m);

	[LibraryImport(LibraryName)]
	public static partial sd_event_source* sd_device_monitor_get_event_source(sd_device_monitor* m);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_set_description(sd_device_monitor* m, byte* description);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_get_description(sd_device_monitor* m, byte** ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_is_running(sd_device_monitor* m);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_start(
		sd_device_monitor* m,
		delegate* unmanaged<sd_device_monitor*, sd_device*, void*, int> callback,
		void* userdata
	);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_stop(sd_device_monitor* m);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_receive(sd_device_monitor* m, sd_device** ret);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_filter_add_match_subsystem_devtype(
		sd_device_monitor* m,
		byte* subsystem,
		byte* devtype
	);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_filter_add_match_tag(sd_device_monitor* m, byte* tag);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_filter_add_match_sysattr(
		sd_device_monitor* m,
		byte* sysattr,
		byte* value,
		int match
	);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_filter_add_match_parent(sd_device_monitor* m, sd_device* device, int match);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_filter_update(sd_device_monitor* m);

	[LibraryImport(LibraryName)]
	public static partial int sd_device_monitor_filter_remove(sd_device_monitor* m);
}
