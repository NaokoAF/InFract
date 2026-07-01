using InFract.Usb.LibUsb;

namespace InFract.Drivers;

public interface IDriver
{
	bool IsSupported(LibUsbDevice device, LibUsbDeviceDescriptor descriptor);
	IDriverDevice Open(LibUsbDeviceHandle device);
}
