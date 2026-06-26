using System.Runtime.InteropServices;

namespace InFract.Gamepads.SInput;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 63)]
public struct SInputReport
{
	public SInputPlugStatus PlugStatus;
	public byte ChargePercent;
	public SInputButtons Buttons;
	public short LeftStickX;
	public short LeftStickY;
	public short RightStickX;
	public short RightStickY;
	public short LeftTrigger;
	public short RightTrigger;
	public uint ImuTimestampUs;
	public short AccelX;
	public short AccelY;
	public short AccelZ;
	public short GyroX;
	public short GyroY;
	public short GyroZ;
	public short Touchpad1X;
	public short Touchpad1Y;
	public ushort Touchpad1Pressure;
	public short Touchpad2X;
	public short Touchpad2Y;
	public ushort Touchpad2Pressure;
}
