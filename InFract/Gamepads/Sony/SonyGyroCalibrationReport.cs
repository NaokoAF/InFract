using System.Runtime.InteropServices;

namespace InFract.Gamepads.Sony;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 36)]
public struct SonyGyroCalibrationReport
{
	public short GyroPitchBias;
	public short GyroYawBias;
	public short GyroRollBias;
	public short GyroPitchPlus;
	public short GyroPitchMinus;
	public short GyroYawPlus;
	public short GyroYawMinus;
	public short GyroRollPlus;
	public short GyroRollMinus;
	public short GyroSpeedPlus; 
	public short GyroSpeedMinus;
	public short AccelXPlus;
	public short AccelXMinus;
	public short AccelYPlus;
	public short AccelYMinus;
	public short AccelZPlus;
	public short AccelZMinus;
	public short Unknown; // speculation: a sequence number incremented per calibration

	public static SonyGyroCalibrationReport Default => new()
	{
		GyroPitchPlus = 8192,
		GyroPitchMinus = -8192,
		GyroYawPlus = 8192,
		GyroYawMinus = -8192,
		GyroRollPlus = 8192,
		GyroRollMinus = -8192,
		GyroSpeedPlus = 500,
		GyroSpeedMinus = 500,
		AccelXPlus = 8192,
		AccelXMinus = -8192,
		AccelYPlus = 8192,
		AccelYMinus = -8192,
		AccelZPlus = 8192,
		AccelZMinus = -8192,
	};
}
