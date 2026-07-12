namespace InFract.Gamepads.Microsoft.Xbox360;

public class Xbox360Converter : IGamepadConverter
{
	private readonly IXbox360Target target;

	public Xbox360Converter(IXbox360Target target)
	{
		this.target = target;
	}

	public void Update(GamepadState state)
	{
		Xbox360InputReport input = default;
		if (state.GetButton(GamepadButtons.DpadUp)) input.Buttons |= Xbox360Buttons.DpadUp;
		if (state.GetButton(GamepadButtons.DpadDown)) input.Buttons |= Xbox360Buttons.DpadDown;
		if (state.GetButton(GamepadButtons.DpadLeft)) input.Buttons |= Xbox360Buttons.DpadLeft;
		if (state.GetButton(GamepadButtons.DpadRight)) input.Buttons |= Xbox360Buttons.DpadRight;
		if (state.GetButton(GamepadButtons.Start)) input.Buttons |= Xbox360Buttons.Start;
		if (state.GetButton(GamepadButtons.Back)) input.Buttons |= Xbox360Buttons.Back;
		if (state.GetButton(GamepadButtons.LeftStick)) input.Buttons |= Xbox360Buttons.LeftThumb;
		if (state.GetButton(GamepadButtons.RightStick)) input.Buttons |= Xbox360Buttons.RightThumb;
		if (state.GetButton(GamepadButtons.LeftShoulder)) input.Buttons |= Xbox360Buttons.LeftShoulder;
		if (state.GetButton(GamepadButtons.RightShoulder)) input.Buttons |= Xbox360Buttons.RightShoulder;
		if (state.GetButton(GamepadButtons.Guide)) input.Buttons |= Xbox360Buttons.Guide;
		if (state.GetButton(GamepadButtons.South)) input.Buttons |= Xbox360Buttons.A;
		if (state.GetButton(GamepadButtons.East)) input.Buttons |= Xbox360Buttons.B;
		if (state.GetButton(GamepadButtons.West)) input.Buttons |= Xbox360Buttons.X;
		if (state.GetButton(GamepadButtons.North)) input.Buttons |= Xbox360Buttons.Y;

		input.LeftStickX = state.LeftStickX;
		input.LeftStickY = state.LeftStickY;
		input.RightStickX = state.RightStickX;
		input.RightStickY = state.RightStickY;
		input.LeftTrigger = BitHelpers.ScaleShortToByte(state.LeftTrigger);
		input.RightTrigger = BitHelpers.ScaleShortToByte(state.RightTrigger);

		target.SendInput(input);
	}

	public GamepadEffects PollEffects()
	{
		Xbox360Effects effects = target.PollEffects();
		return new()
		{
			RumbleLeft = effects.RumbleLeft,
			RumbleRight = effects.RumbleRight,
			PlayerIndex = effects.PlayerLed,
		};
	}

	public void Close() => target.Close();
	public void Dispose() => target.Dispose();
}
