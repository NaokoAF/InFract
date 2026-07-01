using System.Diagnostics.CodeAnalysis;
using InFract.Gamepads;
using InFract.Gamepads.SInput;
using InFract.Gamepads.Sony.DualSense;
using InFract.Gamepads.Sony.DualShock4;
using InFract.Platforms.Linux.UHid.Targets;

namespace InFract.Platforms.Linux.UHid;

public class UHidEmulator : IEmulator
{
	public IEnumerable<string> ConverterIds => Converters.Keys;
	
	private static readonly Dictionary<string, Func<GamepadDescriptor, IGamepadConverter>> Converters =
		new(StringComparer.OrdinalIgnoreCase)
		{
			{ "sinput", d => new SInputConverter(new SInputUHidTarget(d)) },
			{ "dualshock4", d => new DualShock4Converter(new DualShock4UHidTarget(d)) },
			{ "dualsense", d => new DualSenseConverter(new DualSenseUHidTarget(d, false)) },
			{ "dualsenseedge", d => new DualSenseConverter(new DualSenseUHidTarget(d, true)) },
		};

	public bool HasConverter(string id) => Converters.ContainsKey(id);

	public bool TryCreateConverter(string id, GamepadDescriptor descriptor, [NotNullWhen(true)] out IGamepadConverter? converter)
	{
		Func<GamepadDescriptor, IGamepadConverter>? factory;
		if (!Converters.TryGetValue(id, out factory))
		{
			converter = null;
			return false;
		}

		converter = factory(descriptor);
		return true;
	}
}
