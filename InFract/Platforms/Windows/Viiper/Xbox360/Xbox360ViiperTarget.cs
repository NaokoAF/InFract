using InFract.Gamepads;
using InFract.Gamepads.Microsoft.Xbox360;
using Viiper.Client;

namespace InFract.Platforms.Windows.Viiper.Xbox360;

public class Xbox360ViiperTarget : ViiperTarget<ViiperXbox360Input, ViiperXbox360Output>, IXbox360Target
{
	private Xbox360Effects effects;

	internal Xbox360ViiperTarget(ViiperDevice device, GamepadDescriptor descriptor) : base(device)
	{
	}

	public Xbox360Effects PollEffects() => effects;

	public void SendInput(in Xbox360InputReport input)
	{
		ViiperXbox360Input packet;
		packet.Buttons = (uint)input.Buttons;
		packet.LeftStickX = input.LeftStickX;
		packet.LeftStickY = input.LeftStickY;
		packet.RightStickX = input.RightStickX;
		packet.RightStickY = input.RightStickY;
		packet.LeftTrigger = input.LeftTrigger;
		packet.RightTrigger = input.RightTrigger;
		EnqueueInput(packet);
	}

	protected override void OnOutputReceived(ViiperXbox360Output output)
	{
		effects.RumbleLeft = output.RumbleLeft;
		effects.RumbleRight = output.RumbleRight;
	}
}
