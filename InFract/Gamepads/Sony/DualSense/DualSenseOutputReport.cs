using System.Runtime.InteropServices;

namespace InFract.Gamepads.Sony.DualSense;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 47)]
internal unsafe struct DualSenseOutputReport
{
	public byte Flags1;
	public byte Flags2;
	public byte MotorRight;
	public byte MotorLeft;
	public byte HeadphoneVolume; // max 0x7F
	public byte SpeakVolume; // max 0xFF
	public byte MicVolume; // max 0x40
	public byte AudioControl1;
	public byte MuteButtonLed;
	public byte PowerSaveControl;
	public fixed byte Padding0[27];
	public byte AudioControl2;
	public byte Flags3;
	public fixed byte Padding1[2];
	public byte LightbarSetup;
	public byte LedBrightness;
	public byte PlayerLeds;
	public byte LightbarRed;
	public byte LightbarGreen;
	public byte LightbarBlue;
}
