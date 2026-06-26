using InFract.Gamepads;
using InFract.SDL3.HidApi;

namespace InFract.Drivers;

public interface IDriverDevice : IDisposable
{
	HidDevice Device { get; }
	Gamepad Gamepad { get; }
	void Start(CancellationToken cancellationToken);
}
