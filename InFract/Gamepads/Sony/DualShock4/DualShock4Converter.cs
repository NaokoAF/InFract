using System.Numerics;
using System.Runtime.CompilerServices;

namespace InFract.Gamepads.Sony.DualShock4;

public class DualShock4Converter : IGamepadConverter
{
	private readonly IDualShock4Target target;
	private readonly SonyTouchpadHandler touchpadHandler = new();
	private readonly Vector3 gyroBias;
	private readonly Vector3 gyroScale;
	private readonly Vector3 accelBias;
	private readonly Vector3 accelScale;
	private byte sequenceNumber;

	private const int TouchpadWidth = 1920;
	private const int TouchpadHeight = 920; // technically 944, but SDL uses 920

	public DualShock4Converter(IDualShock4Target target)
	{
		this.target = target;
		
		SonyGyroCalibrationReport c = target.GyroCalibration;
		Vector3 gyroPlus = new(c.GyroPitchPlus, c.GyroYawPlus, c.GyroRollPlus);
		Vector3 gyroMinus = new(c.GyroPitchMinus, c.GyroYawMinus, c.GyroRollMinus);
		Vector3 accelPlus = new(c.AccelXPlus, c.AccelYPlus, c.AccelZPlus);
		Vector3 accelMinus = new(c.AccelXMinus, c.AccelYMinus, c.AccelZMinus);
		gyroBias = new(c.GyroPitchBias, c.GyroYawBias, c.GyroRollBias);
		accelBias = accelPlus - (accelPlus - accelMinus) / 2f;
		gyroScale = Vector3.One * (c.GyroSpeedPlus + c.GyroSpeedMinus) / (gyroPlus - gyroMinus) * (32768f / 2048f);
		accelScale = Vector3.One / (accelPlus - accelMinus) * (32768f / 2f);
		Console.WriteLine(gyroScale);
	}

	public void Update(Gamepad gamepad)
	{
		DualShock4Effects effects = target.PollEffects();
		gamepad.RumbleLeft = effects.RumbleLeft;
		gamepad.RumbleRight = effects.RumbleRight;
		gamepad.RgbRed = effects.LightbarRed;
		gamepad.RgbGreen = effects.LightbarGreen;
		gamepad.RgbBlue = effects.LightbarBlue;
		
		// input
		DualShock4InputReport input = default;

		// power
		input.BatteryLevel = gamepad.PowerStatus switch
		{
			GamepadPowerStatus.Charging => (byte)(0b10000 | Math.Clamp((gamepad.BatteryLevel - 5) / 10, 0, 10)),
			GamepadPowerStatus.Discharging => (byte)Math.Clamp((gamepad.BatteryLevel - 5) / 10, 0, 10),
			_ => 0b10000 | 11
		};

		// buttons
		bool dpadUp = gamepad.GetButton(GamepadButtons.DpadUp);
		bool dpadDown = gamepad.GetButton(GamepadButtons.DpadDown);
		bool dpadLeft = gamepad.GetButton(GamepadButtons.DpadLeft);
		bool dpadRight = gamepad.GetButton(GamepadButtons.DpadRight);
		input.Buttons = EncodeDpad(dpadUp, dpadDown, dpadLeft, dpadRight);

		if (gamepad.GetButton(GamepadButtons.South)) input.Buttons |= DualShock4Buttons.South;
		if (gamepad.GetButton(GamepadButtons.East)) input.Buttons |= DualShock4Buttons.East;
		if (gamepad.GetButton(GamepadButtons.West)) input.Buttons |= DualShock4Buttons.West;
		if (gamepad.GetButton(GamepadButtons.North)) input.Buttons |= DualShock4Buttons.North;
		if (gamepad.GetButton(GamepadButtons.LeftStick)) input.Buttons |= DualShock4Buttons.LeftStick;
		if (gamepad.GetButton(GamepadButtons.RightStick)) input.Buttons |= DualShock4Buttons.RightStick;
		if (gamepad.GetButton(GamepadButtons.LeftShoulder)) input.Buttons |= DualShock4Buttons.LeftShoulder;
		if (gamepad.GetButton(GamepadButtons.RightShoulder)) input.Buttons |= DualShock4Buttons.RightShoulder;
		if (gamepad.GetButton(GamepadButtons.Back)) input.Buttons |= DualShock4Buttons.Share;
		if (gamepad.GetButton(GamepadButtons.Start)) input.Buttons |= DualShock4Buttons.Options;
		if (gamepad.GetButton(GamepadButtons.Guide)) input.SpecialButtons |= DualShock4SpecialButtons.Ps;
		if (gamepad.GetButton(GamepadButtons.Misc1)) input.SpecialButtons |= DualShock4SpecialButtons.Touchpad;
		
		// sequence number
		input.SpecialButtons |= (DualShock4SpecialButtons)(sequenceNumber++ << 2);

		// axes
		input.LeftStickX = BitHelpers.ScaleShortToByte(gamepad.GetAxis(GamepadAxis.LeftStickX));
		input.LeftStickY = BitHelpers.ScaleShortToByte(gamepad.GetAxis(GamepadAxis.LeftStickY));
		input.RightStickX = BitHelpers.ScaleShortToByte(gamepad.GetAxis(GamepadAxis.RightStickX));
		input.RightStickY = BitHelpers.ScaleShortToByte(gamepad.GetAxis(GamepadAxis.RightStickY));
		input.LeftTrigger = BitHelpers.ScaleShortToByte(gamepad.GetAxis(GamepadAxis.LeftTrigger));
		input.RightTrigger = BitHelpers.ScaleShortToByte(gamepad.GetAxis(GamepadAxis.RightTrigger));

		if (input.LeftTrigger > 0) input.Buttons |= DualShock4Buttons.LeftTrigger;
		if (input.RightTrigger > 0) input.Buttons |= DualShock4Buttons.RightTrigger;

		// gyro
		input.SensorTimestamp = (ushort)(gamepad.ImuTimestampUs * 1000 / 5333); // 5.33us units
		input.GyroX = ApplyCalibration(gamepad.GyroPitch, gyroBias.X, gyroScale.X);
		input.GyroY = ApplyCalibration(gamepad.GyroYaw, gyroBias.Y, gyroScale.Y);
		input.GyroZ = ApplyCalibration(gamepad.GyroRoll, gyroBias.Z, gyroScale.Z);
		input.AccelX = ApplyCalibration(gamepad.AccelX, accelBias.X, accelScale.X);
		input.AccelY = ApplyCalibration(gamepad.AccelY, accelBias.Y, accelScale.Y);
		input.AccelZ = ApplyCalibration(gamepad.AccelZ, accelBias.Z, accelScale.Z);

		// touchpads
		for (int i = 0; i < gamepad.Touches.Length; i++)
		{
			GamepadTouch touch = gamepad.Touches[i];
			touchpadHandler.Update(
				i,
				(touch.X - short.MinValue) * TouchpadWidth / ushort.MaxValue,
				(touch.Y - short.MinValue) * TouchpadHeight / ushort.MaxValue,
				touch.Down
			);
		}

		// map paddles to touchpads
		touchpadHandler.Update(
			gamepad.Touches.Length + 0,
			TouchpadWidth / 4,
			TouchpadHeight / 4,
			gamepad.GetButton(GamepadButtons.LeftPaddle1)
		);

		touchpadHandler.Update(
			gamepad.Touches.Length + 1,
			TouchpadWidth - TouchpadWidth / 4,
			TouchpadHeight / 4,
			gamepad.GetButton(GamepadButtons.RightPaddle1)
		);

		touchpadHandler.Update(
			gamepad.Touches.Length + 2,
			TouchpadWidth / 4,
			TouchpadHeight - TouchpadHeight / 4,
			gamepad.GetButton(GamepadButtons.LeftPaddle2)
		);

		touchpadHandler.Update(
			gamepad.Touches.Length + 3,
			TouchpadWidth - TouchpadWidth / 4,
			TouchpadHeight - TouchpadHeight / 4,
			gamepad.GetButton(GamepadButtons.RightPaddle2)
		);

		input.Touchpad1 = touchpadHandler.Finger1;
		input.Touchpad2 = touchpadHandler.Finger2;

		target.SendInput(input);
	}

	public void Dispose() => target.Dispose();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static DualShock4Buttons EncodeDpad(bool up, bool down, bool left, bool right)
	{
		if (up)
		{
			if (left) return DualShock4Buttons.DpadNorthwest;
			if (right) return DualShock4Buttons.DpadNortheast;
			return DualShock4Buttons.DpadNorth;
		}

		if (down)
		{
			if (left) return DualShock4Buttons.DpadSouthwest;
			if (right) return DualShock4Buttons.DpadSoutheast;
			return DualShock4Buttons.DpadSouth;
		}

		if (left) return DualShock4Buttons.DpadWest;
		if (right) return DualShock4Buttons.DpadEast;
		return DualShock4Buttons.DpadCenter;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static short ApplyCalibration(short value, float bias, float scale) => (short)((value + bias) / scale);
}
