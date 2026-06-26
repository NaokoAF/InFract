using System.Runtime.InteropServices;

namespace InFract.Gamepads.Sony.DualShock4;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DualShock4OutputReport
{
	public DualShock4EffectMask Effects;
	public ushort Unknown1;
	public byte RumbleRight;
	public byte RumbleLeft;
	public byte LedRed;
	public byte LedGreen;
	public byte LedBlue;
	public byte LedDelayOn;
	public byte LedDelayOff;
	public ulong Unknown2;
	public byte VolumeLeft;
	public byte VolumeRight;
	public byte VolumeMic;
	public byte VolumeSpeaker;
}
