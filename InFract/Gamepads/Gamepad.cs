namespace InFract.Gamepads;

public class Gamepad
{
	public GamepadDescriptor Descriptor { get; }
	public GamepadEffects Effects;
	
	public event Action<GamepadState>? InputReceived;

	public Gamepad(GamepadDescriptor descriptor)
	{
		if (descriptor.TouchpadCount * descriptor.TouchpadFingerCount > GamepadState.TouchCount)
			throw new ArgumentOutOfRangeException(nameof(descriptor));
		
		Descriptor = descriptor;
	}

	public void PushInput(GamepadState state) => InputReceived?.Invoke(state);
}
