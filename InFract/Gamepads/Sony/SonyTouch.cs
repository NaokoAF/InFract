using System.Runtime.InteropServices;

namespace InFract.Gamepads.Sony;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SonyTouch
{
	public byte Counter;
	public byte XLow;
	public byte XHighYLow;
	public byte YHigh;
}
