using InFract.Gamepads;
using InFract.Usb.Hid;
using InFract.Usb.LibUsb;
using InFract.Usb.XUsb;

namespace InFract.Platforms;

public interface IPlatform : IDisposable
{
	ValueTask StartAsync();
	void Close();
	void Poll();
	IGamepadConverter CreateConverter(Gamepad gamepad);
	IXUsbInterface OpenXUsb(LibUsbDeviceHandle device, byte interfaceNumber);
	IHidInterface OpenHid(LibUsbDeviceHandle device, byte interfaceNumber);
}
