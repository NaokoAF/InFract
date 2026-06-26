using System.Runtime.InteropServices;

namespace InFract.Emulators.Vigem.Native;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DS4_REPORT
{
	public byte LeftStickX;
	public byte LeftStickY;
	public byte RightStickX;
	public byte RightStickY;
	public ushort Buttons;
	public byte SpecialButtons;
	public byte LeftTrigger;
	public byte RightTrigger;
}
