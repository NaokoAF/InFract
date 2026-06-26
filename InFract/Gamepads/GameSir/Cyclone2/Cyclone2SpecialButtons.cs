namespace InFract.Gamepads.GameSir.Cyclone2;

[Flags]
public enum Cyclone2SpecialButtons : byte
{
	Guide = 1 << 0,
	Capture = 1 << 1,
	LeftBackButton = 1 << 3,
	RightBackButton = 1 << 4,
	MButton = 1 << 5,
}
