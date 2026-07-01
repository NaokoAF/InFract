namespace InFract.Usb.XUsb;

public enum XUsbBatteryType : byte
{
	MSBatteryPack = 0x00,
	Alkaline = 0x01,
	Reserved = 0x02,
	Unrecognized = 0x03,
	Bad = 0x0F,
}
