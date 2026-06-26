namespace InFract.Gamepads.SInput;

[Flags]
public enum SInputButtons : uint
{
	None = 0,
	South = 1u << 0,
	East = 1u << 1,
	West = 1u << 2,
	North = 1u << 3,
	DpadUp = 1u << 4,
	DpadDown = 1u << 5,
	DpadLeft = 1u << 6,
	DpadRight = 1u << 7,
	LeftStick = 1u << 8,
	RightStick = 1u << 9,
	LeftShoulder = 1u << 10,
	RightShoulder = 1u << 11,
	LeftTrigger = 1u << 12,
	RightTrigger = 1u << 13,
	LeftPaddle1 = 1u << 14,
	RightPaddle1 = 1u << 15,
	Start = 1u << 16,
	Back = 1u << 17,
	Guide = 1u << 18,
	Misc1 = 1u << 19,
	LeftPaddle2 = 1u << 20,
	RightPaddle2 = 1u << 21,
	Touchpad1 = 1u << 22,
	Touchpad2 = 1u << 23, // always last defined misc
	Misc3 = 1u << 24,
	Misc4 = 1u << 25,
	Misc5 = 1u << 26,
	Misc6 = 1u << 27,
}
