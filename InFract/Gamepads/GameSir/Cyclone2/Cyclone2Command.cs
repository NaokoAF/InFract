namespace InFract.Gamepads.GameSir.Cyclone2;

public enum Cyclone2Command : byte
{
	OutSetRegistry = 0x03,
	OutGetRegistry = 0x04,
	InGetRegistryResponse = 0x05,
	InSetAck = 0x06, // no data - response to 0x03 and 0x07
	OutSetProfile = 0x07,
	OutFirmwareInfo = 0x09, // no data
	InFirmwareInfoResponse = 0x0A, // 16 bytes in unicode. first 8 are controller version, last 8 are dongle version
	OutGetProfile = 0x0B,
	InGetProfileResponse = 0x0C, // 0C XX
	OutSelectRgbKeyframe = 0x0D, // 0D 01 XX
	InRegistryChanged = 0x0F, // 0F XX YY YY ZZ ZZ - user changed registry (M button)
	OutGetRegistry2 = 0x10, // 10 XX YY YY ZZ - called after 0x0F
	InGetRegistry2Response = 0x11, // 11 XX YY YY ZZ + data
	OutRumble = 0x20,
	OutHeartbeat = 0xF2,
}
