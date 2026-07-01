using System.Runtime.InteropServices;

namespace InFract.Gamepads.GameSir.Tegenaria;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TegenariaInputReport
{
	public TegenariaButtons Buttons;
	public byte LeftTrigger;
	public byte RightTrigger;
	public short LeftStickX;
	public short LeftStickY;
	public short RightStickX;
	public short RightStickY;
	public TegenariaSpecialButtons SpecialButtons;
}
