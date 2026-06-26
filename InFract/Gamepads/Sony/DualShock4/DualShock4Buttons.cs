namespace InFract.Gamepads.Sony.DualShock4;

[Flags]
public enum DualShock4Buttons : ushort
{
	DpadNorth = 0x00,
	DpadNortheast = 0x01,
	DpadEast = 0x02,
	DpadSoutheast = 0x03,
	DpadSouth = 0x04,
	DpadSouthwest = 0x05,
	DpadWest = 0x06,
	DpadNorthwest = 0x07,
	DpadCenter = 0x08,
	West = 1 << 4,
	South = 1 << 5,
	East = 1 << 6,
	North = 1 << 7,
	LeftShoulder = 1 << 8,
	RightShoulder = 1 << 9,
	LeftTrigger = 1 << 10,
	RightTrigger = 1 << 11,
	Share = 1 << 12,
	Options = 1 << 13,
	LeftStick = 1 << 14,
	RightStick = 1 << 15,
}
