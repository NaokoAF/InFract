using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace InFract.Platforms.Linux.Native.LibC;

public struct pollfd
{
	public int fd;
	public short events;
	public short revents;
}

public static unsafe partial class LibC
{
	public const int POLLIN = 1;
	public const int POLLPRI = 2;
	public const int POLLOUT = 4;
	public const int POLLERR = 8;
	public const int POLLHUP = 16;
	public const int POLLNVAL = 32;
	public const int POLLMSG = 1024;
	public const int POLLRDHUP = 8192;

	[LibraryImport(LibraryName, SetLastError = true)]
	public static partial int poll(pollfd* fds, ulong nfds, int timeout);
}
