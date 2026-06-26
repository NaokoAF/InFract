using System.Runtime.InteropServices;

namespace InFract.Emulators.Viiper.Xbox360;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ViiperXbox360Output
{
	public byte RumbleLeft;
	public byte RumbleRight;
}
