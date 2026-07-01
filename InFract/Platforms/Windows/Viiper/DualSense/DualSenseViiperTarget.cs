using InFract.Gamepads;
using InFract.Gamepads.Sony;
using InFract.Gamepads.Sony.DualSense;
using Viiper.Client;

namespace InFract.Platforms.Windows.Viiper.DualSense;

public class DualSenseViiperTarget : ViiperTarget<ViiperDualSenseInput, ViiperDualSenseOutput>, IDualSenseTarget
{
	public SonyGyroCalibrationReport GyroCalibration { get; }
	public bool IsEdge { get; }

	private DualSenseEffects effects;

	internal DualSenseViiperTarget(ViiperDevice device, bool edge, GamepadDescriptor descriptor) : base(device)
	{
		IsEdge = edge;

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

	public DualSenseEffects PollEffects() => effects;

	public void SendInput(in DualSenseInputReport input)
	{
		ViiperDualSenseInput packet;
		packet.Buttons = (uint)input.Buttons;
		packet.Dpad = (DualSenseButtons)((uint)input.Buttons & 0xF) switch
		{
			DualSenseButtons.DpadNorth => 0b0001,
			DualSenseButtons.DpadSouth => 0b0010,
			DualSenseButtons.DpadWest => 0b0100,
			DualSenseButtons.DpadEast => 0b1000,
			DualSenseButtons.DpadNorthwest => 0b0101,
			DualSenseButtons.DpadNortheast => 0b1001,
			DualSenseButtons.DpadSouthwest => 0b0110,
			DualSenseButtons.DpadSoutheast => 0b1010,
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

	protected override void OnOutputReceived(ViiperDualSenseOutput output)
	{
		effects.RumbleLeft = output.RumbleLeft;
		effects.RumbleRight = output.RumbleRight;
		effects.LightbarRed = output.LedRed;
		effects.LightbarGreen = output.LedGreen;
		effects.LightbarBlue = output.LedBlue;
		effects.LightbarBrightness = byte.MaxValue;
	}

	// https://github.com/Alia5/VIIPER/blob/88f66f1ed0c3716c78f810d92b1924112093f896/device/dualsense/device.go#L292-L297
	private static readonly SonyGyroCalibrationReport ViiperGyroCalibration = new()
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
