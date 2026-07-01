using InFract.Gamepads;
using InFract.Gamepads.Sony;
using InFract.Gamepads.Sony.DualShock4;
using Viiper.Client;

namespace InFract.Platforms.Windows.Viiper.DualShock4;

public class DualShock4ViiperTarget : ViiperTarget<ViiperDualShock4Input, ViiperDualShock4Output>, IDualShock4Target
{
	public SonyGyroCalibrationReport GyroCalibration { get; }

	private DualShock4Effects effects;

	internal DualShock4ViiperTarget(ViiperDevice device, GamepadDescriptor descriptor) : base(device)
	{
		// gyro calibration. scale for gamepad descriptor
		SonyGyroCalibrationReport calibration = ViiperGyroCalibration;
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
		ViiperDualShock4Input packet;
		packet.Buttons = (uint)input.Buttons;
		packet.Dpad = (DualShock4Buttons)((uint)input.Buttons & 0xF) switch
		{
			DualShock4Buttons.DpadNorth => 0b0001,
			DualShock4Buttons.DpadSouth => 0b0010,
			DualShock4Buttons.DpadWest => 0b0100,
			DualShock4Buttons.DpadEast => 0b1000,
			DualShock4Buttons.DpadNorthwest => 0b0101,
			DualShock4Buttons.DpadNortheast => 0b1001,
			DualShock4Buttons.DpadSouthwest => 0b0110,
			DualShock4Buttons.DpadSoutheast => 0b1010,
			_ => 0
		};

		// axes
		packet.LeftStickX = BitHelpers.ScaleByteToSByte(input.LeftStickX);
		packet.LeftStickY = BitHelpers.ScaleByteToSByte(input.LeftStickY);
		packet.RightStickX = BitHelpers.ScaleByteToSByte(input.RightStickX);
		packet.RightStickY = BitHelpers.ScaleByteToSByte(input.RightStickY);
		packet.LeftTrigger = input.LeftTrigger;
		packet.RightTrigger = input.RightTrigger;

		// touchpads
		packet.Touch1Down = (input.Touchpad1.Counter & 0x80) == 0;
		packet.Touch1X = (ushort)(((input.Touchpad1.XHighYLow & 0xF) << 8) | input.Touchpad1.XLow);
		packet.Touch1Y = (ushort)((input.Touchpad1.YHigh << 4) | (input.Touchpad1.XHighYLow >> 4));

		packet.Touch2Down = (input.Touchpad2.Counter & 0x80) == 0;
		packet.Touch2X = (ushort)(((input.Touchpad2.XHighYLow & 0xF) << 8) | input.Touchpad2.XLow);
		packet.Touch2Y = (ushort)((input.Touchpad2.YHigh << 4) | (input.Touchpad2.XHighYLow >> 4));

		// gyro
		packet.GyroX = input.GyroX;
		packet.GyroY = input.GyroY;
		packet.GyroZ = input.GyroZ;
		packet.AccelX = input.AccelX;
		packet.AccelY = input.AccelY;
		packet.AccelZ = input.AccelZ;

		EnqueueInput(packet);
	}

	protected override void OnOutputReceived(ViiperDualShock4Output output)
	{
		effects.RumbleLeft = output.RumbleLeft;
		effects.RumbleRight = output.RumbleRight;
		effects.LightbarRed = output.LedRed;
		effects.LightbarGreen = output.LedGreen;
		effects.LightbarBlue = output.LedBlue;
	}

	// https://github.com/Alia5/VIIPER/blob/88f66f1ed0c3716c78f810d92b1924112093f896/device/dualshock4/device.go#L422-L427
	private static readonly SonyGyroCalibrationReport ViiperGyroCalibration = new()
	{
		GyroPitchPlus = 1024,
		GyroPitchMinus = -1024,
		GyroYawPlus = 1024,
		GyroYawMinus = -1024,
		GyroRollPlus = 1024,
		GyroRollMinus = -1024,
		GyroSpeedPlus = 64,
		GyroSpeedMinus = 64,
		AccelXPlus = 8192,
		AccelXMinus = -8192,
		AccelYPlus = 8192,
		AccelYMinus = -8192,
		AccelZPlus = 8192,
		AccelZMinus = -8192,
	};
}
