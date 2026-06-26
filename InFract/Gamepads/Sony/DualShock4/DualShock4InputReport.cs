using System.Runtime.InteropServices;

namespace InFract.Gamepads.Sony.DualShock4;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 63)]
public unsafe struct DualShock4InputReport
{
	public byte LeftStickX;
	public byte LeftStickY;
	public byte RightStickX;
	public byte RightStickY;
	public DualShock4Buttons Buttons;
	public DualShock4SpecialButtons SpecialButtons;
	public byte LeftTrigger;
	public byte RightTrigger;
	public ushort SensorTimestamp;
	public byte Padding0;
	public short GyroX;
	public short GyroY;
	public short GyroZ;
	public short AccelX;
	public short AccelY;
	public short AccelZ;
	public fixed byte Padding1[5];
	public byte BatteryLevel;
	public fixed byte Padding2[4];
	public SonyTouch Touchpad1;
	public SonyTouch Touchpad2;
}
