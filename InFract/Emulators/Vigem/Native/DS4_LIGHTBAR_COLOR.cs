using System.Runtime.InteropServices;

namespace InFract.Emulators.Vigem.Native;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DS4_LIGHTBAR_COLOR
{
	public byte Red;
	public byte Green;
	public byte Blue;
}
