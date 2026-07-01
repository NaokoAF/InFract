namespace InFract.Platforms.Windows.Vigem.Native;

public class VigemException : Exception
{
	public VIGEM_ERROR Error { get; }

	public VigemException(string? message) : base(message)
	{
	}

	public VigemException(VIGEM_ERROR error) : base(error.ToString())
	{
		Error = error;
	}

	public static void ThrowIfError(VIGEM_ERROR error)
	{
		if (error != VIGEM_ERROR.VIGEM_ERROR_NONE) throw new VigemException(error);
	}
}
