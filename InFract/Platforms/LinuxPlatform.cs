using InFract.Emulators;
using InFract.Emulators.UHid;
using InFract.Emulators.Viiper;
using InFract.Gamepads;

namespace InFract.Platforms;

public class LinuxPlatform : IPlatform
{
	private UHidEmulator uhid = new();

	private const string DefaultConverter = "dualsense";

	public ValueTask StartAsync() => ValueTask.CompletedTask;

	public IGamepadConverter CreateConverter(Gamepad gamepad)
	{
		string converterId = Hints.Get(Hints.Converter).ToLowerInvariant();
		
		IGamepadConverter? converter;
		if (!uhid.HasConverter(converterId)) converterId = DefaultConverter;

		if (!uhid.TryCreateConverter(converterId, gamepad.Descriptor, out converter))
			throw new Exception($"Failed to create converter: {converterId}");
		
		return converter;
	}

	public void Dispose()
	{
	}
}
