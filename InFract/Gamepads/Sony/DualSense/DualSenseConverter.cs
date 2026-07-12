using System.Numerics;
using System.Runtime.CompilerServices;

namespace InFract.Gamepads.Sony.DualSense;

public class DualSenseConverter : IGamepadConverter
{
	private readonly IDualSenseTarget target;
	private readonly SonyTouchpadHandler touchpadHandler = new();
	private readonly Vector3 gyroBias;
	private readonly Vector3 gyroScale;
	private readonly Vector3 accelBias;
	private readonly Vector3 accelScale;
	private byte sequenceNumber;

	private const int TouchpadWidth = 1920;
	private const int TouchpadHeight = 1070;

	public DualSenseConverter(IDualSenseTarget target)
	{
		this.target = target;

		SonyGyroCalibrationReport c = target.GyroCalibration;
		Vector3 gyroPlus = new(c.GyroPitchPlus, c.GyroYawPlus, c.GyroRollPlus);
		Vector3 gyroMinus = new(c.GyroPitchMinus, c.GyroYawMinus, c.GyroRollMinus);
		Vector3 accelPlus = new(c.AccelXPlus, c.AccelYPlus, c.AccelZPlus);
		Vector3 accelMinus = new(c.AccelXMinus, c.AccelYMinus, c.AccelZMinus);
		gyroBias = new(c.GyroPitchBias, c.GyroYawBias, c.GyroRollBias);
		accelBias = accelPlus - (accelPlus - accelMinus) / 2f;
		gyroScale = Vector3.One * (c.GyroSpeedPlus + c.GyroSpeedMinus) / (gyroPlus - gyroMinus) * (32768f / 2000f);
		accelScale = Vector3.One / (accelPlus - accelMinus) * (32768f / 2f);
	}
	
	public void Update(GamepadState state)
	{
		DualSenseInputReport input = default;
		input.SequenceNumber = sequenceNumber++;

		// power
		int level = Math.Clamp((state.BatteryLevel - 5) / 10, 0, 10);
		input.BatteryStatus = state.PowerStatus switch
		{
			GamepadPowerStatus.Discharging => (byte)(level),
			GamepadPowerStatus.Charging => (byte)(0b010000 | level),
			_ => 0b100000 | 10,
		};

		input.ConnectionStatus = 0x0C;

		// buttons
		bool dpadUp = state.GetButton(GamepadButtons.DpadUp);
		bool dpadDown = state.GetButton(GamepadButtons.DpadDown);
		bool dpadLeft = state.GetButton(GamepadButtons.DpadLeft);
		bool dpadRight = state.GetButton(GamepadButtons.DpadRight);
		input.Buttons = EncodeDpad(dpadUp, dpadDown, dpadLeft, dpadRight);

		if (state.GetButton(GamepadButtons.South)) input.Buttons |= DualSenseButtons.South;
		if (state.GetButton(GamepadButtons.East)) input.Buttons |= DualSenseButtons.East;
		if (state.GetButton(GamepadButtons.West)) input.Buttons |= DualSenseButtons.West;
		if (state.GetButton(GamepadButtons.North)) input.Buttons |= DualSenseButtons.North;
		if (state.GetButton(GamepadButtons.LeftStick)) input.Buttons |= DualSenseButtons.LeftStick;
		if (state.GetButton(GamepadButtons.RightStick)) input.Buttons |= DualSenseButtons.RightStick;
		if (state.GetButton(GamepadButtons.LeftShoulder)) input.Buttons |= DualSenseButtons.LeftShoulder;
		if (state.GetButton(GamepadButtons.RightShoulder)) input.Buttons |= DualSenseButtons.RightShoulder;
		if (state.GetButton(GamepadButtons.Back)) input.Buttons |= DualSenseButtons.Create;
		if (state.GetButton(GamepadButtons.Start)) input.Buttons |= DualSenseButtons.Options;
		if (state.GetButton(GamepadButtons.Guide)) input.Buttons |= DualSenseButtons.Ps;
		if (state.GetButton(GamepadButtons.Misc1)) input.Buttons |= DualSenseButtons.Touchpad;
		if (state.GetButton(GamepadButtons.Misc2)) input.Buttons |= DualSenseButtons.MicMute;

		if (target.IsEdge)
		{
			if (state.GetButton(GamepadButtons.LeftPaddle1)) input.Buttons |= DualSenseButtons.LeftPaddle;
			if (state.GetButton(GamepadButtons.RightPaddle1)) input.Buttons |= DualSenseButtons.RightPaddle;
			if (state.GetButton(GamepadButtons.LeftPaddle2)) input.Buttons |= DualSenseButtons.LeftFunction;
			if (state.GetButton(GamepadButtons.RightPaddle2)) input.Buttons |= DualSenseButtons.RightFunction;
		}

		// axes
		input.LeftStickX = BitHelpers.ScaleShortToByte(state.LeftStickX);
		input.LeftStickY = BitHelpers.ScaleShortToByte(state.LeftStickY);
		input.RightStickX = BitHelpers.ScaleShortToByte(state.RightStickX);
		input.RightStickY = BitHelpers.ScaleShortToByte(state.RightStickY);
		input.LeftTrigger = BitHelpers.ScaleShortToByte(state.LeftTrigger);
		input.RightTrigger = BitHelpers.ScaleShortToByte(state.RightTrigger);

		if (input.LeftTrigger > 0) input.Buttons |= DualSenseButtons.LeftTrigger;
		if (input.RightTrigger > 0) input.Buttons |= DualSenseButtons.RightTrigger;

		// gyro
		input.SensorTimestamp = (uint)(state.ImuTimestampUs * 1000 / 333); // 0.33us units
		input.GyroX = ApplyCalibration(state.GyroPitch, gyroBias.X, gyroScale.X);
		input.GyroY = ApplyCalibration(state.GyroYaw, gyroBias.Y, gyroScale.Y);
		input.GyroZ = ApplyCalibration(state.GyroRoll, gyroBias.Z, gyroScale.Z);
		input.AccelX = ApplyCalibration(state.AccelX, accelBias.X, accelScale.X);
		input.AccelY = ApplyCalibration(state.AccelY, accelBias.Y, accelScale.Y);
		input.AccelZ = ApplyCalibration(state.AccelZ, accelBias.Z, accelScale.Z);

		// touchpads
		for (int i = 0; i < state.Touches.Length; i++)
		{
			GamepadTouch touch = state.Touches[i];
			touchpadHandler.Update(
				i,
				(touch.X - short.MinValue) * TouchpadWidth / ushort.MaxValue,
				(touch.Y - short.MinValue) * TouchpadHeight / ushort.MaxValue,
				touch.Down
			);
		}

		if (!target.IsEdge)
		{
			// map paddles to touchpads
			touchpadHandler.Update(
				state.Touches.Length + 0,
				TouchpadWidth / 4,
				TouchpadHeight / 4,
				state.GetButton(GamepadButtons.LeftPaddle1)
			);

			touchpadHandler.Update(
				state.Touches.Length + 1,
				TouchpadWidth - TouchpadWidth / 4,
				TouchpadHeight / 4,
				state.GetButton(GamepadButtons.RightPaddle1)
			);

			touchpadHandler.Update(
				state.Touches.Length + 2,
				TouchpadWidth / 4,
				TouchpadHeight - TouchpadHeight / 4,
				state.GetButton(GamepadButtons.LeftPaddle2)
			);

			touchpadHandler.Update(
				state.Touches.Length + 3,
				TouchpadWidth - TouchpadWidth / 4,
				TouchpadHeight - TouchpadHeight / 4,
				state.GetButton(GamepadButtons.RightPaddle2)
			);
		}

		input.Touchpad1 = touchpadHandler.Finger1;
		input.Touchpad2 = touchpadHandler.Finger2;

		target.SendInput(input);
	}

	public GamepadEffects PollEffects()
	{
		DualSenseEffects effects = target.PollEffects();
		return new()
		{
			RumbleLeft = effects.RumbleLeft,
			RumbleRight = effects.RumbleRight,
			RgbRed = effects.LightbarRed,
			RgbGreen = effects.LightbarGreen,
			RgbBlue = effects.LightbarBlue,
		};
	}

	public void Dispose() => target.Dispose();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static DualSenseButtons EncodeDpad(bool up, bool down, bool left, bool right)
	{
		if (up)
		{
			if (left) return DualSenseButtons.DpadNorthwest;
			if (right) return DualSenseButtons.DpadNortheast;
			return DualSenseButtons.DpadNorth;
		}

		if (down)
		{
			if (left) return DualSenseButtons.DpadSouthwest;
			if (right) return DualSenseButtons.DpadSoutheast;
			return DualSenseButtons.DpadSouth;
		}

		if (left) return DualSenseButtons.DpadWest;
		if (right) return DualSenseButtons.DpadEast;
		return DualSenseButtons.DpadCenter;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static short ApplyCalibration(short value, float bias, float scale) => (short)((value + bias) / scale);
}
