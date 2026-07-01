using System.Runtime.InteropServices;

namespace InFract.Platforms.Windows.Viiper.Xbox360;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 20)]
public struct ViiperXbox360Input
{
	public uint Buttons;
	public byte LeftTrigger;
	public byte RightTrigger;
	public short LeftStickX;
	public short LeftStickY;
	public short RightStickX;
	public short RightStickY;
}
