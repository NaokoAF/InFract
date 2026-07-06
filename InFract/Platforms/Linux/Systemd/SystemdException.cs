namespace InFract.Platforms.Linux.Systemd;

public unsafe class SystemdException : Exception
{
	public SystemdException(string? message = null) : base(message)
	{
	}

	public static int ThrowIfError(int error, string? message = null)
	{
		return error >= 0 ? error : throw new SystemdException(message);
	}
	
	public static T* ThrowIfError<T>(T* pointer, string? message = null) where T : unmanaged
	{
		return pointer != null ? pointer : throw new SystemdException(message);
	}
}
