using System.Runtime.InteropServices;

namespace InFract.Platforms.Windows.Vigem.Native;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DS4_LIGHTBAR_COLOR
{
	public byte Red;
	public byte Green;
	public byte Blue;
}
