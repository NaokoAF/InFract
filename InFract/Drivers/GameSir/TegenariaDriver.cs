using System.Runtime.CompilerServices;
using InFract.Gamepads;
using InFract.Gamepads.GameSir.Tegenaria;
using InFract.Usb.Hid;
using InFract.Usb.LibUsb;
using InFract.Usb.XUsb;

namespace InFract.Drivers.GameSir;

public class TegenariaDriver : IDriver
{
	public bool IsSupported(LibUsbDevice device, LibUsbDeviceDescriptor descriptor) => descriptor is
	{
		IdVendor: UsbIds.GameSirVendorId,
		IdProduct: UsbIds.GameSirTegenariaXUsbProductId or UsbIds.GameSirTegenariaHidProductId
	};

	public IDriverDevice Open(LibUsbDeviceHandle device)
	{
		DriverDevice driver = new(device);
		driver.Open();
		return driver;
	}

	private class DriverDevice : IDriverDevice
	{
		public LibUsbDeviceHandle Device => device;
		public Gamepad Gamepad => gamepad;

		private readonly LibUsbDeviceHandle device;
		private readonly XUsbDriver xusb;
		private readonly HidDriver hid;
		private readonly Gamepad gamepad;
		private byte rumbleLeft;
		private byte rumbleRight;

		private const int ReportSizeXUsb = 32;
		private const int ReportSizeHid = 64;
		private const byte ReportIdOutput = 0x0F;
		private const byte ReportIdInput = 0x10;

		private const byte InterfaceNumberXUsb = 0x00;
		private const byte EndpointXUsbIn = 0x82;
		private const byte EndpointXUsbOut = 0x02;

		private const byte InterfaceNumberHid = 0x01;
		private const byte EndpointHidIn = 0x84;
		private const byte EndpointHidOut = 0x04;

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

		public DriverDevice(LibUsbDeviceHandle device)
		{
			this.device = device;
			xusb = new(device, InterfaceNumberXUsb, EndpointXUsbIn, EndpointXUsbOut, ReportSizeXUsb, ReportSizeXUsb);
			xusb.InputReceived += OnXUsbInputReceived;

			hid = new(device, InterfaceNumberHid, EndpointHidIn, EndpointHidOut, ReportSizeHid, ReportSizeHid);
			hid.InputReceived += OnHidInputReceived;

			gamepad = new(Descriptor);
		}

		public void Open()
		{
			device.SetAutoDetachKernelDriver(true);

			xusb.Open();
			hid.Open();
		}

		public void Update()
		{
			if (rumbleLeft != gamepad.RumbleLeft || rumbleRight != gamepad.RumbleRight)
			{
				if (xusb.Rumble(gamepad.RumbleLeft, gamepad.RumbleRight))
				{
					rumbleLeft = gamepad.RumbleLeft;
					rumbleRight = gamepad.RumbleRight;
				}
			}
		}

		private void OnXUsbInputReceived(XUsbInputReport input)
		{
			// buttons
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

			// axes
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

			TegenariaSpecialButtons special = input.SpecialButtons;
			gamepad.SetButton(GamepadButtons.LeftPaddle1, special.HasFlag(TegenariaSpecialButtons.LeftBackButton));
			gamepad.SetButton(GamepadButtons.RightPaddle1, special.HasFlag(TegenariaSpecialButtons.RightBackButton));
			gamepad.SetButton(GamepadButtons.Misc1, special.HasFlag(TegenariaSpecialButtons.Capture));
			gamepad.SetButton(GamepadButtons.Misc2, special.HasFlag(TegenariaSpecialButtons.MButton));
		}

		public void Dispose()
		{
			xusb.Dispose();
			hid.Dispose();
			device.Dispose();
		}
	}
}
