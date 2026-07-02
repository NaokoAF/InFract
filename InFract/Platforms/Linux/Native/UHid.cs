using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace InFract.Platforms.Linux.Native;

public enum uhid_event_type
{
	UHID_DESTROY = 1,
	UHID_START = 2,
	UHID_STOP = 3,
	UHID_OPEN = 4,
	UHID_CLOSE = 5,
	UHID_OUTPUT = 6,
	UHID_GET_REPORT = 9,
	UHID_GET_REPORT_REPLY = 10,
	UHID_CREATE2 = 11,
	UHID_INPUT2 = 12,
	UHID_SET_REPORT = 13,
	UHID_SET_REPORT_REPLY = 14,
}

[StructLayout(LayoutKind.Explicit, Size = 4380)]
public struct uhid_event
{
	[FieldOffset(0)] public uint type;
	[FieldOffset(4)] public uhid_output_req output;
	[FieldOffset(4)] public uhid_get_report_req get_report;
	[FieldOffset(4)] public uhid_get_report_reply_req get_report_reply;
	[FieldOffset(4)] public uhid_create2_req create2;
	[FieldOffset(4)] public uhid_input2_req input2;
	[FieldOffset(4)] public uhid_set_report_req set_report;
	[FieldOffset(4)] public uhid_set_report_reply_req set_report_reply;
	[FieldOffset(4)] public uhid_start_req start;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct uhid_create2_req
{
	public _name_e__FixedBuffer name;
	public _phys_e__FixedBuffer phys;
	public _uniq_e__FixedBuffer uniq;
	public ushort rd_size;
	public ushort bus;
	public uint vendor;
	public uint product;
	public uint version;
	public uint country;
	public _rd_data_e__FixedBuffer rd_data;

	[InlineArray(128)]
	public struct _name_e__FixedBuffer
	{
		public byte e0;
	}

	[InlineArray(64)]
	public struct _phys_e__FixedBuffer
	{
		public byte e0;
	}

	[InlineArray(64)]
	public struct _uniq_e__FixedBuffer
	{
		public byte e0;
	}

	[InlineArray(4096)] // HID_MAX_DESCRIPTOR_SIZE
	public struct _rd_data_e__FixedBuffer
	{
		public byte e0;
	}
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct uhid_input2_req
{
	public ushort size;
	public _data_e__FixedBuffer data;

	[InlineArray(4096)] // UHID_DATA_MAX
	public struct _data_e__FixedBuffer
	{
		public byte e0;
	}
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct uhid_output_req
{
	public _data_e__FixedBuffer data;
	public ushort size;
	public byte rtype;

	[InlineArray(4096)] // UHID_DATA_MAX
	public struct _data_e__FixedBuffer
	{
		public byte e0;
	}
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct uhid_get_report_req
{
	public uint id;
	public byte rnum;
	public byte rtype;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct uhid_get_report_reply_req
{
	public uint id;
	public ushort err;
	public ushort size;
	public _data_e__FixedBuffer data;

	[InlineArray(4096)] // UHID_DATA_MAX
	public struct _data_e__FixedBuffer
	{
		public byte e0;
	}
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct uhid_set_report_req
{
	public uint id;
	public byte rnum;
	public byte rtype;
	public ushort size;
	public _data_e__FixedBuffer data;

	[InlineArray(4096)] // UHID_DATA_MAX
	public struct _data_e__FixedBuffer
	{
		public byte e0;
	}
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct uhid_set_report_reply_req
{
	public uint id;
	public ushort err;
}

public struct uhid_start_req
{
	public ulong dev_flags;
}
