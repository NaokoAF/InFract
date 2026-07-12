using System.Collections.Concurrent;
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
		private readonly ConcurrentBag<Exception> inputErrors = new();
		private long prevHeartbeatTime;
		private long sensorTicks;
		private ushort? prevSensorTick;
		private GamepadEffects prevEffects;

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
			if (!inputErrors.IsEmpty) throw new AggregateException(inputErrors);

			// periodically send heartbeat to enable hid mode
			if (Stopwatch.GetTimestamp() - prevHeartbeatTime >= HeartbeatRate)
			{
				SendHeartbeat();
			}

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

			Interlocked.Increment(ref state.SequenceNumber);
		}

		private void OnHidInputReceived(Exception? exception, ReadOnlySpan<byte> data)
		{
			if (exception != null)
			{
				inputErrors.Add(exception);
				return;
			}

			if (data[0] != ReportIdInput) return;

			ref GamepadState state = ref gamepad.State;
			ref Cyclone2InputReport input = ref Unsafe.As<byte, Cyclone2InputReport>(ref Unsafe.AsRef(in data[1]));

			Cyclone2Buttons buttons = input.Buttons;
			Cyclone2SpecialButtons special = input.RawSpecialButtons;
			if (xusb == null)
			{
				Cyclone2Buttons dpad = (Cyclone2Buttons)((ushort)input.Buttons & 0xF);
				bool dpadUp = dpad is Cyclone2Buttons.DpadNorth or Cyclone2Buttons.DpadNorthwest
					or Cyclone2Buttons.DpadNortheast;
				bool dpadDown = dpad is Cyclone2Buttons.DpadSouth or Cyclone2Buttons.DpadSouthwest
					or Cyclone2Buttons.DpadSoutheast;
				bool dpadLeft = dpad is Cyclone2Buttons.DpadWest or Cyclone2Buttons.DpadNorthwest
					or Cyclone2Buttons.DpadSouthwest;
				bool dpadRight = dpad is Cyclone2Buttons.DpadEast or Cyclone2Buttons.DpadNortheast
					or Cyclone2Buttons.DpadSoutheast;

				state.SetButton(GamepadButtons.DpadUp, dpadUp);
				state.SetButton(GamepadButtons.DpadDown, dpadDown);
				state.SetButton(GamepadButtons.DpadLeft, dpadLeft);
				state.SetButton(GamepadButtons.DpadRight, dpadRight);
				state.SetButton(GamepadButtons.West, buttons.HasFlag(Cyclone2Buttons.West));
				state.SetButton(GamepadButtons.South, buttons.HasFlag(Cyclone2Buttons.South));
				state.SetButton(GamepadButtons.East, buttons.HasFlag(Cyclone2Buttons.East));
				state.SetButton(GamepadButtons.North, buttons.HasFlag(Cyclone2Buttons.North));
				state.SetButton(GamepadButtons.LeftShoulder, buttons.HasFlag(Cyclone2Buttons.LeftShoulder));
				state.SetButton(GamepadButtons.RightShoulder, buttons.HasFlag(Cyclone2Buttons.RightShoulder));
				state.SetButton(GamepadButtons.Back, buttons.HasFlag(Cyclone2Buttons.Share));
				state.SetButton(GamepadButtons.Start, buttons.HasFlag(Cyclone2Buttons.Options));
				state.SetButton(GamepadButtons.LeftStick, buttons.HasFlag(Cyclone2Buttons.LeftStick));
				state.SetButton(GamepadButtons.RightStick, buttons.HasFlag(Cyclone2Buttons.RightStick));
				state.SetButton(GamepadButtons.Guide, special.HasFlag(Cyclone2SpecialButtons.Guide));

				state.LeftStickX = BitHelpers.ScaleByteToShort(input.LeftStickX);
				state.LeftStickY = BitHelpers.ScaleByteToShort(input.LeftStickY);
				state.RightStickX = BitHelpers.ScaleByteToShort(input.RightStickX);
				state.RightStickY = BitHelpers.ScaleByteToShort(input.RightStickY);
				state.LeftTrigger = BitHelpers.ScaleByteToShort(input.LeftTrigger);
				state.RightTrigger = BitHelpers.ScaleByteToShort(input.RightTrigger);
			}

			state.SetButton(GamepadButtons.LeftPaddle1, special.HasFlag(Cyclone2SpecialButtons.LeftBackButton));
			state.SetButton(GamepadButtons.RightPaddle1, special.HasFlag(Cyclone2SpecialButtons.RightBackButton));
			state.SetButton(GamepadButtons.Misc1, special.HasFlag(Cyclone2SpecialButtons.Capture));
			state.SetButton(GamepadButtons.Misc2, special.HasFlag(Cyclone2SpecialButtons.MButton));

			// gyro
			long delta = input.Timestamp - (prevSensorTick ?? input.Timestamp);
			if (delta < 0) delta += ushort.MaxValue; // wrap

			sensorTicks += delta;
			prevSensorTick = input.Timestamp;

			state.GyroPitch = input.GyroX;
			state.GyroYaw = input.GyroY;
			state.GyroRoll = input.GyroZ;
			state.AccelX = input.AccelX;
			state.AccelY = input.AccelY;
			state.AccelZ = input.AccelZ;
			state.ImuTimestampUs = (sensorTicks * 16) / 3; // 5.33us units;

			Interlocked.Increment(ref state.SequenceNumber);
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
