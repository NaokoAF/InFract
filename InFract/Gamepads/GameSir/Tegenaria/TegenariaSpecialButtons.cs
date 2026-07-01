namespace InFract.Gamepads.GameSir.Tegenaria;

[Flags]
public enum TegenariaSpecialButtons : byte
{
	None = 0,
	MButton = 1 << 0,
	Capture = 1 << 1,
	LeftBackButton = 1 << 2,
	RightBackButton = 1 << 3,
}
