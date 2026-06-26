namespace InFract.Gamepads.SInput;

[Flags]
public enum SInputFaceStyleSubProduct : byte
{
	FaceStyleHid = 0x00, // ABXY
	FaceStyleXbox = 0x20, // ABXY
	FaceStyleGameCube = 0x40, // AXBY
	FaceStyleNintendo = 0x60, // BAYX
	FaceStyleSony = 0x80, // Cross, Circle, Square, Triangle
	SubProductDeveloperMode = 0x00, // expose all buttons and axes
	SubProductDynamicMode = 0x1F, // expose only enabled buttons and axes
}
