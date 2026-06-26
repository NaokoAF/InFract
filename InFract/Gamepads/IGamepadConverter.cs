namespace InFract.Gamepads;

public interface IGamepadConverter : IDisposable
{
	void Update(Gamepad gamepad);
}
