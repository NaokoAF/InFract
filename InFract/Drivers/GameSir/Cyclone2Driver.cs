using System.Diagnostics;
using System.Runtime.CompilerServices;
using InFract.Gamepads;
using InFract.Gamepads.GameSir.Cyclone2;
using InFract.SDL3.HidApi;

namespace InFract.Drivers.GameSir;

public class Cyclone2Driver : IDriver
{
	public bool IsSupported(in HidDeviceInfo device)
	{
		if (device.VendorId != UsbIds.GameSirVendorId) return false;

		if (device.ProductId == UsbIds.GameSirCyclone2WiredProductId ||
		    device.ProductId == UsbIds.GameSirCyclone2WirelessProductId)
		{
			// Cyclone 2 appears as multiple HID interfaces. search for the vendor-defined one
			return device.UsagePage == 0xFFF0;
		}

		return false;
	}

	public IDriverDevice Create(HidDevice device) => new DriverDevice(device);

	private class DriverDevice : IDriverDevice
	{
		public HidDevice Device => device;
		public Gamepad Gamepad => gamepad;

		private readonly HidDevice device;
		private readonly Gamepad gamepad;
		private long prevHeartbeatTime;
		private byte rumbleLeft;
		private byte rumbleRight;
		private long sensorTicks;
		private ushort? prevSensorTick;

		private const int ReportSize = 64;
		private const byte ReportIdOutput = 0x0F;
		private const byte ReportIdInput = 0x12;
		private const byte ReportIdInputCommands = 0x10;

		private static readonly long HeartbeatRate = Stopwatch.Frequency / 2; // 500ms
		private static ReadOnlySpan<byte> PacketHeartbeat => [ReportIdOutput, (byte)Cyclone2Command.OutHeartbeat];

		private static readonly GamepadDescriptor Descriptor = new()
		{
			Name = "GameSir Cyclone 2",
			GyroPollingRate = 250,
			GyroRangeDps = 2000,
			AccelRangeGs = 2,
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

		public DriverDevice(HidDevice device)
		{
			this.device = device;
			gamepad = new(Descriptor);

			// hid writes can fail. try a few times
			bool switched = false;
			for (int attempts = 0; attempts < 8; attempts++)
			{
				switched = SendHeartbeat();
				if (switched) break;
			}

			if (!switched) throw new Exception("Failed to switch to extended mode");

			// wait for an input packet
			bool acknowledged = false;
			byte[] readBuffer = new byte[ReportSize];
			for (int attempts = 0; attempts < 100; attempts++)
			{
				int readBytes = device.Read(readBuffer, 1);
				if (readBytes < 0) throw new Exception("Failed to read response");

				acknowledged = readBytes == ReportSize && readBuffer[0] == ReportIdInput;
				if (acknowledged) break;
			}

			if (!acknowledged) throw new Exception("No response after switching to extended mode");
		}

		public void Start(CancellationToken cancellationToken)
		{
			byte[] readBuffer = new byte[ReportSize];
			while (device.Read(readBuffer, 500) > 0 && !cancellationToken.IsCancellationRequested)
			{
				// read input
				if (readBuffer[0] == ReportIdInput)
				{
					ref Cyclone2InputReport inputReport = ref Unsafe.As<byte, Cyclone2InputReport>(ref readBuffer[1]);
					UpdateState(inputReport);
				}

				// periodically send heartbeat to enable hid mode
				if (Stopwatch.GetTimestamp() - prevHeartbeatTime >= HeartbeatRate)
				{
					SendHeartbeat();
				}

				// rumble
				if (rumbleLeft != gamepad.RumbleLeft || rumbleRight != gamepad.RumbleRight)
				{
					rumbleLeft = gamepad.RumbleLeft;
					rumbleRight = gamepad.RumbleRight;
					SendRumble(rumbleLeft, rumbleRight);
				}
			}
		}

		private void UpdateState(in Cyclone2InputReport state)
		{
			Cyclone2Buttons buttons = state.RawButtons;
			Cyclone2SpecialButtons special = state.RawSpecialButtons;

			// dpad
			Cyclone2Buttons dpad = (Cyclone2Buttons)((int)buttons & 0xF);
			bool dpadUp = dpad is Cyclone2Buttons.DpadNorth or Cyclone2Buttons.DpadNortheast or Cyclone2Buttons.DpadNorthwest;
			bool dpadDown = dpad is Cyclone2Buttons.DpadSouth or Cyclone2Buttons.DpadSoutheast or Cyclone2Buttons.DpadSouthwest;
			bool dpadLeft = dpad is Cyclone2Buttons.DpadWest or Cyclone2Buttons.DpadNorthwest or Cyclone2Buttons.DpadSouthwest;
			bool dpadRight = dpad is Cyclone2Buttons.DpadEast or Cyclone2Buttons.DpadNortheast or Cyclone2Buttons.DpadSoutheast;
			gamepad.SetButton(GamepadButtons.DpadUp, dpadUp);
			gamepad.SetButton(GamepadButtons.DpadDown, dpadDown);
			gamepad.SetButton(GamepadButtons.DpadLeft, dpadLeft);
			gamepad.SetButton(GamepadButtons.DpadRight, dpadRight);

			// buttons
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
			gamepad.SetButton(GamepadButtons.LeftPaddle1, special.HasFlag(Cyclone2SpecialButtons.LeftBackButton));
			gamepad.SetButton(GamepadButtons.RightPaddle1, special.HasFlag(Cyclone2SpecialButtons.RightBackButton));
			gamepad.SetButton(GamepadButtons.Misc1, special.HasFlag(Cyclone2SpecialButtons.Capture));
			gamepad.SetButton(GamepadButtons.Misc2, special.HasFlag(Cyclone2SpecialButtons.MButton));

			// axes
			gamepad.SetAxis(GamepadAxis.LeftStickX, BitHelpers.ScaleByteToShort(state.LeftStickX));
			gamepad.SetAxis(GamepadAxis.LeftStickY, BitHelpers.ScaleByteToShort(state.LeftStickY));
			gamepad.SetAxis(GamepadAxis.RightStickX, BitHelpers.ScaleByteToShort(state.RightStickX));
			gamepad.SetAxis(GamepadAxis.RightStickY, BitHelpers.ScaleByteToShort(state.RightStickY));
			gamepad.SetAxis(GamepadAxis.LeftTrigger, BitHelpers.ScaleByteToShort(state.LeftTrigger));
			gamepad.SetAxis(GamepadAxis.RightTrigger, BitHelpers.ScaleByteToShort(state.RightTrigger));

			// gyro
			long delta = state.Timestamp - (prevSensorTick ?? state.Timestamp);
			if (delta < 0) delta += ushort.MaxValue; // wrap

			sensorTicks += delta;
			prevSensorTick = state.Timestamp;

			gamepad.GyroPitch = state.GyroX;
			gamepad.GyroYaw = state.GyroY;
			gamepad.GyroRoll = state.GyroZ;
			gamepad.AccelX = state.AccelX;
			gamepad.AccelY = state.AccelY;
			gamepad.AccelZ = state.AccelZ;
			gamepad.ImuTimestampUs = (sensorTicks * 16) / 3; // 5.33us units;

			gamepad.Update();
		}

		private void SendRumble(byte rumbleLeft, byte rumbleRight)
		{
			Span<byte> buffer = stackalloc byte[ReportSize];
			buffer[0] = ReportIdOutput;

			Unsafe.As<byte, Cyclone2RumbleCommand>(ref buffer[1]) = new()
			{
				RumbleLeft = rumbleLeft,
				RumbleRight = rumbleRight,
			};

			device.Write(buffer);
		}

		private bool SendHeartbeat()
		{
			int written = device.Write(PacketHeartbeat);
			if (written < PacketHeartbeat.Length) return false;

			prevHeartbeatTime = Stopwatch.GetTimestamp();
			return true;
		}

		public void Dispose()
		{
			device.Dispose();
		}
	}
}
