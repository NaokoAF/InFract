using System.Runtime.InteropServices;

namespace InFract.Emulators.Viiper.DualSense;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ViiperDualSenseOutput
{
	public byte RumbleRight;
	public byte RumbleLeft;
	public byte LedRed;
	public byte LedGreen;
	public byte LedBlue;
	public byte PlayerLed;
}
