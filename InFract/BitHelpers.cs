using System.Runtime.CompilerServices;

namespace InFract;

public static class BitHelpers
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte ScaleShortToByte(short value) => (byte)((value >> 8) - sbyte.MinValue);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static short ScaleByteToShort(byte value) => (short)(value * 257 + short.MinValue);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static sbyte ScaleByteToSByte(byte value) => (sbyte)(value + sbyte.MinValue);
}
