namespace InFract.Gamepads.GameSir.Cyclone2;

public enum Cyclone2Registry : ushort
{
	// profiles 1 to 4
	PollingRate = 0x002E, // value: 0=250hz, 1=500hz, 2=1000hz
	LeftRumbleStrength = 0x0020, // value: 0 to 100
	RightRumbleStrength = 0x0021, // value: 0 to 100
	
	// profile 0x20 (rgb)
	RgbProfile = 0x0000,
	Rgb1KeyframeCount = 0x0001, // value: 1 to 8
	Rgb1Speed = 0x0003, // value: 0 to 20 (higher = slower)
	Rgb1Brightness = 0x0004, // value: 0 to 100
	Rgb1ColorLeft = 0x0005,
	Rgb1ColorRight = 0x0008,
	Rgb1ColorUnknown = 0x000B,
	Rgb1ColorHome = 0x000E,
	Rgb1ColorDots = 0x0011,
	Rgb2KeyframeCount = 0x007D,
	Rgb3KeyframeCount = 0x00F9,
	Rgb4KeyframeCount = 0x0175,
	
	// profile 0x30 (selected profile)
	SelectedProfile = 0x0000,
}
