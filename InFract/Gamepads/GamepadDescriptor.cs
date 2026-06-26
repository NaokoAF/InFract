namespace InFract.Gamepads;

public class GamepadDescriptor
{
	public required string Name { get; init; }
	public required GamepadButtons Buttons { get; init; }
	public GamepadStyle Style { get; init; } = GamepadStyle.Standard;
	public GamepadSerialNumber SerialNumber { get; init; }
	public int GyroPollingRate { get; init; }
	public int GyroRangeDps { get; init; }
	public int AccelRangeGs { get; init; }
	public int TouchpadCount { get; init; }
	public int TouchpadFingerCount { get; init; }

	public bool HasGyro => GyroRangeDps > 0;
	public bool HasAccel => AccelRangeGs > 0;
	public bool HasTouchpads => TouchpadCount > 0 && TouchpadFingerCount > 0;

	public bool HasPaddles => Buttons.HasFlag(
		GamepadButtons.LeftPaddle1 | GamepadButtons.RightPaddle1 |
		GamepadButtons.LeftPaddle2 | GamepadButtons.RightPaddle2
	);
	
	public bool HasMisc => Buttons.HasFlag(
		GamepadButtons.Misc1 | GamepadButtons.Misc2 |
		GamepadButtons.Misc3 | GamepadButtons.Misc4
	);
}
