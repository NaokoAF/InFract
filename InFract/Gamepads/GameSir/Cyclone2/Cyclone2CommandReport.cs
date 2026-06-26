using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace InFract.Gamepads.GameSir.Cyclone2;

[StructLayout(LayoutKind.Explicit, Size = 63)]
public struct Cyclone2CommandReport
{
	[FieldOffset(0)] public Cyclone2Command Command;
	[FieldOffset(0)] public Cyclone2SetRegistryCommand SetRegistry;
	[FieldOffset(0)] public Cyclone2SetProfileCommand SetProfile;
	[FieldOffset(0)] public Cyclone2RumbleCommand Rumble;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Cyclone2SetRegistryCommand()
{
	public Cyclone2Command Command = Cyclone2Command.OutSetRegistry;
	public byte Profile;
	public byte RegistryHigh;
	public byte RegistryLow;
	public byte Length;
	public ValueBuffer Value;
	public Span<byte> ValueSpan => MemoryMarshal.CreateSpan(ref Value.e0, Length);

	[InlineArray(58)]
	public struct ValueBuffer
	{
		public byte e0;
		public Span<byte> Span => MemoryMarshal.CreateSpan(ref e0, Unsafe.SizeOf<ValueBuffer>());
	}
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Cyclone2SetProfileCommand()
{
	public Cyclone2Command Command = Cyclone2Command.OutSetProfile;
	public byte Profile;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Cyclone2RumbleCommand()
{
	public Cyclone2Command Command = Cyclone2Command.OutRumble;
	public byte Unknown0 = 0x66;
	public byte Unknown1 = 0x55;
	public byte RumbleLeft;
	public byte RumbleRight;
}
