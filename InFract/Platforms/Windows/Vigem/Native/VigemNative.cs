using System.Runtime.InteropServices;

namespace InFract.Platforms.Windows.Vigem.Native;

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate void PVIGEM_X360_NOTIFICATION(
	nint client,
	nint target,
	byte largeMotor,
	byte smallMotor,
	byte ledNumber,
	nint userData
);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate void PVIGEM_DS4_NOTIFICATION(
	nint client,
	nint target,
	byte largeMotor,
	byte smallMotor,
	DS4_LIGHTBAR_COLOR lightbarColor,
	nint userData
);

public static partial class VigemNative
{
	private const string LibraryName = "vigemclient";

	[LibraryImport(LibraryName)]
	public static partial nint vigem_alloc();

	[LibraryImport(LibraryName)]
	public static partial void vigem_free(nint vigem);

	[LibraryImport(LibraryName)]
	public static partial VIGEM_ERROR vigem_connect(nint vigem);

	[LibraryImport(LibraryName)]
	public static partial void vigem_disconnect(nint vigem);

	[LibraryImport(LibraryName)]
	public static partial nint vigem_target_x360_alloc();

	[LibraryImport(LibraryName)]
	public static partial nint vigem_target_ds4_alloc();

	[LibraryImport(LibraryName)]
	public static partial void vigem_target_free(nint target);

	[LibraryImport(LibraryName)]
	public static partial VIGEM_ERROR vigem_target_add(nint vigem, nint target);

	[LibraryImport(LibraryName)]
	public static partial VIGEM_ERROR vigem_target_add_async(nint vigem, nint target, nint result);

	[LibraryImport(LibraryName)]
	public static partial VIGEM_ERROR vigem_target_remove(nint vigem, nint target);

	[LibraryImport(LibraryName)]
	public static partial VIGEM_ERROR vigem_target_x360_register_notification(
		nint vigem,
		nint target,
		PVIGEM_X360_NOTIFICATION notification
	);

	[LibraryImport(LibraryName)]
	public static partial VIGEM_ERROR vigem_target_ds4_register_notification(
		nint vigem,
		nint target,
		PVIGEM_DS4_NOTIFICATION notification
	);

	[LibraryImport(LibraryName)]
	public static partial void vigem_target_x360_unregister_notification(nint target);

	[LibraryImport(LibraryName)]
	public static partial void vigem_target_ds4_unregister_notification(nint target);

	[LibraryImport(LibraryName)]
	public static partial void vigem_target_set_vid(nint target, ushort vid);

	[LibraryImport(LibraryName)]
	public static partial void vigem_target_set_pid(nint target, ushort pid);

	[LibraryImport(LibraryName)]
	public static partial ushort vigem_target_get_vid(nint target);

	[LibraryImport(LibraryName)]
	public static partial ushort vigem_target_get_pid(nint target);

	[LibraryImport(LibraryName)]
	public static partial VIGEM_ERROR vigem_target_x360_update(nint vigem, nint target, XUSB_REPORT report);

	[LibraryImport(LibraryName)]
	public static partial VIGEM_ERROR vigem_target_ds4_update(nint vigem, nint target, DS4_REPORT report);

	[LibraryImport(LibraryName)]
	public static partial VIGEM_ERROR vigem_target_ds4_update_ex(nint vigem, nint target, DS4_REPORT_EX report);

	[LibraryImport(LibraryName)]
	public static partial uint vigem_target_get_index(nint target);

	[LibraryImport(LibraryName)]
	public static partial VIGEM_TARGET_TYPE vigem_target_get_type(nint target);

	[LibraryImport(LibraryName)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool vigem_target_is_attached(nint target);

	[LibraryImport(LibraryName)]
	public static partial VIGEM_ERROR vigem_target_x360_get_user_index(nint vigem, nint target, out uint index);

	[LibraryImport(LibraryName)]
	public static partial VIGEM_ERROR vigem_target_ds4_await_output_report(
		nint vigem,
		nint target,
		ref DS4_AWAIT_OUTPUT_BUFFER buffer
	);

	[LibraryImport(LibraryName)]
	public static partial VIGEM_ERROR vigem_target_ds4_await_output_report_timeout(
		nint vigem,
		nint target,
		uint milliseconds,
		ref DS4_AWAIT_OUTPUT_BUFFER buffer
	);
}
