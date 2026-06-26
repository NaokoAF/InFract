using InFract.Gamepads;

namespace InFract.Platforms;

public interface IPlatform : IDisposable
{
	ValueTask StartAsync();
	IGamepadConverter CreateConverter(Gamepad gamepad);
}
