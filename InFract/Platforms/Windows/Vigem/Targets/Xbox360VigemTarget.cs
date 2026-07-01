using System.Runtime.CompilerServices;
using InFract.Gamepads;
using InFract.Gamepads.Microsoft.Xbox360;
using InFract.Platforms.Windows.Vigem.Native;
using static InFract.Platforms.Windows.Vigem.Native.VigemNative;

namespace InFract.Platforms.Windows.Vigem.Targets;

public class Xbox360VigemTarget : IXbox360Target
{
	private readonly nint client;
	private readonly nint target;
	private Xbox360Effects effects;

	internal Xbox360VigemTarget(nint client, nint target, GamepadDescriptor descriptor)
	{
		this.client = client;
		this.target = target;
		
		VigemException.ThrowIfError(vigem_target_add(client, target));
		VigemException.ThrowIfError(vigem_target_x360_register_notification(client, target, OnNotified));
	}

	public Xbox360Effects PollEffects() => effects;
	
	public void SendInput(in Xbox360InputReport input)
	{
		XUSB_REPORT report = Unsafe.As<Xbox360InputReport, XUSB_REPORT>(ref Unsafe.AsRef(in input));
		VigemException.ThrowIfError(vigem_target_x360_update(client, target, report));
	}

	public void Dispose()
	{
		vigem_target_x360_unregister_notification(target);
		vigem_target_remove(client, target);
		vigem_target_free(target);
	}
	
	private void OnNotified(
		nint client,
		nint target,
		byte largeMotor,
		byte smallMotor,
		byte ledNumber,
		nint userData
	)
	{
		effects.RumbleLeft = largeMotor;
		effects.RumbleRight = smallMotor;
		effects.PlayerLed = ledNumber;
	}
}
