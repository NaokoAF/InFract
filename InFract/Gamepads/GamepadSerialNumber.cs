using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace InFract.Gamepads;

[InlineArray(6)]
public struct GamepadSerialNumber
{
	public byte e0;
	public Span<byte> Span => MemoryMarshal.CreateSpan(ref e0, Unsafe.SizeOf<GamepadSerialNumber>());

	public GamepadSerialNumber(ReadOnlySpan<byte> span)
	{
		span.CopyTo(Span);
	}
}
