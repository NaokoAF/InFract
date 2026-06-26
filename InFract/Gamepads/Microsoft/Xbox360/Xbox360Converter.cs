namespace InFract.Gamepads.Microsoft.Xbox360;

public class Xbox360Converter : IGamepadConverter
{
	private readonly IXbox360Target target;

	public Xbox360Converter(IXbox360Target target)
	{
		this.target = target;
	}

	public void Update(Gamepad gamepad)
	{
		Xbox360Effects effects = target.PollEffects();
		gamepad.RumbleLeft = effects.RumbleLeft;
		gamepad.RumbleRight = effects.RumbleRight;
		gamepad.PlayerIndex = effects.PlayerLed;

		// input
		Xbox360InputReport input = default;
		if (gamepad.GetButton(GamepadButtons.DpadUp)) input.Buttons |= Xbox360Buttons.DpadUp;
		if (gamepad.GetButton(GamepadButtons.DpadDown)) input.Buttons |= Xbox360Buttons.DpadDown;
		if (gamepad.GetButton(GamepadButtons.DpadLeft)) input.Buttons |= Xbox360Buttons.DpadLeft;
		if (gamepad.GetButton(GamepadButtons.DpadRight)) input.Buttons |= Xbox360Buttons.DpadRight;
		if (gamepad.GetButton(GamepadButtons.Start)) input.Buttons |= Xbox360Buttons.Start;
		if (gamepad.GetButton(GamepadButtons.Back)) input.Buttons |= Xbox360Buttons.Back;
		if (gamepad.GetButton(GamepadButtons.LeftStick)) input.Buttons |= Xbox360Buttons.LeftThumb;
		if (gamepad.GetButton(GamepadButtons.RightStick)) input.Buttons |= Xbox360Buttons.RightThumb;
		if (gamepad.GetButton(GamepadButtons.LeftShoulder)) input.Buttons |= Xbox360Buttons.LeftShoulder;
		if (gamepad.GetButton(GamepadButtons.RightShoulder)) input.Buttons |= Xbox360Buttons.RightShoulder;
		if (gamepad.GetButton(GamepadButtons.Guide)) input.Buttons |= Xbox360Buttons.Guide;
		if (gamepad.GetButton(GamepadButtons.South)) input.Buttons |= Xbox360Buttons.A;
		if (gamepad.GetButton(GamepadButtons.East)) input.Buttons |= Xbox360Buttons.B;
		if (gamepad.GetButton(GamepadButtons.West)) input.Buttons |= Xbox360Buttons.X;
		if (gamepad.GetButton(GamepadButtons.North)) input.Buttons |= Xbox360Buttons.Y;

		input.LeftStickX = gamepad.GetAxis(GamepadAxis.LeftStickX);
		input.LeftStickY = gamepad.GetAxis(GamepadAxis.LeftStickY);
		input.RightStickX = gamepad.GetAxis(GamepadAxis.RightStickX);
		input.RightStickY = gamepad.GetAxis(GamepadAxis.RightStickY);
		input.LeftTrigger = BitHelpers.ScaleShortToByte(gamepad.GetAxis(GamepadAxis.LeftTrigger));
		input.RightTrigger = BitHelpers.ScaleShortToByte(gamepad.GetAxis(GamepadAxis.RightTrigger));

		target.SendInput(input);
	}

	public void Dispose() => target.Dispose();
}
