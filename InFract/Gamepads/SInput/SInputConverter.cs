namespace InFract.Gamepads.SInput;

public class SInputConverter : IGamepadConverter
{
	private readonly ISInputTarget target;

	public SInputConverter(ISInputTarget target)
	{
		this.target = target;
	}
	
	public void Update(GamepadState state)
	{
		SInputReport input = default;
		input.PlugStatus = state.PowerStatus switch
		{
			GamepadPowerStatus.NoBattery => SInputPlugStatus.NoBattery,
			GamepadPowerStatus.Charging => SInputPlugStatus.Charging,
			GamepadPowerStatus.Charged => SInputPlugStatus.Charged,
			GamepadPowerStatus.Discharging => SInputPlugStatus.Unplugged,
			_ => SInputPlugStatus.NoBattery,
		};
		input.ChargePercent = state.BatteryLevel;

		// buttons
		input.Buttons = SInputButtons.None;
		if (state.GetButton(GamepadButtons.South)) input.Buttons |= SInputButtons.South;
		if (state.GetButton(GamepadButtons.East)) input.Buttons |= SInputButtons.East;
		if (state.GetButton(GamepadButtons.West)) input.Buttons |= SInputButtons.West;
		if (state.GetButton(GamepadButtons.North)) input.Buttons |= SInputButtons.North;
		if (state.GetButton(GamepadButtons.Back)) input.Buttons |= SInputButtons.Back;
		if (state.GetButton(GamepadButtons.Guide)) input.Buttons |= SInputButtons.Guide;
		if (state.GetButton(GamepadButtons.Start)) input.Buttons |= SInputButtons.Start;
		if (state.GetButton(GamepadButtons.LeftStick)) input.Buttons |= SInputButtons.LeftStick;
		if (state.GetButton(GamepadButtons.RightStick)) input.Buttons |= SInputButtons.RightStick;
		if (state.GetButton(GamepadButtons.LeftShoulder)) input.Buttons |= SInputButtons.LeftShoulder;
		if (state.GetButton(GamepadButtons.RightShoulder)) input.Buttons |= SInputButtons.RightShoulder;
		if (state.GetButton(GamepadButtons.DpadUp)) input.Buttons |= SInputButtons.DpadUp;
		if (state.GetButton(GamepadButtons.DpadDown)) input.Buttons |= SInputButtons.DpadDown;
		if (state.GetButton(GamepadButtons.DpadLeft)) input.Buttons |= SInputButtons.DpadLeft;
		if (state.GetButton(GamepadButtons.DpadRight)) input.Buttons |= SInputButtons.DpadRight;
		if (state.GetButton(GamepadButtons.LeftPaddle1)) input.Buttons |= SInputButtons.LeftPaddle1;
		if (state.GetButton(GamepadButtons.RightPaddle1)) input.Buttons |= SInputButtons.RightPaddle1;
		if (state.GetButton(GamepadButtons.LeftPaddle2)) input.Buttons |= SInputButtons.LeftPaddle2;
		if (state.GetButton(GamepadButtons.RightPaddle2)) input.Buttons |= SInputButtons.RightPaddle2;
		if (state.GetButton(GamepadButtons.Misc1)) input.Buttons |= SInputButtons.Misc1;
		if (state.GetButton(GamepadButtons.Misc2)) input.Buttons |= SInputButtons.Misc3;
		if (state.GetButton(GamepadButtons.Misc3)) input.Buttons |= SInputButtons.Misc4;
		if (state.GetButton(GamepadButtons.Misc4)) input.Buttons |= SInputButtons.Misc5;

		// axes
		input.LeftStickX = state.LeftStickX;
		input.LeftStickY = state.LeftStickY;
		input.RightStickX = state.RightStickX;
		input.RightStickY = state.RightStickY;
		input.LeftTrigger = state.LeftTrigger;
		input.RightTrigger = state.RightTrigger;

		// gyro
		input.ImuTimestampUs = (uint)state.ImuTimestampUs;
		input.GyroX = (short)~state.GyroPitch;
		input.GyroY = (short)~state.GyroRoll;
		input.GyroZ = state.GyroYaw;
		input.AccelX = (short)~state.AccelX;
		input.AccelY = (short)~state.AccelZ;
		input.AccelZ = state.AccelY;

		// touchpads
		GamepadTouch touch0 = state.Touches[0];
		input.Touchpad1X = touch0.X;
		input.Touchpad1Y = touch0.Y;
		input.Touchpad1Pressure = touch0.Pressure;
		
		GamepadTouch touch1 = state.Touches[1];
		input.Touchpad2X = touch1.X;
		input.Touchpad2Y = touch1.Y;
		input.Touchpad2Pressure = touch1.Pressure;

		target.SendInput(input);
	}
	
	public GamepadEffects GetEffects()
	{
		SInputEffects effects = target.PollEffects();
		return new()
		{
			RumbleLeft = effects.RumbleLeft,
			RumbleRight = effects.RumbleRight,
			PlayerIndex = effects.PlayerLed,
			RgbRed = effects.LedRed,
			RgbGreen = effects.LedGreen,
			RgbBlue = effects.LedBlue,
		};
	}

	public void Dispose() => target.Dispose();
}
