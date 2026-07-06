using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming

namespace InFract.Platforms.Linux.Native.LibSystemd;

[InlineArray(16)]
public struct sd_id128_t
{
	public byte e0;
}

public static unsafe partial class LibSystemd
{
	private const string LibraryName = "libsystemd.so.0";
}
