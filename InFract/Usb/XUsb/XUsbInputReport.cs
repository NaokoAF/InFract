using System.Runtime.InteropServices;

namespace InFract.Usb.XUsb;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct XUsbInputReport
{
	public XUsbButtons Buttons;
	public byte LeftTrigger;
	public byte RightTrigger;
	public short ThumbLeftX;
	public short ThumbLeftY;
	public short ThumbRightX;
	public short ThumbRightY;
	public byte Reserved0;
	public byte Reserved1;
	public byte Reserved2;
	public byte Reserved3;
	public byte Reserved4;
	public byte Reserved5;
}
