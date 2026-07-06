using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace InFract.Platforms.Linux.Native.LibC;

public enum dev_t : ulong;

public enum ino_t : ulong;

public enum nlink_t : ulong;

public enum mode_t : uint;

public enum uid_t : uint;

public enum gid_t : uint;

public enum pid_t : int;

public enum off_t : long;

public enum blksize_t : long;

public enum blkcnt_t : long;

public struct timespec
{
	public nint tv_sec;
	public nint tv_usec;
}

public struct stat
{
	public dev_t st_dev;
	public ino_t st_ino;
	public nlink_t st_nlink;
	public mode_t st_mode;
	public uid_t st_uid;
	public gid_t st_gid;
	private uint __pad0;
	public dev_t st_rdev;
	public off_t st_size;
	public blksize_t st_blksize;
	public blkcnt_t st_blocks;
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
