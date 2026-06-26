using System.Runtime.InteropServices;

namespace InFract.Gamepads.Sony.DualSense;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 63)]
public unsafe struct DualSenseInputReport
{
	public byte LeftStickX;
	public byte LeftStickY;
	public byte RightStickX;
	public byte RightStickY;
	public byte LeftTrigger;
	public byte RightTrigger;
	public byte SequenceNumber;
	public DualSenseButtons Buttons;
	public fixed byte Padding0[4];
	public short GyroX;
	public short GyroY;
	public short GyroZ;
	public short AccelX;
	public short AccelY;
	public short AccelZ;
	public uint SensorTimestamp;
	public byte Padding1;
	public SonyTouch Touchpad1;
	public SonyTouch Touchpad2;
	public byte Padding2;
	public byte RightAdaptiveTrigger;
	public byte LeftAdaptiveTrigger;
	public fixed byte Padding3[9];
	public byte BatteryStatus;
	public byte ConnectionStatus;
}
