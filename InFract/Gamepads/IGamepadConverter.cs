namespace InFract.Gamepads;

public interface IGamepadConverter : IDisposable
{
	void Update(GamepadState state);
	GamepadEffects PollEffects();
	void Close();
}
