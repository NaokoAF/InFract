namespace InFract.Gamepads.SInput;

public class SInputConverter : IGamepadConverter
{
	private readonly ISInputTarget target;

	public SInputConverter(ISInputTarget target)
	{
		this.target = target;
	}

	public void Update(Gamepad gamepad)
	{
		SInputEffects effects = target.PollEffects();
		gamepad.RumbleLeft = effects.RumbleLeft;
		gamepad.RumbleRight = effects.RumbleRight;
		gamepad.PlayerIndex = effects.PlayerLed;
		gamepad.RgbRed = effects.LedRed;
		gamepad.RgbGreen = effects.LedGreen;
		gamepad.RgbBlue = effects.LedBlue;

		// input
		SInputReport input = default;
		input.PlugStatus = gamepad.PowerStatus switch
		{
			GamepadPowerStatus.NoBattery => SInputPlugStatus.NoBattery,
			GamepadPowerStatus.Charging => SInputPlugStatus.Charging,
			GamepadPowerStatus.Charged => SInputPlugStatus.Charged,
			GamepadPowerStatus.Discharging => SInputPlugStatus.Unplugged,
			_ => SInputPlugStatus.NoBattery,
		};
		input.ChargePercent = gamepad.BatteryLevel;

		// buttons
		input.Buttons = SInputButtons.None;
		if (gamepad.GetButton(GamepadButtons.South)) input.Buttons |= SInputButtons.South;
		if (gamepad.GetButton(GamepadButtons.East)) input.Buttons |= SInputButtons.East;
		if (gamepad.GetButton(GamepadButtons.West)) input.Buttons |= SInputButtons.West;
		if (gamepad.GetButton(GamepadButtons.North)) input.Buttons |= SInputButtons.North;
		if (gamepad.GetButton(GamepadButtons.Back)) input.Buttons |= SInputButtons.Back;
		if (gamepad.GetButton(GamepadButtons.Guide)) input.Buttons |= SInputButtons.Guide;
		if (gamepad.GetButton(GamepadButtons.Start)) input.Buttons |= SInputButtons.Start;
		if (gamepad.GetButton(GamepadButtons.LeftStick)) input.Buttons |= SInputButtons.LeftStick;
		if (gamepad.GetButton(GamepadButtons.RightStick)) input.Buttons |= SInputButtons.RightStick;
		if (gamepad.GetButton(GamepadButtons.LeftShoulder)) input.Buttons |= SInputButtons.LeftShoulder;
		if (gamepad.GetButton(GamepadButtons.RightShoulder)) input.Buttons |= SInputButtons.RightShoulder;
		if (gamepad.GetButton(GamepadButtons.DpadUp)) input.Buttons |= SInputButtons.DpadUp;
		if (gamepad.GetButton(GamepadButtons.DpadDown)) input.Buttons |= SInputButtons.DpadDown;
		if (gamepad.GetButton(GamepadButtons.DpadLeft)) input.Buttons |= SInputButtons.DpadLeft;
		if (gamepad.GetButton(GamepadButtons.DpadRight)) input.Buttons |= SInputButtons.DpadRight;
		if (gamepad.GetButton(GamepadButtons.LeftPaddle1)) input.Buttons |= SInputButtons.LeftPaddle1;
		if (gamepad.GetButton(GamepadButtons.RightPaddle1)) input.Buttons |= SInputButtons.RightPaddle1;
		if (gamepad.GetButton(GamepadButtons.LeftPaddle2)) input.Buttons |= SInputButtons.LeftPaddle2;
		if (gamepad.GetButton(GamepadButtons.RightPaddle2)) input.Buttons |= SInputButtons.RightPaddle2;
		if (gamepad.GetButton(GamepadButtons.Misc1)) input.Buttons |= SInputButtons.Misc1;
		if (gamepad.GetButton(GamepadButtons.Misc2)) input.Buttons |= SInputButtons.Misc3;
		if (gamepad.GetButton(GamepadButtons.Misc3)) input.Buttons |= SInputButtons.Misc4;
		if (gamepad.GetButton(GamepadButtons.Misc4)) input.Buttons |= SInputButtons.Misc5;

		// axes
		input.LeftStickX = gamepad.GetAxis(GamepadAxis.LeftStickX);
		input.LeftStickY = gamepad.GetAxis(GamepadAxis.LeftStickY);
		input.RightStickX = gamepad.GetAxis(GamepadAxis.RightStickX);
		input.RightStickY = gamepad.GetAxis(GamepadAxis.RightStickY);
		input.LeftTrigger = gamepad.GetAxis(GamepadAxis.LeftTrigger);
		input.RightTrigger = gamepad.GetAxis(GamepadAxis.RightTrigger);

		// gyro
		input.ImuTimestampUs = (uint)gamepad.ImuTimestampUs;
		input.GyroX = (short)~gamepad.GyroPitch;
		input.GyroY = (short)~gamepad.GyroRoll;
		input.GyroZ = gamepad.GyroYaw;
		input.AccelX = (short)~gamepad.AccelX;
		input.AccelY = (short)~gamepad.AccelZ;
		input.AccelZ = gamepad.AccelY;

		// touchpads
		if (gamepad.Touches.Length >= 1)
		{
			GamepadTouch touch0 = gamepad.Touches[0];
			input.Touchpad1X = touch0.X;
			input.Touchpad1Y = touch0.Y;
			input.Touchpad1Pressure = touch0.Pressure;
		}

		if (gamepad.Touches.Length >= 2)
		{
			GamepadTouch touch1 = gamepad.Touches[1];
			input.Touchpad2X = touch1.X;
			input.Touchpad2Y = touch1.Y;
			input.Touchpad2Pressure = touch1.Pressure;
		}

		target.SendInput(input);
	}

	public void Dispose() => target.Dispose();
}
