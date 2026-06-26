using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace InFract.Emulators.Vigem.Native;

[InlineArray(64)]
public struct DS4_AWAIT_OUTPUT_BUFFER
{
	public byte e0;
	public Span<byte> Span => MemoryMarshal.CreateSpan(ref e0, Unsafe.SizeOf<DS4_AWAIT_OUTPUT_BUFFER>());
}
