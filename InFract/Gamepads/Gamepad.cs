using System.Runtime.CompilerServices;

namespace InFract.Gamepads;

public class Gamepad
{
	public GamepadDescriptor Descriptor { get; }
	public GamepadPowerStatus PowerStatus { get; set; } = GamepadPowerStatus.NoBattery;
	public byte BatteryLevel { get; set; } = 100;
	public short GyroPitch { get; set; }
	public short GyroYaw { get; set; }
	public short GyroRoll { get; set; }
	public short AccelX { get; set; }
	public short AccelY { get; set; }
	public short AccelZ { get; set; }
	public long ImuTimestampUs { get; set; }
	public byte RumbleLeft { get; set; }
	public byte RumbleRight { get; set; }
	public byte PlayerIndex { get; set; }
	public byte RgbRed { get; set; }
	public byte RgbGreen { get; set; }
	public byte RgbBlue { get; set; }
	public Span<GamepadTouch> Touches => touches;

	private GamepadButtons buttons;
	private readonly short[] axes = new short[6];
	private readonly GamepadTouch[] touches;

	public Gamepad(GamepadDescriptor descriptor)
	{
		Descriptor = descriptor;
		touches = new GamepadTouch[descriptor.TouchpadCount * descriptor.TouchpadFingerCount];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool GetButton(GamepadButtons button) => buttons.HasFlag(button);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetButton(GamepadButtons button, bool down) => buttons = down ? (buttons | button) : (buttons & ~button);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public short GetAxis(GamepadAxis axis) => axes[(int)axis];

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetAxis(GamepadAxis axis, short value) => axes[(int)axis] = value;
}
