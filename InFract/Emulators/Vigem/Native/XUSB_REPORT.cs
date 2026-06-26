using System.Runtime.InteropServices;

namespace InFract.Emulators.Vigem.Native;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct XUSB_REPORT
{
	public ushort Buttons;
	public byte LeftTrigger;
	public byte RightTrigger;
	public short LeftStickX;
	public short LeftStickY;
	public short RightStickX;
	public short RightStickY;
}
