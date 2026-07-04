using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace InFract.Platforms.Linux.Native.LibC;

public static unsafe partial class LibC
{
	public const int _IOC_NRBITS = 8;
	public const int _IOC_TYPEBITS = 8;
	public const int _IOC_SIZEBITS = 14;
	public const int _IOC_DIRBITS = 2;
	public const int _IOC_NRMASK = (1 << _IOC_NRBITS) - 1;
	public const int _IOC_TYPEMASK = (1 << _IOC_TYPEBITS) - 1;
	public const int _IOC_SIZEMASK = (1 << _IOC_SIZEBITS) - 1;
	public const int _IOC_DIRMASK = (1 << _IOC_DIRBITS) - 1;
	public const int _IOC_NRSHIFT = 0;
	public const int _IOC_TYPESHIFT = _IOC_NRSHIFT + _IOC_NRBITS;
	public const int _IOC_SIZESHIFT = _IOC_TYPESHIFT + _IOC_TYPEBITS;
	public const int _IOC_DIRSHIFT = _IOC_SIZESHIFT + _IOC_SIZEBITS;

	public const byte _IOC_NONE = 0;
	public const byte _IOC_WRITE = 1;
	public const byte _IOC_READ = 2;

	public static int _IOC(byte dir, char type, byte nr, int size) => (
		(dir << _IOC_DIRSHIFT) |
		(type << _IOC_TYPESHIFT) |
		(nr << _IOC_NRSHIFT) |
		(size << _IOC_SIZESHIFT)
	);

	public static int _IO(char type, byte nr) => _IOC(_IOC_NONE, type, nr, 0);
	public static int _IOR<T>(char type, byte nr) => _IOC(_IOC_READ, type, nr, Unsafe.SizeOf<T>());
	public static int _IOW<T>(char type, byte nr) => _IOC(_IOC_WRITE, type, nr, Unsafe.SizeOf<T>());
	public static int _IOWR<T>(char type, byte nr) => _IOC(_IOC_READ | _IOC_WRITE, type, nr, Unsafe.SizeOf<T>());
	public static int _IOR_BAD<T>(char type, byte nr) => _IOC(_IOC_READ, type, nr, Unsafe.SizeOf<T>());
	public static int _IOW_BAD<T>(char type, byte nr) => _IOC(_IOC_WRITE, type, nr, Unsafe.SizeOf<T>());
	public static int _IOWR_BAD<T>(char type, byte nr) => _IOC(_IOC_READ | _IOC_WRITE, type, nr, Unsafe.SizeOf<T>());

	public static int _IOC_DIR(char nr) => (nr >> _IOC_DIRSHIFT) & _IOC_DIRMASK;
	public static int _IOC_TYPE(char nr) => (nr >> _IOC_TYPESHIFT) & _IOC_TYPEMASK;
	public static int _IOC_NR(char nr) => (nr >> _IOC_NRSHIFT) & _IOC_NRMASK;
	public static int _IOC_SIZE(char nr) => (nr >> _IOC_SIZESHIFT) & _IOC_SIZEMASK;

	public const int IOC_IN = _IOC_WRITE << _IOC_DIRSHIFT;
	public const int IOC_OUT = _IOC_READ << _IOC_DIRSHIFT;
	public const int IOC_INOUT = (_IOC_WRITE | _IOC_READ) << _IOC_DIRSHIFT;
	public const int IOCSIZE_MASK = _IOC_SIZEMASK << _IOC_SIZESHIFT;
	public const int IOCSIZE_SHIFT = _IOC_SIZESHIFT;

	[LibraryImport(LibraryName, SetLastError = true)]
	public static partial int ioctl(int fd, int request, void* arg);
}
