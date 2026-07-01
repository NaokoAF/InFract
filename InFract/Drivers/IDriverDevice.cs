using InFract.Gamepads;
using InFract.Usb.LibUsb;

namespace InFract.Drivers;

public interface IDriverDevice : IDisposable
{
	LibUsbDeviceHandle Device { get; }
	Gamepad Gamepad { get; }
	void Update();
	void Close();
}
