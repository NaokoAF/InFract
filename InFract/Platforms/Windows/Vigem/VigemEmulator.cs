using System.Diagnostics.CodeAnalysis;
using InFract.Gamepads;
using InFract.Gamepads.Microsoft.Xbox360;
using InFract.Gamepads.Sony.DualShock4;
using InFract.Platforms.Windows.Vigem.Native;
using InFract.Platforms.Windows.Vigem.Targets;
using static InFract.Platforms.Windows.Vigem.Native.VigemNative;

namespace InFract.Platforms.Windows.Vigem;

public class VigemEmulator : IEmulator
{
	public IEnumerable<string> ConverterIds => Converters.Keys;
	
	private readonly nint client;
	
	private static readonly Dictionary<string, Func<nint, GamepadDescriptor, IGamepadConverter>> Converters =
		new(StringComparer.OrdinalIgnoreCase)
		{
			{ "xbox360", (c, d) => new Xbox360Converter(CreateXbox360(c, d)) },
			{ "dualshock4", (c, d) => new DualShock4Converter(CreateDualShock4(c, d)) },
		};

	public VigemEmulator()
	{
		client = vigem_alloc();
		if (client == 0) throw new VigemException("Failed to allocate ViGEm");

		VigemException.ThrowIfError(vigem_connect(client));
	}

	public bool HasConverter(string id) => Converters.ContainsKey(id);

	public bool TryCreateConverter(string id, GamepadDescriptor descriptor, [NotNullWhen(true)] out IGamepadConverter? converter)
	{
		Func<nint, GamepadDescriptor, IGamepadConverter>? factory;
		if (!Converters.TryGetValue(id, out factory))
		{
			converter = null;
			return false;
		}

		converter = factory(client, descriptor);
		return true;
	}

	public void Dispose()
	{
		vigem_disconnect(client);
		vigem_free(client);
	}
	
	private static DualShock4VigemTarget CreateDualShock4(nint client, GamepadDescriptor descriptor)
	{
		nint target = vigem_target_ds4_alloc();
		vigem_target_set_vid(target, UsbIds.SonyVendorId);
		vigem_target_set_pid(target, UsbIds.SonyDualShock4Zct1ProductId);
		return new DualShock4VigemTarget(client, target, descriptor);
	}

	private static Xbox360VigemTarget CreateXbox360(nint client, GamepadDescriptor descriptor)
	{
		nint target = vigem_target_x360_alloc();
		vigem_target_set_vid(target, UsbIds.MicrosoftVendorId);
		vigem_target_set_pid(target, UsbIds.MicrosoftXbox360WiredProductId);
		return new Xbox360VigemTarget(client, target, descriptor);
	}
}
