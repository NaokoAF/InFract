namespace InFract.Gamepads.GameSir.Cyclone2;

public enum Cyclone2Command : byte
{
	OutSetRegistry = 0x03,
	OutGetRegistry = 0x04,
	InGetRegistryResponse = 0x05,
	InSetAck = 0x06, // no data - response to 0x03 and 0x07
	OutSetProfile = 0x07,
	OutUnknown0 = 0x09, // no data - sent when app finished reading registry
	InUnknown0Response = 0x0A, // 0A 30 00 33 00 35 00 32 00 - ????
	OutGetProfile = 0x0B,
	InGetProfileResponse = 0x0C, // 0C XX
	OutSelectRgbKeyframe = 0x0D, // 0D 01 XX
	InRegistryChanged = 0x0F, // 0F XX YY YY 00 ZZ - user changed registry (M button)
	OutGetRegistry2 = 0x10, // 10 XX YY YY ZZ - called after 0x0F
	InGetRegistry2Response = 0x11, // 11 XX YY YY ZZ + data
	OutRumble = 0x20,
	OutHeartbeat = 0xF2,
}
