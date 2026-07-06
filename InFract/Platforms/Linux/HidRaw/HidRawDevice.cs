using System.Runtime.CompilerServices;
using InFract.Platforms.Linux.Native;
using InFract.Usb.Hid;
using static InFract.Platforms.Linux.Native.LibC.LibC;
using static InFract.Platforms.Linux.Native.HidRaw;

namespace InFract.Platforms.Linux.HidRaw;

public unsafe class HidRawDevice : IDisposable
{
	public event Action<ReadOnlySpan<byte>>? InputReceived;
	
	public int FileDescriptor => fd;

	private readonly int fd;
	private readonly byte[] readBuffer = new byte[16384];

	private HidRawDevice(int fd) => this.fd = fd;

	public static HidRawDevice Open(Utf8String path, bool blocking)
	{
		int flags = O_RDWR;
		if (!blocking) flags |= O_NONBLOCK;

		int fd = open(path, flags, 0);
		return fd >= 0 ? new(fd) : throw new ErrnoException();
	}

	public bool Poll()
	{
		int readBytes = (int)read(fd, Unsafe.AsPointer(ref readBuffer[0]), (nuint)readBuffer.Length);
		if (readBytes <= 0) return false;

		InputReceived?.Invoke(readBuffer.AsSpan(0, readBytes));
		return true;
	}

	public int Write(ReadOnlySpan<byte> buffer)
	{
		return (int)write(fd, Unsafe.AsPointer(in buffer[0]), (nuint)buffer.Length);
	}

	public int GetFeature(Span<byte> buffer)
	{
		return ioctl(fd, HIDIOCGFEATURE(buffer.Length), Unsafe.AsPointer(ref buffer[0]));
	}

	public int SetFeature(ReadOnlySpan<byte> buffer)
	{
		return ioctl(fd, HIDIOCSFEATURE(buffer.Length), Unsafe.AsPointer(in buffer[0]));
	}

	public int GetReportDescriptor(Span<byte> buffer)
	{
		// get descriptor size
		uint descSize;
		if (ioctl(fd, HIDIOCGRDESCSIZE, &descSize) < 0) throw new ErrnoException();

		if (buffer.Length < descSize) throw new ArgumentOutOfRangeException(nameof(buffer));

		// create struct and get descriptor
		Unsafe.SkipInit(out hidraw_report_descriptor desc);
		desc.size = descSize;

		if (ioctl(fd, HIDIOCGRDESC, Unsafe.AsPointer(ref desc)) < 0) throw new ErrnoException();

		// copy to output buffer
		Unsafe.CopyBlockUnaligned(ref buffer[0], ref desc.value.e0, descSize);
		return (int)descSize;
	}

	public void Close() => close(fd);
	
	public void Dispose() => close(fd);
}
