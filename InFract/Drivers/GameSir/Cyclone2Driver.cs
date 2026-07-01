using System.Diagnostics;
using System.Runtime.CompilerServices;
using InFract.Gamepads;
using InFract.Gamepads.GameSir.Cyclone2;
using InFract.Usb.Hid;
using InFract.Usb.LibUsb;
using InFract.Usb.XUsb;

namespace InFract.Drivers.GameSir;

public class Cyclone2Driver : IDriver
{
	public bool IsSupported(LibUsbDevice device, LibUsbDeviceDescriptor descriptor) => descriptor is
	{
		IdVendor: UsbIds.GameSirVendorId,
		IdProduct: UsbIds.GameSirCyclone2WiredProductId or UsbIds.GameSirCyclone2WirelessProductId
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
		private const byte EndpointXUsbIn = 0x82;
		private const byte EndpointXUsbOut = 0x02;

		private const byte InterfaceNumberHid = 0x01;
		private const byte EndpointHidIn = 0x84;
		private const byte EndpointHidOut = 0x04;

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

		public void Close()
		{
			xusb.Close();
			hid.Close();
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
			if (data[0] != ReportIdInput) return;

			ref Cyclone2InputReport input = ref Unsafe.As<byte, Cyclone2InputReport>(ref Unsafe.AsRef(in data[1]));

			// buttons
			Cyclone2SpecialButtons special = input.RawSpecialButtons;
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
			if (!hid.Write(PacketHeartbeat)) return false;

			prevHeartbeatTime = Stopwatch.GetTimestamp();
			return true;
		}

		public void Dispose()
		{
			xusb.Dispose();
			hid.Dispose();
			device.Dispose();
		}
	}
}
