namespace InFract.Gamepads;

public class Gamepad
{
	public GamepadDescriptor Descriptor { get; }
	public GamepadState State;
	public GamepadEffects Effects;

	public Gamepad(GamepadDescriptor descriptor)
	{
		if (descriptor.TouchpadCount * descriptor.TouchpadFingerCount > GamepadState.TouchCount)
			throw new ArgumentOutOfRangeException(nameof(descriptor));
		
		Descriptor = descriptor;

		State.PowerStatus = GamepadPowerStatus.NoBattery;
		State.BatteryLevel = 100;
		State.LeftTrigger = short.MinValue;
		State.RightTrigger = short.MinValue;
	}
}
