using System.Runtime.CompilerServices;
using static InFract.Platforms.Linux.Native.LibC.LibC;

// ReSharper disable InconsistentNaming

namespace InFract.Platforms.Linux.Native;

public struct hidraw_report_descriptor
{
	public uint size;
	public _value_e__FixedBuffer value;

	[InlineArray(HidRaw.HID_MAX_DESCRIPTOR_SIZE)]
	public struct _value_e__FixedBuffer
	{
		public byte e0;
	}
}

public struct hidraw_devinfo
{
	public uint bustype;
	public short vendor;
	public short product;
}

public static class HidRaw
{
	public const int HID_MAX_DESCRIPTOR_SIZE = 4096;
	public const int HIDRAW_FIRST_MINOR = 0;
	public const int HIDRAW_MAX_DEVICES = 64;
	public const int HIDRAW_BUFFER_SIZE = 64;

	public static readonly int HIDIOCGRDESCSIZE = _IOR<int>('H', 0x01);
	public static readonly int HIDIOCGRDESC = _IOR<hidraw_report_descriptor>('H', 0x02);
	public static readonly int HIDIOCGRAWINFO = _IOR<hidraw_devinfo>('H', 0x03);
	public static int HIDIOCGRAWNAME(int len) => _IOC(_IOC_READ, 'H', 0x04, len);
	public static int HIDIOCGRAWPHYS(int len) => _IOC(_IOC_READ, 'H', 0x05, len);
	public static int HIDIOCSFEATURE(int len) => _IOC(_IOC_WRITE | _IOC_READ, 'H', 0x06, len);
	public static int HIDIOCGFEATURE(int len) => _IOC(_IOC_WRITE | _IOC_READ, 'H', 0x07, len);
	public static int HIDIOCGRAWUNIQ(int len) => _IOC(_IOC_READ, 'H', 0x08, len);
	public static int HIDIOCSINPUT(int len) => _IOC(_IOC_WRITE | _IOC_READ, 'H', 0x09, len);
	public static int HIDIOCGINPUT(int len) => _IOC(_IOC_WRITE | _IOC_READ, 'H', 0x0A, len);
	public static int HIDIOCSOUTPUT(int len) => _IOC(_IOC_WRITE | _IOC_READ, 'H', 0x0B, len);
	public static int HIDIOCGOUTPUT(int len) => _IOC(_IOC_WRITE | _IOC_READ, 'H', 0x0C, len);
	public static readonly int HIDIOCREVOKE = _IOW<int>('H', 0x0D);
	public static readonly int HIDIOCTL_LAST = _IOC_NR((byte)HIDIOCREVOKE);
}
