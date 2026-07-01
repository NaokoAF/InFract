using System.Runtime.CompilerServices;
using InFract.Gamepads;
using InFract.Gamepads.Sony;
using InFract.Gamepads.Sony.DualShock4;
using InFract.Platforms.Windows.Vigem.Native;
using static InFract.Platforms.Windows.Vigem.Native.VigemNative;

namespace InFract.Platforms.Windows.Vigem.Targets;

public class DualShock4VigemTarget : IDualShock4Target
{
	public SonyGyroCalibrationReport GyroCalibration { get; }

	private readonly nint client;
	private readonly nint target;
	private DualShock4Effects effects;

	internal DualShock4VigemTarget(nint client, nint target, GamepadDescriptor descriptor)
	{
		this.client = client;
		this.target = target;

		VigemException.ThrowIfError(vigem_target_add(client, target));
		VigemException.ThrowIfError(vigem_target_ds4_register_notification(client, target, OnNotified));

		// gyro calibration. scale for gamepad descriptor
		SonyGyroCalibrationReport calibration = VigemGyroCalibration;
		if (descriptor.HasGyro)
		{
			float gyroScale = 2000f / descriptor.GyroRangeDps;
			calibration.GyroSpeedPlus = (short)(gyroScale * calibration.GyroSpeedPlus);
			calibration.GyroSpeedMinus = (short)(gyroScale * calibration.GyroSpeedMinus);
		}

		if (descriptor.HasAccel)
		{
			float accelScale = descriptor.AccelRangeGs / 4f;
			calibration.AccelXPlus = (short)(accelScale * calibration.AccelXPlus);
			calibration.AccelXMinus = (short)(accelScale * calibration.AccelXMinus);
			calibration.AccelYPlus = (short)(accelScale * calibration.AccelYPlus);
			calibration.AccelYMinus = (short)(accelScale * calibration.AccelYMinus);
			calibration.AccelZPlus = (short)(accelScale * calibration.AccelZPlus);
			calibration.AccelZMinus = (short)(accelScale * calibration.AccelZMinus);
		}
		
		GyroCalibration = calibration;
	}

	public DualShock4Effects PollEffects() => effects;

	public void SendInput(in DualShock4InputReport input)
	{
		DS4_REPORT_EX report = Unsafe.As<DualShock4InputReport, DS4_REPORT_EX>(ref Unsafe.AsRef(in input));
		VigemException.ThrowIfError(vigem_target_ds4_update_ex(client, target, report));
	}

	public void Dispose()
	{
		vigem_target_ds4_unregister_notification(target);
		vigem_target_remove(client, target);
		vigem_target_free(target);
	}

	private void OnNotified(
		nint client,
		nint target,
		byte largeMotor,
		byte smallMotor,
		DS4_LIGHTBAR_COLOR color,
		nint userData
	)
	{
		effects.RumbleLeft = largeMotor;
		effects.RumbleRight = smallMotor;
		effects.LightbarRed = color.Red;
		effects.LightbarGreen = color.Green;
		effects.LightbarBlue = color.Blue;
	}

	// https://github.com/nefarius/ViGEmBus/blob/d986e1d93708ec9b11049542fa6027272cce716c/sys/Ds4Pdo.cpp#L531-L538
	private static readonly SonyGyroCalibrationReport VigemGyroCalibration = new()
	{
		GyroPitchBias = 1,
		GyroYawBias = 0,
		GyroRollBias = 0,
		GyroPitchPlus = 8839,
		GyroPitchMinus = -8837,
		GyroYawPlus = 8882,
		GyroYawMinus = -8889,
		GyroRollPlus = 8893,
		GyroRollMinus = -8893,
		GyroSpeedPlus = 540,
		GyroSpeedMinus = 540,
		AccelXPlus = 7807,
		AccelXMinus = -8402,
		AccelYPlus = 8032,
		AccelYMinus = -8116,
		AccelZPlus = 7482,
		AccelZMinus = -8506,
	};
}
