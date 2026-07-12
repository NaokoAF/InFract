using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace InFract.Gamepads;

public struct GamepadState
{
	public uint SequenceNumber;
	public GamepadPowerStatus PowerStatus;
	public byte BatteryLevel;
	public GamepadButtons Buttons;
	public short LeftStickX;
	public short LeftStickY;
	public short RightStickX;
	public short RightStickY;
	public short LeftTrigger;
	public short RightTrigger;
	public short GyroPitch;
	public short GyroYaw;
	public short GyroRoll;
	public short AccelX;
	public short AccelY;
	public short AccelZ;
	public long ImuTimestampUs;
	public Span<GamepadTouch> Touches => MemoryMarshal.CreateSpan(ref touches.e0, TouchCount);

	private TouchBuffer touches;

	public const int TouchCount = 2;
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool GetButton(GamepadButtons button) => Buttons.HasFlag(button);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetButton(GamepadButtons button, bool down) => Buttons = down ? (Buttons | button) : (Buttons & ~button);
	
	[InlineArray(TouchCount)]
	private struct TouchBuffer
	{
		public GamepadTouch e0;
	}
}
