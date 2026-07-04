using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace InFract.Platforms.Linux.Native.LibC;

public static unsafe partial class LibC
{
	private const string LibraryName = "libc.so.6";

	public const int O_ACCMODE = 3;
	public const int O_RDONLY = 0;
	public const int O_WRONLY = 1 << 0;
	public const int O_RDWR = 1 << 1;
	public const int O_CREAT = 1 << 6;
	public const int O_EXCL = 1 << 7;
	public const int O_NOCTTY = 1 << 8;
	public const int O_TRUNC = 1 << 9;
	public const int O_APPEND = 1 << 10;
	public const int O_NONBLOCK = 1 << 11;
	public const int O_DSYNC = 1 << 12;
	public const int FASYNC = 1 << 13;
	public const int O_DIRECT = 1 << 14;
	public const int O_LARGEFILE = 1 << 15;
	public const int O_DIRECTORY = 1 << 16;
	public const int O_NOFOLLOW = 1 << 17;
	public const int O_NOATIME = 1 << 18;
	public const int O_CLOEXEC = 1 << 19;

	[LibraryImport(LibraryName, SetLastError = true)]
	public static partial int creat(byte* pathname, int mode);

	[LibraryImport(LibraryName, SetLastError = true)]
	public static partial int open(byte* pathname, int flags, int mode);

	[LibraryImport(LibraryName, SetLastError = true)]
	public static partial int openat(int dirfd, byte* pathname, int flags, int mode);

	[LibraryImport(LibraryName, SetLastError = true)]
	public static partial int fcntl(int fd, int cmd, void* arg);

	[LibraryImport(LibraryName, SetLastError = true)]
	public static partial int close(int fd);

	[LibraryImport(LibraryName, SetLastError = true)]
	public static partial nint read(int fd, void* buf, nuint count);

	[LibraryImport(LibraryName, SetLastError = true)]
	public static partial nint write(int fd, void* buf, nuint count);
}
