using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace InFract.Platforms.Linux.UHid;

[StructLayout(LayoutKind.Explicit)] // Size = 4380
public struct UHidEvent
{
	[FieldOffset(0)] public UHidEventType Type;
	[FieldOffset(4)] public UHidCreate2 Create2;
	[FieldOffset(4)] public UHidInput2 Input2;
	[FieldOffset(4)] public UHidOutput Output;
	[FieldOffset(4)] public UHidGetReport GetReport;
	[FieldOffset(4)] public UHidGetReportReply GetReportReply;
	[FieldOffset(4)] public UHidSetReport SetReport;
	[FieldOffset(4)] public UHidSetReportReply SetReportReply;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UHidCreate2
{
	public NameBuffer Name;
	public PhysUniqBuffer Phys;
	public PhysUniqBuffer Uniq;
	public ushort DescriptorSize;
	public UHidBusType Bus;
	public uint Vendor;
	public uint Product;
	public uint Version;
	public uint Country;
	public UHidDataBuffer Descriptor;

	[InlineArray(128)]
	public struct NameBuffer
	{
		public byte e0;
		public Span<byte> Span => MemoryMarshal.CreateSpan(ref e0, Unsafe.SizeOf<NameBuffer>());
	}

	[InlineArray(64)]
	public struct PhysUniqBuffer
	{
		public byte e0;
		public Span<byte> Span => MemoryMarshal.CreateSpan(ref e0, Unsafe.SizeOf<PhysUniqBuffer>());
	}
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UHidInput2
{
	public ushort Size;
	public UHidDataBuffer Data;

	public Span<byte> DataSpan => MemoryMarshal.CreateSpan(ref Data.e0, Size);
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UHidOutput
{
	public UHidDataBuffer Data;
	public ushort Size;
	public UHidReportType ReportType;

	public Span<byte> DataSpan => MemoryMarshal.CreateSpan(ref Data.e0, Size);
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UHidGetReport
{
	public uint Id;
	public byte ReportNum;
	public UHidReportType ReportType;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UHidGetReportReply
{
	public uint Id;
	public ushort Err;
	public ushort Size;
	public UHidDataBuffer Data;

	public Span<byte> DataSpan => MemoryMarshal.CreateSpan(ref Data.e0, Size);
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UHidSetReport
{
	public uint Id;
	public byte ReportNum;
	public UHidReportType ReportType;
	public ushort Size;
	public UHidDataBuffer Data;

	public Span<byte> DataSpan => MemoryMarshal.CreateSpan(ref Data.e0, Size);
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UHidSetReportReply
{
	public uint Id;
	public ushort Err;
}

[InlineArray(4096)]
public struct UHidDataBuffer
{
	public byte e0;
	public Span<byte> Span => MemoryMarshal.CreateSpan(ref e0, Unsafe.SizeOf<UHidDataBuffer>());
}
