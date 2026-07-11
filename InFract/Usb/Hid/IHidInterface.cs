namespace InFract.Usb.Hid;

public interface IHidInterface : IDisposable
{
	event Action<Exception?, ReadOnlySpan<byte>>? InputReceived;

	int Write(ReadOnlySpan<byte> buffer);
	void Close();
}
