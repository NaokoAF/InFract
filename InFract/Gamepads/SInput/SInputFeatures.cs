using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace InFract.Gamepads.SInput;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SInputFeatures
{
	public SInputFeatureFlags FeatureFlags;
	public SInputPhysicalType PhysicalType;
	public SInputFaceStyleSubProduct FaceStyleAndSubProduct;
	public ushort PollingIntervalUs;
	public ushort AccelRange;
	public ushort GyroRange;
	public SInputButtons ButtonMask;
	public byte TouchpadCount;
	public byte TouchpadFingerCount;
	public MacAddressBuffer MacAddress;

	[InlineArray(6)]
	public struct MacAddressBuffer
	{
		public byte e0;
		public Span<byte> Span => MemoryMarshal.CreateSpan(ref e0, Unsafe.SizeOf<MacAddressBuffer>());
	}
}
