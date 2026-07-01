using System.Diagnostics.CodeAnalysis;
using InFract.Gamepads;

namespace InFract.Platforms;

public interface IEmulator
{
	IEnumerable<string> ConverterIds { get; }
	
	bool HasConverter(string id);
	bool TryCreateConverter(string id, GamepadDescriptor descriptor, [NotNullWhen(true)] out IGamepadConverter? converter);
}
