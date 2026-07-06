namespace InFract.Usb.Hid;

public interface IHidInterface : IDisposable
{
	event Action<ReadOnlySpan<byte>>? InputReceived;

	int Write(ReadOnlySpan<byte> buffer);
	void Close();
}
