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
		gamepad.InputReceived += converter.Update;
		
		converters.Add(gamepad, converter);
		return converter;
	}

	public void Close(Gamepad gamepad)
	{
		if (!converters.Remove(gamepad, out var converter)) return;
		
		gamepad.InputReceived -= converter.Update;
		
		converter.Dispose();
	}

	public void Poll()
	{
		foreach ((Gamepad gamepad, IGamepadConverter converter) in converters)
		{
			try
			{
				gamepad.Effects = converter.PollEffects();
			}
			catch (Exception e)
			{
				logger.LogError(e, $"Failed to poll gamepad converter: {gamepad.Descriptor.Name}");
			}
		}
	}

	public void Close()
	{
		foreach ((Gamepad gamepad, IGamepadConverter converter) in converters)
		{
			gamepad.InputReceived -= converter.Update;
			
			converter.Close();
			converter.Dispose();
		}
		
		converters.Clear();
	}
	
	public void Dispose()
	{
		foreach (IGamepadConverter converter in converters.Values) converter.Dispose();
		converters.Clear();
	}
}
