using System.Diagnostics;
using System.Runtime.CompilerServices;
using InFract.Gamepads;
using InFract.Gamepads.GameSir.Cyclone2;
using InFract.Platforms;
using InFract.Usb.Hid;
using InFract.Usb.LibUsb;
using InFract.Usb.XUsb;

namespace InFract.Drivers.GameSir;

public class Cyclone2Driver : IDriver
{
	private readonly IPlatform platform;

	public Cyclone2Driver(IPlatform platform)
	{
		this.platform = platform;
	}

	public bool IsSupported(LibUsbDevice device, LibUsbDeviceDescriptor descriptor) => descriptor is
	{
		IdVendor: UsbIds.GameSirVendorId,
		IdProduct: UsbIds.GameSirCyclone2WiredProductId or UsbIds.GameSirCyclone2WirelessProductId
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
		private long prevHeartbeatTime;
		private byte rumbleLeft;
		private byte rumbleRight;
		private long sensorTicks;
		private ushort? prevSensorTick;

		private const int ReportSizeXUsb = 32;
		private const int ReportSizeHid = 64;
		private const byte ReportIdOutput = 0x0F;
		private const byte ReportIdInput = 0x12;
		private const byte ReportIdInputCommands = 0x10;

		private const byte InterfaceNumberXUsb = 0x00;
		private const byte InterfaceNumberHid = 0x01;

		private static readonly long HeartbeatRate = Stopwatch.Frequency / 2; // 500ms
		private static ReadOnlySpan<byte> PacketHeartbeat => [ReportIdOutput, (byte)Cyclone2Command.OutHeartbeat];

		private static readonly GamepadDescriptor Descriptor = new()
		{
			Name = "GameSir Cyclone 2",
			GyroPollingRate = 250,
			GyroRangeDps = 2000,
			AccelRangeGs = 4,
			SerialNumber = new("CYCLN2"u8),
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
			// periodically send heartbeat to enable hid mode
			if (Stopwatch.GetTimestamp() - prevHeartbeatTime >= HeartbeatRate)
			{
				SendHeartbeat();
			}

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
			if (data[0] != ReportIdInput) return;

			ref Cyclone2InputReport input = ref Unsafe.As<byte, Cyclone2InputReport>(ref Unsafe.AsRef(in data[1]));

			Cyclone2Buttons buttons = input.Buttons;
			Cyclone2SpecialButtons special = input.RawSpecialButtons;
			if (xusb == null)
			{
				Cyclone2Buttons dpad = (Cyclone2Buttons)((ushort)input.Buttons & 0xF);
				bool dpadUp = dpad is Cyclone2Buttons.DpadNorth or Cyclone2Buttons.DpadNorthwest or Cyclone2Buttons.DpadNortheast;
				bool dpadDown = dpad is Cyclone2Buttons.DpadSouth or Cyclone2Buttons.DpadSouthwest
					or Cyclone2Buttons.DpadSoutheast;
				bool dpadLeft = dpad is Cyclone2Buttons.DpadWest or Cyclone2Buttons.DpadNorthwest
					or Cyclone2Buttons.DpadSouthwest;
				bool dpadRight = dpad is Cyclone2Buttons.DpadEast or Cyclone2Buttons.DpadNortheast
					or Cyclone2Buttons.DpadSoutheast;

				gamepad.SetButton(GamepadButtons.DpadUp, dpadUp);
				gamepad.SetButton(GamepadButtons.DpadDown, dpadDown);
				gamepad.SetButton(GamepadButtons.DpadLeft, dpadLeft);
				gamepad.SetButton(GamepadButtons.DpadRight, dpadRight);
				gamepad.SetButton(GamepadButtons.West, buttons.HasFlag(Cyclone2Buttons.West));
				gamepad.SetButton(GamepadButtons.South, buttons.HasFlag(Cyclone2Buttons.South));
				gamepad.SetButton(GamepadButtons.East, buttons.HasFlag(Cyclone2Buttons.East));
				gamepad.SetButton(GamepadButtons.North, buttons.HasFlag(Cyclone2Buttons.North));
				gamepad.SetButton(GamepadButtons.LeftShoulder, buttons.HasFlag(Cyclone2Buttons.LeftShoulder));
				gamepad.SetButton(GamepadButtons.RightShoulder, buttons.HasFlag(Cyclone2Buttons.RightShoulder));
				gamepad.SetButton(GamepadButtons.Back, buttons.HasFlag(Cyclone2Buttons.Share));
				gamepad.SetButton(GamepadButtons.Start, buttons.HasFlag(Cyclone2Buttons.Options));
				gamepad.SetButton(GamepadButtons.LeftStick, buttons.HasFlag(Cyclone2Buttons.LeftStick));
				gamepad.SetButton(GamepadButtons.RightStick, buttons.HasFlag(Cyclone2Buttons.RightStick));
				gamepad.SetButton(GamepadButtons.Guide, special.HasFlag(Cyclone2SpecialButtons.Guide));

				gamepad.SetAxis(GamepadAxis.LeftStickX, BitHelpers.ScaleByteToShort(input.LeftStickX));
				gamepad.SetAxis(GamepadAxis.LeftStickY, BitHelpers.ScaleByteToShort(input.LeftStickY));
				gamepad.SetAxis(GamepadAxis.RightStickX, BitHelpers.ScaleByteToShort(input.RightStickX));
				gamepad.SetAxis(GamepadAxis.RightStickY, BitHelpers.ScaleByteToShort(input.RightStickY));
				gamepad.SetAxis(GamepadAxis.LeftTrigger, BitHelpers.ScaleByteToShort(input.LeftTrigger));
				gamepad.SetAxis(GamepadAxis.RightTrigger, BitHelpers.ScaleByteToShort(input.RightTrigger));
			}

			gamepad.SetButton(GamepadButtons.LeftPaddle1, special.HasFlag(Cyclone2SpecialButtons.LeftBackButton));
			gamepad.SetButton(GamepadButtons.RightPaddle1, special.HasFlag(Cyclone2SpecialButtons.RightBackButton));
			gamepad.SetButton(GamepadButtons.Misc1, special.HasFlag(Cyclone2SpecialButtons.Capture));
			gamepad.SetButton(GamepadButtons.Misc2, special.HasFlag(Cyclone2SpecialButtons.MButton));
			
			// gyro
			long delta = input.Timestamp - (prevSensorTick ?? input.Timestamp);
			if (delta < 0) delta += ushort.MaxValue; // wrap

			sensorTicks += delta;
			prevSensorTick = input.Timestamp;

			gamepad.GyroPitch = input.GyroX;
			gamepad.GyroYaw = input.GyroY;
			gamepad.GyroRoll = input.GyroZ;
			gamepad.AccelX = input.AccelX;
			gamepad.AccelY = input.AccelY;
			gamepad.AccelZ = input.AccelZ;
			gamepad.ImuTimestampUs = (sensorTicks * 16) / 3; // 5.33us units;
		}

		private bool SendHeartbeat()
		{
			if (hid.Write(PacketHeartbeat) >= 0) return false;

			prevHeartbeatTime = Stopwatch.GetTimestamp();
			return true;
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
