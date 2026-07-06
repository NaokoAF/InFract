using InFract.Gamepads;

namespace InFract.Platforms;

public interface IPlatform : IDisposable
{
	ValueTask StartAsync();
	void Poll();
	IGamepadConverter CreateConverter(Gamepad gamepad);
}
