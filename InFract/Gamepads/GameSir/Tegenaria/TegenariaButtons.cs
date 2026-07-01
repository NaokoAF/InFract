namespace InFract.Gamepads.GameSir.Tegenaria;

[Flags]
public enum TegenariaButtons : ushort
{
	None = 0,
	DpadUp = 1 << 0,
	DpadDown = 1 << 1,
	DpadLeft = 1 << 2,
	DpadRight = 1 << 3,
	Start = 1 << 4,
	Back = 1 << 5,
	LeftThumb = 1 << 6,
	RightThumb = 1 << 7,
	LeftShoulder = 1 << 8,
	RightShoulder = 1 << 9,
	Guide = 1 << 10,
	A = 1 << 12,
	B = 1 << 13,
	X = 1 << 14,
	Y = 1 << 15,
}
