using System.Runtime.InteropServices;

namespace InFract.Platforms.Linux;

public class ErrnoException : Exception
{
	public int Error { get; }
	
	public ErrnoException(int error) : base(Marshal.GetPInvokeErrorMessage(error))
	{
		Error = error;
	}

	public ErrnoException(int error, string message) : base($"{message}: {Marshal.GetPInvokeErrorMessage(error)}")
	{
		Error = error;
	}

	public ErrnoException() : this(Marshal.GetLastPInvokeError())
	{
	}
	
	public ErrnoException(string message) : this(Marshal.GetLastPInvokeError(), message)
	{
	}
}
