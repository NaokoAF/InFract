using InFract.SDL3.HidApi;

namespace InFract.Drivers;

public interface IDriver
{
	bool IsSupported(in HidDeviceInfo device);
	IDriverDevice Create(HidDevice device);
}
