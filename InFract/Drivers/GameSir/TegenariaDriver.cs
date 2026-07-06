using System.Runtime.CompilerServices;
using InFract.Gamepads;
using InFract.Gamepads.GameSir.Tegenaria;
using InFract.Platforms;
using InFract.Usb.Hid;
using InFract.Usb.LibUsb;
using InFract.Usb.XUsb;

namespace InFract.Drivers.GameSir;

public class TegenariaDriver : IDriver
{
	private readonly IPlatform platform;

	public TegenariaDriver(IPlatform platform)
	{
		this.platform = platform;
	}
	
	public bool IsSupported(LibUsbDevice device, LibUsbDeviceDescriptor descriptor) => descriptor is
	{
		IdVendor: UsbIds.GameSirVendorId,
		IdProduct: UsbIds.GameSirTegenariaXUsbProductId or UsbIds.GameSirTegenariaHidProductId
	};

	public IDriverDevice Open(LibUsbDeviceHandle device) => new DriverDevice(device, platform);

	private class DriverDevice : IDriverDevice
	{
		public LibUsbDeviceHandle Device => device;
		public Gamepad Gamepad => gamepad;

		private readonly LibUsbDeviceHandle device;
		private readonly IHidInterface hid;
		private readonly IXUsbInterface? xusb;
		private readonly Gamepad gamepad;
		private byte rumbleLeft;
		private byte rumbleRight;

		private const int ReportSizeXUsb = 32;
		private const int ReportSizeHid = 64;
		private const byte ReportIdOutput = 0x0F;
		private const byte ReportIdInput = 0x10;

		private const byte InterfaceNumberXUsb = 0x00;
		private const byte InterfaceNumberHid = 0x01;

		private const byte CommandIdInput = 0x14;

		private static readonly GamepadDescriptor Descriptor = new()
		{
			Name = "GameSir Tegenaria Lite",
			SerialNumber = new("TEGLIT"u8),
			Style = GamepadStyle.XboxOne,
			Buttons = (
				GamepadButtons.South | GamepadButtons.East | GamepadButtons.West | GamepadButtons.North |
				GamepadButtons.Back | GamepadButtons.Guide | GamepadButtons.Start |
				GamepadButtons.DpadUp | GamepadButtons.DpadDown | GamepadButtons.DpadLeft | GamepadButtons.DpadRight |
				GamepadButtons.LeftStick | GamepadButtons.RightStick |
				GamepadButtons.LeftShoulder | GamepadButtons.RightShoulder |
				GamepadButtons.LeftPaddle1 | GamepadButtons.RightPaddle1 |
				GamepadButtons.Misc1 | GamepadButtons.Misc2
			),
		};

		public DriverDevice(LibUsbDeviceHandle device, IPlatform platform)
		{
			this.device = device;
			device.SetAutoDetachKernelDriver(true);

			hid = platform.OpenHid(device, InterfaceNumberHid);
			hid.InputReceived += OnHidInputReceived;

			try
			{
				xusb = platform.OpenXUsb(device, InterfaceNumberXUsb);
				xusb.InputReceived += OnXUsbInputReceived;
			}
			catch (Exception e)
			{
				xusb = null;
			}

			gamepad = new(Descriptor);
		}

		public void Update()
		{
			if (rumbleLeft != gamepad.RumbleLeft || rumbleRight != gamepad.RumbleRight)
			{
				if (xusb?.Rumble(gamepad.RumbleLeft, gamepad.RumbleRight) ?? false)
				{
					rumbleLeft = gamepad.RumbleLeft;
					rumbleRight = gamepad.RumbleRight;
				}
			}
		}

		private void OnXUsbInputReceived(XUsbInputReport input)
		{
			gamepad.SetButton(GamepadButtons.DpadUp, input.Buttons.HasFlag(XUsbButtons.DpadUp));
			gamepad.SetButton(GamepadButtons.DpadDown, input.Buttons.HasFlag(XUsbButtons.DpadDown));
			gamepad.SetButton(GamepadButtons.DpadLeft, input.Buttons.HasFlag(XUsbButtons.DpadLeft));
			gamepad.SetButton(GamepadButtons.DpadRight, input.Buttons.HasFlag(XUsbButtons.DpadRight));
			gamepad.SetButton(GamepadButtons.West, input.Buttons.HasFlag(XUsbButtons.X));
			gamepad.SetButton(GamepadButtons.South, input.Buttons.HasFlag(XUsbButtons.A));
			gamepad.SetButton(GamepadButtons.East, input.Buttons.HasFlag(XUsbButtons.B));
			gamepad.SetButton(GamepadButtons.North, input.Buttons.HasFlag(XUsbButtons.Y));
			gamepad.SetButton(GamepadButtons.LeftShoulder, input.Buttons.HasFlag(XUsbButtons.LeftShoulder));
			gamepad.SetButton(GamepadButtons.RightShoulder, input.Buttons.HasFlag(XUsbButtons.RightShoulder));
			gamepad.SetButton(GamepadButtons.Back, input.Buttons.HasFlag(XUsbButtons.Back));
			gamepad.SetButton(GamepadButtons.Start, input.Buttons.HasFlag(XUsbButtons.Start));
			gamepad.SetButton(GamepadButtons.Guide, input.Buttons.HasFlag(XUsbButtons.Guide));
			gamepad.SetButton(GamepadButtons.LeftStick, input.Buttons.HasFlag(XUsbButtons.LeftThumb));
			gamepad.SetButton(GamepadButtons.RightStick, input.Buttons.HasFlag(XUsbButtons.RightThumb));

			gamepad.SetAxis(GamepadAxis.LeftStickX, input.ThumbLeftX);
			gamepad.SetAxis(GamepadAxis.LeftStickY, (short)~input.ThumbLeftY);
			gamepad.SetAxis(GamepadAxis.RightStickX, input.ThumbRightX);
			gamepad.SetAxis(GamepadAxis.RightStickY, (short)~input.ThumbRightY);
			gamepad.SetAxis(GamepadAxis.LeftTrigger, BitHelpers.ScaleByteToShort(input.LeftTrigger));
			gamepad.SetAxis(GamepadAxis.RightTrigger, BitHelpers.ScaleByteToShort(input.RightTrigger));
		}

		private void OnHidInputReceived(ReadOnlySpan<byte> data)
		{
			if (data[0] != ReportIdInput && data[1] != CommandIdInput) return;

			ref TegenariaInputReport input = ref Unsafe.As<byte, TegenariaInputReport>(ref Unsafe.AsRef(in data[2]));

			if (xusb == null)
			{
				TegenariaButtons buttons = input.Buttons;
				gamepad.SetButton(GamepadButtons.DpadUp, buttons.HasFlag(TegenariaButtons.DpadUp));
				gamepad.SetButton(GamepadButtons.DpadDown, buttons.HasFlag(TegenariaButtons.DpadDown));
				gamepad.SetButton(GamepadButtons.DpadLeft, buttons.HasFlag(TegenariaButtons.DpadLeft));
				gamepad.SetButton(GamepadButtons.DpadRight, buttons.HasFlag(TegenariaButtons.DpadRight));
				gamepad.SetButton(GamepadButtons.West, buttons.HasFlag(TegenariaButtons.X));
				gamepad.SetButton(GamepadButtons.South, buttons.HasFlag(TegenariaButtons.A));
				gamepad.SetButton(GamepadButtons.East, buttons.HasFlag(TegenariaButtons.B));
				gamepad.SetButton(GamepadButtons.North, buttons.HasFlag(TegenariaButtons.Y));
				gamepad.SetButton(GamepadButtons.LeftShoulder, buttons.HasFlag(TegenariaButtons.LeftShoulder));
				gamepad.SetButton(GamepadButtons.RightShoulder, buttons.HasFlag(TegenariaButtons.RightShoulder));
				gamepad.SetButton(GamepadButtons.Back, buttons.HasFlag(TegenariaButtons.Back));
				gamepad.SetButton(GamepadButtons.Start, buttons.HasFlag(TegenariaButtons.Start));
				gamepad.SetButton(GamepadButtons.Guide, buttons.HasFlag(TegenariaButtons.Guide));
				gamepad.SetButton(GamepadButtons.LeftStick, buttons.HasFlag(TegenariaButtons.LeftThumb));
				gamepad.SetButton(GamepadButtons.RightStick, buttons.HasFlag(TegenariaButtons.RightThumb));
				
				gamepad.SetAxis(GamepadAxis.LeftStickX, input.LeftStickX);
				gamepad.SetAxis(GamepadAxis.LeftStickY, (short)~input.LeftStickY);
				gamepad.SetAxis(GamepadAxis.RightStickX, input.RightStickX);
				gamepad.SetAxis(GamepadAxis.RightStickY, (short)~input.RightStickY);
				gamepad.SetAxis(GamepadAxis.LeftTrigger, BitHelpers.ScaleByteToShort(input.LeftTrigger));
				gamepad.SetAxis(GamepadAxis.RightTrigger, BitHelpers.ScaleByteToShort(input.RightTrigger));
			}
			
			TegenariaSpecialButtons special = input.SpecialButtons;
			gamepad.SetButton(GamepadButtons.LeftPaddle1, special.HasFlag(TegenariaSpecialButtons.LeftBackButton));
			gamepad.SetButton(GamepadButtons.RightPaddle1, special.HasFlag(TegenariaSpecialButtons.RightBackButton));
			gamepad.SetButton(GamepadButtons.Misc1, special.HasFlag(TegenariaSpecialButtons.Capture));
			gamepad.SetButton(GamepadButtons.Misc2, special.HasFlag(TegenariaSpecialButtons.MButton));
		}

		public void Close()
		{
			hid.Close();
			xusb?.Close();
		}
		
		public void Dispose()
		{
			hid.Dispose();
			xusb?.Dispose();
			device.Dispose();
		}
	}
}
