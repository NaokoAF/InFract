using System.Runtime.InteropServices;

namespace InFract.Gamepads.GameSir.Cyclone2;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 63)]
public unsafe struct Cyclone2InputReport
{
	public byte LeftStickX; // 0
	public byte LeftStickY; // 1
	public byte RightStickX; // 2
	public byte RightStickY; // 3
	public Cyclone2Buttons Buttons; // 4-5
	public Cyclone2SpecialButtons SpecialButtons; // 6
	public byte LeftTrigger; // 7
	public byte RightTrigger; // 8
	public ushort Timestamp; // 9-10
	public byte Padding0; // 11
	public short GyroX; // 12-13
	public short GyroY; // 14-15
	public short GyroZ; // 16-17
	public short AccelX; // 18-19
	public short AccelY; // 20-21
	public short AccelZ; // 22-23
	public fixed byte Padding1[10]; // 24-33
	public TouchpadData TouchpadData1; // 34-37
	public TouchpadData TouchpadData2; // 38-41
	public fixed byte Padding2[11]; // 42-52
	public byte RawLeftStickX; // 53
	public byte RawLeftStickY; // 54
	public byte RawRightStickX; // 55
	public byte RawRightStickY; // 56
	public Cyclone2Buttons RawButtons; // 57-58
	public Cyclone2SpecialButtons RawSpecialButtons; // 59
	public byte RawLeftTrigger; // 60
	public byte RawRightTrigger; // 61
	public byte Padding3; // 62

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct TouchpadData
	{
		public byte Counter;
		public byte XLow;
		public byte XHighYLow;
		public byte YHigh;
	}
}
