using System.Runtime.CompilerServices;
using InFract.Gamepads;
using InFract.Gamepads.SInput;

namespace InFract.Platforms.Linux.UHid.Targets;

public class SInputUHidTarget : UHidDevice, ISInputTarget
{
	private SInputEffects effects;
	private readonly byte[] featureReport = new byte[Unsafe.SizeOf<SInputReport>() + 1];

	private const byte ReportSizeOutput = 48;
	private const byte ReportIdInput = 1;
	private const byte ReportIdInputResponse = 2;
	private const byte ReportIdOutput = 3;

	public SInputUHidTarget(GamepadDescriptor descriptor) : base(
		descriptor.Name,
		UsbIds.SInputGenericVendorId,
		UsbIds.SInputGenericProductId,
		UHidBusType.USB,
		ReportDescriptor
	)
	{
		featureReport[0] = ReportIdInputResponse;
		featureReport[1] = (byte)SInputCommandKind.Features;
		featureReport[2] = 0; // protocol version (low byte)
		featureReport[3] = 0; // protocol version (high byte)
		Unsafe.As<byte, SInputFeatures>(ref featureReport[4]) = CreateFeatureReport(descriptor);
	}

	public SInputEffects PollEffects()
	{
		Poll();
		return effects;
	}

	public void SendInput(in SInputReport input) => WriteInput(ReportIdInput, input);

	protected override void OnOutputReport(ReadOnlySpan<byte> data)
	{
		if (data.Length != ReportSizeOutput || data[0] != ReportIdOutput) return;

		switch ((SInputCommandKind)data[1])
		{
			case SInputCommandKind.Features: WriteInput(featureReport); break;
			case SInputCommandKind.PlayerLed: effects.PlayerLed = data[2]; break;
			case SInputCommandKind.JoystickRgb:
				effects.LedRed = data[2];
				effects.LedGreen = data[3];
				effects.LedBlue = data[4];
				break;
			case SInputCommandKind.Haptics:
				ref SInputHaptics haptics = ref Unsafe.As<byte, SInputHaptics>(ref Unsafe.AsRef(in data[2]));
				if (haptics.Type == SInputHapticsType.Erm)
				{
					effects.RumbleLeft = haptics.Erm.LeftAmplitude;
					effects.RumbleRight = haptics.Erm.RightAmplitude;
				}
				break;
		}
	}
	
	private static SInputFeatures CreateFeatureReport(GamepadDescriptor descriptor)
	{
		SInputPhysicalType physicalType = descriptor.Style switch
		{
			GamepadStyle.Standard => SInputPhysicalType.Standard,
			GamepadStyle.Xbox360 => SInputPhysicalType.Xbox360,
			GamepadStyle.XboxOne => SInputPhysicalType.XboxOne,
			GamepadStyle.Ps3 => SInputPhysicalType.Ps3,
			GamepadStyle.Ps4 => SInputPhysicalType.Ps4,
			GamepadStyle.Ps5 => SInputPhysicalType.Ps5,
			GamepadStyle.SwitchPro => SInputPhysicalType.SwitchPro,
			GamepadStyle.JoyconLeft => SInputPhysicalType.JoyconLeft,
			GamepadStyle.JoyconRight => SInputPhysicalType.JoyconRight,
			GamepadStyle.JoyconPair => SInputPhysicalType.JoyconPair,
			GamepadStyle.GameCube => SInputPhysicalType.GameCube,
			GamepadStyle.Steam => SInputPhysicalType.Steam,
			_ => SInputPhysicalType.Standard
		};

		SInputFaceStyleSubProduct faceStyle = descriptor.Style switch
		{
			GamepadStyle.Xbox360 => SInputFaceStyleSubProduct.FaceStyleXbox,
			GamepadStyle.XboxOne => SInputFaceStyleSubProduct.FaceStyleXbox,
			GamepadStyle.Ps3 => SInputFaceStyleSubProduct.FaceStyleSony,
			GamepadStyle.Ps4 => SInputFaceStyleSubProduct.FaceStyleSony,
			GamepadStyle.Ps5 => SInputFaceStyleSubProduct.FaceStyleSony,
			GamepadStyle.SwitchPro => SInputFaceStyleSubProduct.FaceStyleNintendo,
			GamepadStyle.JoyconLeft => SInputFaceStyleSubProduct.FaceStyleNintendo,
			GamepadStyle.JoyconRight => SInputFaceStyleSubProduct.FaceStyleNintendo,
			GamepadStyle.JoyconPair => SInputFaceStyleSubProduct.FaceStyleNintendo,
			GamepadStyle.GameCube => SInputFaceStyleSubProduct.FaceStyleGameCube,
			_ => SInputFaceStyleSubProduct.FaceStyleHid,
		};

		SInputFeatures.MacAddressBuffer macAddress = new();
		descriptor.SerialNumber.Span.CopyTo(macAddress);

		return new()
		{
			FeatureFlags = GetFeatureFlags(descriptor),
			ButtonMask = GetButtonMask(descriptor),
			PhysicalType = physicalType,
			FaceStyleAndSubProduct = faceStyle | SInputFaceStyleSubProduct.SubProductDynamicMode,
			PollingIntervalUs = (ushort)(descriptor.GyroPollingRate > 0 ? 1000000 / descriptor.GyroPollingRate : 0),
			GyroRange = (ushort)descriptor.GyroRangeDps,
			AccelRange = (ushort)descriptor.AccelRangeGs,
			TouchpadCount = (byte)descriptor.TouchpadCount,
			TouchpadFingerCount = (byte)descriptor.TouchpadFingerCount,
			MacAddress = macAddress,
		};
	}

	private static SInputFeatureFlags GetFeatureFlags(GamepadDescriptor descriptor)
	{
		SInputFeatureFlags result = (
			SInputFeatureFlags.Rumble |
			SInputFeatureFlags.LeftAnalogJoystick | SInputFeatureFlags.RightAnalogJoystick |
			SInputFeatureFlags.LeftAnalogTrigger | SInputFeatureFlags.RightAnalogTrigger
		);

		if (descriptor.HasGyro) result |= SInputFeatureFlags.Gyro;
		if (descriptor.HasAccel) result |= SInputFeatureFlags.Accel;
		if (descriptor.HasTouchpads) result |= SInputFeatureFlags.Touchpad;
		return result;
	}

	private static SInputButtons GetButtonMask(GamepadDescriptor descriptor)
	{
		SInputButtons result = SInputButtons.None;
		if (descriptor.Buttons.HasFlag(GamepadButtons.South)) result |= SInputButtons.South;
		if (descriptor.Buttons.HasFlag(GamepadButtons.East)) result |= SInputButtons.East;
		if (descriptor.Buttons.HasFlag(GamepadButtons.West)) result |= SInputButtons.West;
		if (descriptor.Buttons.HasFlag(GamepadButtons.North)) result |= SInputButtons.North;
		if (descriptor.Buttons.HasFlag(GamepadButtons.DpadUp)) result |= SInputButtons.DpadUp;
		if (descriptor.Buttons.HasFlag(GamepadButtons.DpadDown)) result |= SInputButtons.DpadDown;
		if (descriptor.Buttons.HasFlag(GamepadButtons.DpadLeft)) result |= SInputButtons.DpadLeft;
		if (descriptor.Buttons.HasFlag(GamepadButtons.DpadRight)) result |= SInputButtons.DpadRight;
		if (descriptor.Buttons.HasFlag(GamepadButtons.LeftStick)) result |= SInputButtons.LeftStick;
		if (descriptor.Buttons.HasFlag(GamepadButtons.RightStick)) result |= SInputButtons.RightStick;
		if (descriptor.Buttons.HasFlag(GamepadButtons.LeftShoulder)) result |= SInputButtons.LeftShoulder;
		if (descriptor.Buttons.HasFlag(GamepadButtons.RightShoulder)) result |= SInputButtons.RightShoulder;
		if (descriptor.Buttons.HasFlag(GamepadButtons.LeftPaddle1)) result |= SInputButtons.LeftPaddle1;
		if (descriptor.Buttons.HasFlag(GamepadButtons.RightPaddle1)) result |= SInputButtons.RightPaddle1;
		if (descriptor.Buttons.HasFlag(GamepadButtons.Start)) result |= SInputButtons.Start;
		if (descriptor.Buttons.HasFlag(GamepadButtons.Back)) result |= SInputButtons.Back;
		if (descriptor.Buttons.HasFlag(GamepadButtons.Guide)) result |= SInputButtons.Guide;
		if (descriptor.Buttons.HasFlag(GamepadButtons.LeftPaddle2)) result |= SInputButtons.LeftPaddle2;
		if (descriptor.Buttons.HasFlag(GamepadButtons.RightPaddle2)) result |= SInputButtons.RightPaddle2;
		if (descriptor.Buttons.HasFlag(GamepadButtons.Misc1)) result |= SInputButtons.Misc1;
		if (descriptor.Buttons.HasFlag(GamepadButtons.Misc2)) result |= SInputButtons.Misc3;
		if (descriptor.Buttons.HasFlag(GamepadButtons.Misc3)) result |= SInputButtons.Misc4;
		if (descriptor.Buttons.HasFlag(GamepadButtons.Misc4)) result |= SInputButtons.Misc5;
		return result;
	}

	private static ReadOnlySpan<byte> ReportDescriptor =>
	[
		0x05, 0x01, // Usage Page (Generic Desktop Ctrls)
		0x09, 0x05, // Usage (Game Pad)
		0xA1, 0x01, // Collection (Application)
		0x85, 0x01, // Report ID (1)
		0x06, 0x00, 0xFF, //   Usage Page (Vendor Defined 0xFF00)
		0x09, 0x01, //   Usage (0x01)
		0x15, 0x00, //   Logical Minimum (0)
		0x25, 0xFF, //   Logical Maximum (-1)
		0x75, 0x08, //   Report Size (8)
		0x95, 0x02, //   Report Count (2)
		0x81, 0x02, //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
		0x05, 0x09, //   Usage Page (Button)
		0x19, 0x01, //   Usage Minimum (0x01)
		0x29, 0x20, //   Usage Maximum (0x20)
		0x15, 0x00, //   Logical Minimum (0)
		0x25, 0x01, //   Logical Maximum (1)
		0x75, 0x01, //   Report Size (1)
		0x95, 0x20, //   Report Count (32)
		0x81, 0x02, //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
		0x05, 0x01, //   Usage Page (Generic Desktop Ctrls)
		0x09, 0x30, //   Usage (X)
		0x09, 0x31, //   Usage (Y)
		0x09, 0x32, //   Usage (Z)
		0x09, 0x35, //   Usage (Rz)
		0x09, 0x33, //   Usage (Rx)
		0x09, 0x34, //   Usage (Ry)
		0x16, 0x00, 0x80, //   Logical Minimum (-32768)
		0x26, 0xFF, 0x7F, //   Logical Maximum (32767)
		0x75, 0x10, //   Report Size (16)
		0x95, 0x06, //   Report Count (6)
		0x81, 0x02, //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
		0x06, 0x00, 0xFF, //   Usage Page (Vendor Defined 0xFF00)
		0x09, 0x20, //   Usage (0x20)
		0x15, 0x00, //   Logical Minimum (0)
		0x26, 0xFF, 0xFF, //   Logical Maximum (-1)
		0x75, 0x20, //   Report Size (32)
		0x95, 0x01, //   Report Count (1)
		0x81, 0x02, //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
		0x09, 0x21, //   Usage (0x21)
		0x16, 0x00, 0x80, //   Logical Minimum (-32768)
		0x26, 0xFF, 0x7F, //   Logical Maximum (32767)
		0x75, 0x10, //   Report Size (16)
		0x95, 0x06, //   Report Count (6)
		0x81, 0x02, //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
		0x09, 0x22, //   Usage (0x22)
		0x15, 0x00, //   Logical Minimum (0)
		0x26, 0xFF, 0x00, //   Logical Maximum (255)
		0x75, 0x08, //   Report Size (8)
		0x95, 0x1D, //   Report Count (29)
		0x81, 0x02, //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
		0x85, 0x02, //   Report ID (2)
		0x09, 0x23, //   Usage (0x23)
		0x15, 0x00, //   Logical Minimum (0)
		0x26, 0xFF, 0x00, //   Logical Maximum (255)
		0x75, 0x08, //   Report Size (8)
		0x95, 0x3F, //   Report Count (63)
		0x81, 0x02, //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
		0x85, 0x03, //   Report ID (3)
		0x09, 0x24, //   Usage (0x24)
		0x15, 0x00, //   Logical Minimum (0)
		0x26, 0xFF, 0x00, //   Logical Maximum (255)
		0x75, 0x08, //   Report Size (8)
		0x95, 0x2F, //   Report Count (47)
		0x91, 0x02, //   Output (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position,Non-volatile)
		0xC0, // End Collection
	];
}
