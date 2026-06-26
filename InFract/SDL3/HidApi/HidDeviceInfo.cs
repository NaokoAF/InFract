namespace InFract.SDL3.HidApi;

public record struct HidDeviceInfo
{
	public required string Path;
	public ushort VendorId;
	public ushort ProductId;
	public string? SerialNumber;
	public ushort ReleaseNumber;
	public string? ManufacturerString;
	public string? ProductString;
	public ushort UsagePage;
	public ushort Usage;
	public int InterfaceNumber;
	public int InterfaceClass;
	public int InterfaceSubClass;
	public int InterfaceProtocol;
}
