using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace InFract.Platforms.Linux.Native.LibC;

public struct timespec
{
	public nint tv_sec;
	public nint tv_usec;
}

public struct timeval
{
	public nint tv_sec;
	public nint tv_usec;
}

public struct stat
{
	public ulong st_dev;
	public ulong st_ino;
	public ulong st_nlink;
	public uint st_mode;
	public uint st_uid;
	public uint st_gid;
	private uint __pad0;
	public ulong st_rdev;
	public long st_size;
	public long st_blksize;
	public long st_blocks;
	public timespec st_atim;
	public timespec st_mtim;
	public timespec st_ctim;
	private ___unused_e__FixedBuffer __unused;

	[InlineArray(3)]
	private struct ___unused_e__FixedBuffer
	{
		public byte e0;
	}
}

public static unsafe partial class LibC
{
	public const int S_IFDIR = 0x0040000;
	public const int S_IFCHR = 0x0020000;
	public const int S_IFBLK = 0x0060000;
	public const int S_IFREG = 0x0100000;
	public const int S_IFIFO = 0x0010000;
	public const int S_IFLNK = 0x0120000;
	public const int S_IFSOCK = 0x0140000;
	public const int S_IFMT = 0x0170000;
	public const int S_IRWXU = 0x00700;
	public const int S_IRUSR = 0x00400;
	public const int S_IWUSR = 0x00200;
	public const int S_IXUSR = 0x00100;
	public const int S_IRWXG = 0x00070;
	public const int S_IRGRP = 0x00040;
	public const int S_IWGRP = 0x00020;
	public const int S_IXGRP = 0x00010;
	public const int S_IRWXO = 0x00007;
	public const int S_IROTH = 0x00004;
	public const int S_IWOTH = 0x00002;
	public const int S_IXOTH = 0x00001;

	[LibraryImport(LibraryName, SetLastError = true)]
	public static partial int stat(byte* path, stat* stat);

	[LibraryImport(LibraryName, SetLastError = true)]
	public static partial int fstat(int fd, stat* stat);

	[LibraryImport(LibraryName, SetLastError = true)]
	public static partial int lstat(byte* path, stat* stat);

	[LibraryImport(LibraryName, SetLastError = true)]
	public static partial int fstatat(int dirfd, byte* path, stat* stat, int flags);
}
