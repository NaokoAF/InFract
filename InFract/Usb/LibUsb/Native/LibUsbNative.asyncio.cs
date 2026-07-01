using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace InFract.Usb.LibUsb.Native;

[Flags]
public enum libusb_transfer_type : byte
{
	LIBUSB_TRANSFER_TYPE_CONTROL = 0,
	LIBUSB_TRANSFER_TYPE_ISOCHRONOUS = 1,
	LIBUSB_TRANSFER_TYPE_BULK = 2,
	LIBUSB_TRANSFER_TYPE_INTERRUPT = 3,
	LIBUSB_TRANSFER_TYPE_BULK_STREAM = 4,
}

public enum libusb_transfer_status
{
	LIBUSB_TRANSFER_COMPLETED,
	LIBUSB_TRANSFER_ERROR,
	LIBUSB_TRANSFER_TIMED_OUT,
	LIBUSB_TRANSFER_CANCELLED,
	LIBUSB_TRANSFER_STALL,
	LIBUSB_TRANSFER_NO_DEVICE,
	LIBUSB_TRANSFER_OVERFLOW,
}

[Flags]
public enum libusb_transfer_flags : byte
{
	LIBUSB_TRANSFER_SHORT_NOT_OK = 1 << 0,
	LIBUSB_TRANSFER_FREE_BUFFER = 1 << 1,
	LIBUSB_TRANSFER_FREE_TRANSFER = 1 << 2,
	LIBUSB_TRANSFER_ADD_ZERO_PACKET = 1 << 3,
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct libusb_control_setup
{
	public byte bmRequestType;
	public byte bRequest;
	public ushort wValue;
	public ushort wIndex;
	public ushort wLength;
}

[StructLayout(LayoutKind.Sequential)]
public struct libusb_iso_packet_descriptor
{
	public uint length;
	public uint actual_length;
	public libusb_transfer_status status;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct libusb_transfer
{
	public libusb_device_handle *dev_handle;
	public byte flags;
	public byte endpoint;
	public byte type;
	public uint timeout;
	public libusb_transfer_status status;
	public int length;
	public int actual_length;
	public delegate* unmanaged<libusb_transfer*, void> callback;
	public nint user_data;
	public byte* buffer;
	public int num_iso_packets;
	// public libusb_iso_packet_descriptor iso_packet_desc[LIBUSB_FLEXIBLE_ARRAY];
}

public static unsafe partial class LibUsbNative
{
	[LibraryImport(LibraryName)]
	public static partial int libusb_alloc_streams(
		libusb_device_handle* dev_handle,
		uint num_streams,
		byte* endpoints,
		int num_endpoints
	);

	[LibraryImport(LibraryName)]
	public static partial int libusb_free_streams(libusb_device_handle* dev_handle, byte* endpoints, int num_endpoints);

	[LibraryImport(LibraryName)]
	public static partial byte* libusb_dev_mem_alloc(libusb_device_handle* dev_handle, nint length);

	[LibraryImport(LibraryName)]
	public static partial int libusb_dev_mem_free(libusb_device_handle* dev_handle, byte* buffer, nint length);

	[LibraryImport(LibraryName)]
	public static partial libusb_transfer* libusb_alloc_transfer(int iso_packets);

	[LibraryImport(LibraryName)]
	public static partial void libusb_free_transfer(libusb_transfer* transfer);

	[LibraryImport(LibraryName)]
	public static partial int libusb_submit_transfer(libusb_transfer* transfer);

	[LibraryImport(LibraryName)]
	public static partial int libusb_cancel_transfer(libusb_transfer* transfer);

	[LibraryImport(LibraryName)]
	public static partial void libusb_transfer_set_stream_id(libusb_transfer* transfer, uint stream_id);

	[LibraryImport(LibraryName)]
	public static partial uint libusb_transfer_get_stream_id(libusb_transfer* transfer);

	[LibraryImport(LibraryName)]
	public static partial byte* libusb_control_transfer_get_data(libusb_transfer* transfer);

	[LibraryImport(LibraryName)]
	public static partial libusb_control_setup* libusb_control_transfer_get_setup(libusb_transfer* transfer);

	[LibraryImport(LibraryName)]
	public static partial void libusb_set_iso_packet_lengths(libusb_transfer* transfer, uint length);

	[LibraryImport(LibraryName)]
	public static partial byte* libusb_get_iso_packet_buffer(libusb_transfer* transfer, uint packet);

	[LibraryImport(LibraryName)]
	public static partial byte* libusb_get_iso_packet_buffer_simple(libusb_transfer* transfer, uint packet);
}
