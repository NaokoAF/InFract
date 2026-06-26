namespace InFract.Gamepads;

public class GamepadConverterManager : IDisposable
{
	private readonly Dictionary<Gamepad, IGamepadConverter> converters = new();
	private readonly Func<Gamepad, IGamepadConverter> factory;

	public GamepadConverterManager(Func<Gamepad, IGamepadConverter> factory)
	{
		this.factory = factory;
	}
	
	public IGamepadConverter Open(Gamepad gamepad)
	{
		IGamepadConverter? converter;
		if (converters.TryGetValue(gamepad, out converter)) return converter;

		converter = factory(gamepad);
		gamepad.Updated += () => converter.Update(gamepad);
		
		converters.Add(gamepad, converter);
		return converter;
	}

	public void Close(Gamepad gamepad)
	{
		if (!converters.Remove(gamepad, out var converter)) return;
		converter.Dispose();
	}

	public void Dispose()
	{
		foreach (IGamepadConverter converter in converters.Values) converter.Dispose();
		converters.Clear();
	}
}
