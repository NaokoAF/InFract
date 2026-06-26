namespace InFract.Gamepads.SInput;

[Flags]
public enum SInputFeatureFlags : ushort
{
	Rumble = 0x0001,
	PlayerLed = 0x0002,
	Accel = 0x0004,
	Gyro = 0x0008,
	LeftAnalogJoystick = 0x0010,
	RightAnalogJoystick = 0x0020,
	LeftAnalogTrigger = 0x0040,
	RightAnalogTrigger = 0x0080,
	Touchpad = 0x0100,
	JoystickRgb = 0x0200,
	Handheld = 0x0400,
}
