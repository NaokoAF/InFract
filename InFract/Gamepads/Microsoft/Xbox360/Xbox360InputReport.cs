using System.Runtime.InteropServices;

namespace InFract.Gamepads.Microsoft.Xbox360;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 12)]
public struct Xbox360InputReport
{
	public Xbox360Buttons Buttons;
	public byte LeftTrigger;
	public byte RightTrigger;
	public short LeftStickX;
	public short LeftStickY;
	public short RightStickX;
	public short RightStickY;
}
