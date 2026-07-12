using System.Collections.Concurrent;
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
		private readonly ConcurrentBag<Exception> inputErrors = new();
		private GamepadEffects prevEffects;

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
			if (!inputErrors.IsEmpty) throw new AggregateException(inputErrors);
			
			GamepadEffects effects = gamepad.Effects;
			if (prevEffects.RumbleLeft != effects.RumbleLeft || prevEffects.RumbleRight != effects.RumbleRight)
			{
				if (xusb?.Rumble(effects.RumbleLeft, effects.RumbleRight) ?? false)
				{
					prevEffects.RumbleLeft = effects.RumbleLeft;
					prevEffects.RumbleRight = effects.RumbleRight;
				}
			}

			prevEffects = effects;
		}

		private void OnXUsbInputReceived(Exception? exception, XUsbInputReport input)
		{
			if (exception != null)
			{
				inputErrors.Add(exception);
				return;
			}
			
			ref GamepadState state = ref gamepad.State;
			state.SetButton(GamepadButtons.DpadUp, input.Buttons.HasFlag(XUsbButtons.DpadUp));
			state.SetButton(GamepadButtons.DpadDown, input.Buttons.HasFlag(XUsbButtons.DpadDown));
			state.SetButton(GamepadButtons.DpadLeft, input.Buttons.HasFlag(XUsbButtons.DpadLeft));
			state.SetButton(GamepadButtons.DpadRight, input.Buttons.HasFlag(XUsbButtons.DpadRight));
			state.SetButton(GamepadButtons.West, input.Buttons.HasFlag(XUsbButtons.X));
			state.SetButton(GamepadButtons.South, input.Buttons.HasFlag(XUsbButtons.A));
			state.SetButton(GamepadButtons.East, input.Buttons.HasFlag(XUsbButtons.B));
			state.SetButton(GamepadButtons.North, input.Buttons.HasFlag(XUsbButtons.Y));
			state.SetButton(GamepadButtons.LeftShoulder, input.Buttons.HasFlag(XUsbButtons.LeftShoulder));
			state.SetButton(GamepadButtons.RightShoulder, input.Buttons.HasFlag(XUsbButtons.RightShoulder));
			state.SetButton(GamepadButtons.Back, input.Buttons.HasFlag(XUsbButtons.Back));
			state.SetButton(GamepadButtons.Start, input.Buttons.HasFlag(XUsbButtons.Start));
			state.SetButton(GamepadButtons.Guide, input.Buttons.HasFlag(XUsbButtons.Guide));
			state.SetButton(GamepadButtons.LeftStick, input.Buttons.HasFlag(XUsbButtons.LeftThumb));
			state.SetButton(GamepadButtons.RightStick, input.Buttons.HasFlag(XUsbButtons.RightThumb));

			state.LeftStickX = input.ThumbLeftX;
			state.LeftStickY = (short)~input.ThumbLeftY;
			state.RightStickX = input.ThumbRightX;
			state.RightStickY = (short)~input.ThumbRightY;
			state.LeftTrigger = BitHelpers.ScaleByteToShort(input.LeftTrigger);
			state.RightTrigger = BitHelpers.ScaleByteToShort(input.RightTrigger);
		}

		private void OnHidInputReceived(Exception? exception, ReadOnlySpan<byte> data)
		{
			if (exception != null)
			{
				inputErrors.Add(exception);
				return;
			}
			
			if (data[0] != ReportIdInput && data[1] != CommandIdInput) return;

			ref GamepadState state = ref gamepad.State;
			ref TegenariaInputReport input = ref Unsafe.As<byte, TegenariaInputReport>(ref Unsafe.AsRef(in data[2]));

			if (xusb == null)
			{
				TegenariaButtons buttons = input.Buttons;
				state.SetButton(GamepadButtons.DpadUp, buttons.HasFlag(TegenariaButtons.DpadUp));
				state.SetButton(GamepadButtons.DpadDown, buttons.HasFlag(TegenariaButtons.DpadDown));
				state.SetButton(GamepadButtons.DpadLeft, buttons.HasFlag(TegenariaButtons.DpadLeft));
				state.SetButton(GamepadButtons.DpadRight, buttons.HasFlag(TegenariaButtons.DpadRight));
				state.SetButton(GamepadButtons.West, buttons.HasFlag(TegenariaButtons.X));
				state.SetButton(GamepadButtons.South, buttons.HasFlag(TegenariaButtons.A));
				state.SetButton(GamepadButtons.East, buttons.HasFlag(TegenariaButtons.B));
				state.SetButton(GamepadButtons.North, buttons.HasFlag(TegenariaButtons.Y));
				state.SetButton(GamepadButtons.LeftShoulder, buttons.HasFlag(TegenariaButtons.LeftShoulder));
				state.SetButton(GamepadButtons.RightShoulder, buttons.HasFlag(TegenariaButtons.RightShoulder));
				state.SetButton(GamepadButtons.Back, buttons.HasFlag(TegenariaButtons.Back));
				state.SetButton(GamepadButtons.Start, buttons.HasFlag(TegenariaButtons.Start));
				state.SetButton(GamepadButtons.Guide, buttons.HasFlag(TegenariaButtons.Guide));
				state.SetButton(GamepadButtons.LeftStick, buttons.HasFlag(TegenariaButtons.LeftThumb));
				state.SetButton(GamepadButtons.RightStick, buttons.HasFlag(TegenariaButtons.RightThumb));
				
				state.LeftStickX = input.LeftStickX;
				state.LeftStickY = (short)~input.LeftStickY;
				state.RightStickX = input.RightStickX;
				state.RightStickY = (short)~input.RightStickY;
				state.LeftTrigger = BitHelpers.ScaleByteToShort(input.LeftTrigger);
				state.RightTrigger = BitHelpers.ScaleByteToShort(input.RightTrigger);
			}
			
			TegenariaSpecialButtons special = input.SpecialButtons;
			state.SetButton(GamepadButtons.LeftPaddle1, special.HasFlag(TegenariaSpecialButtons.LeftBackButton));
			state.SetButton(GamepadButtons.RightPaddle1, special.HasFlag(TegenariaSpecialButtons.RightBackButton));
			state.SetButton(GamepadButtons.Misc1, special.HasFlag(TegenariaSpecialButtons.Capture));
			state.SetButton(GamepadButtons.Misc2, special.HasFlag(TegenariaSpecialButtons.MButton));
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
