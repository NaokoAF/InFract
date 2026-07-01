using InFract.Platforms;
using Microsoft.Extensions.Logging;

namespace InFract.Gamepads;

public class GamepadConverterManager : IDisposable
{
	private readonly ILogger<GamepadConverterManager> logger;
	private readonly IPlatform platform;
	private readonly Dictionary<Gamepad, IGamepadConverter> converters = new();

	public GamepadConverterManager(ILogger<GamepadConverterManager> logger, IPlatform platform)
	{
		this.logger = logger;
		this.platform = platform;
	}

	public IGamepadConverter Open(Gamepad gamepad)
	{
		IGamepadConverter? converter;
		if (converters.TryGetValue(gamepad, out converter)) return converter;

		converter = platform.CreateConverter(gamepad);
		converters.Add(gamepad, converter);
		return converter;
	}

	public void Close(Gamepad gamepad)
	{
		if (!converters.Remove(gamepad, out var converter)) return;
		converter.Dispose();
	}

	public void Update()
	{
		foreach ((Gamepad gamepad, IGamepadConverter converter) in converters)
		{
			converter.Update(gamepad);
		}
	}
	
	public void Dispose()
	{
		foreach (IGamepadConverter converter in converters.Values) converter.Dispose();
		converters.Clear();
	}
}
