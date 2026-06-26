namespace InFract.Gamepads;

[Flags]
public enum GamepadButtons
{
	None = 0,
	South = 1 << 0,
	East = 1 << 1,
	West = 1 << 2,
	North = 1 << 3,
	Back = 1 << 4,
	Guide = 1 << 5,
	Start = 1 << 6,
	LeftStick = 1 << 7,
	RightStick = 1 << 8,
	LeftShoulder = 1 << 9,
	RightShoulder = 1 << 10,
	DpadUp = 1 << 11,
	DpadDown = 1 << 12,
	DpadLeft = 1 << 13,
	DpadRight = 1 << 14,
	LeftPaddle1 = 1 << 15,
	RightPaddle1 = 1 << 16,
	LeftPaddle2 = 1 << 17,
	RightPaddle2 = 1 << 18,
	Misc1 = 1 << 19,
	Misc2 = 1 << 20,
	Misc3 = 1 << 21,
	Misc4 = 1 << 22,
}
