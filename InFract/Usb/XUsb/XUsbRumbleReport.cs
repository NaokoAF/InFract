using System.Runtime.InteropServices;

namespace InFract.Usb.XUsb;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct XUsbRumbleReport
{
	public byte Type; // must be 0
	public byte LeftRumble;
	public byte RightRumble;
	public byte Reserved0;
	public byte Reserved1;
	public byte Reserved2;
}
