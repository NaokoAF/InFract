using System.Runtime.InteropServices;

namespace InFract.Gamepads.SInput;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct SInputHaptics
{
	[FieldOffset(0)] public SInputHapticsType Type;
	[FieldOffset(1)] public HapticsPrecise Precise;
	[FieldOffset(1)] public HapticsErm Erm;

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct HapticsPrecise
	{
		public ushort LeftFrequency1;
		public ushort LeftAmplitude1;
		public ushort LeftFrequency2;
		public ushort LeftAmplitude2;
		public ushort RightFrequency1;
		public ushort RightAmplitude1;
		public ushort RightFrequency2;
		public ushort RightAmplitude2;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct HapticsErm
	{
		public byte LeftAmplitude;
		public bool LeftBrake;
		public byte RightAmplitude;
		public bool RightBrake;
	}
}
