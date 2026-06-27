using static SDL.SDL3;

namespace InFract.SDL3;

public class SdlException : Exception
{
	public SdlException(string? message) : base(message)
	{
	}

	public SdlException() : base(SDL_GetError())
	{
	}
}
