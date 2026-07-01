using System.Runtime.InteropServices;

namespace InFract.Platforms.Windows.Viiper.DualShock4;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ViiperDualShock4Input
{
	public sbyte LeftStickX;
	public sbyte LeftStickY;
	public sbyte RightStickX;
	public sbyte RightStickY;
	public uint Buttons;
	public byte Dpad;
	public byte LeftTrigger;
	public byte RightTrigger;
	public ushort Touch1X;
	public ushort Touch1Y;
	public bool Touch1Down;
	public ushort Touch2X;
	public ushort Touch2Y;
	public bool Touch2Down;
	public short GyroX;
	public short GyroY;
	public short GyroZ;
	public short AccelX;
	public short AccelY;
	public short AccelZ;
}
