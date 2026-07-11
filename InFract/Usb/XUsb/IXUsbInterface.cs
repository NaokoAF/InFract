namespace InFract.Usb.XUsb;

public interface IXUsbInterface : IDisposable
{
	event Action<Exception?, XUsbInputReport>? InputReceived;

	bool Rumble(byte leftRumble, byte rightRumble);
	void Close();
}
