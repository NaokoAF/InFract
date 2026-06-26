using System.Runtime.InteropServices;

namespace InFract.Emulators.Viiper.DualShock4;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ViiperDualShock4Output
{
	public byte RumbleRight;
	public byte RumbleLeft;
	public byte LedRed;
	public byte LedGreen;
	public byte LedBlue;
	public byte FlashOn;
	public byte FlashOff;
}
