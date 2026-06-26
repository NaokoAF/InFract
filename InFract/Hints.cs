using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace InFract;

public static class Hints
{
	public const string Emulator = "EMULATOR";
	public const string Converter = "CONVERTER";
	public const string ViiperAddress = "VIIPER_ADDRESS";
	public const string ViiperPort = "VIIPER_PORT";
	public const string ViiperPassword = "VIIPER_PASSWORD";

	public static readonly FrozenSet<string> HintNames;

	private static readonly ConcurrentDictionary<string, string> Registry = new();
	private static readonly ConcurrentDictionary<string, string> Values = new();

	static Hints()
	{
		Register(Emulator);
		Register(Converter);
		Register(ViiperAddress, "localhost");
		Register(ViiperPort, "3242");
		Register(ViiperPassword);

		HintNames = Registry.Keys.ToFrozenSet();
	}

	private static void Register(string name, string defaultValue = "")
	{
		Registry.TryAdd(name, defaultValue);
	}

	public static string Get(string name)
	{
		if (!Registry.TryGetValue(name, out var defaultValue))
			throw new ArgumentOutOfRangeException(nameof(name), $"Unknown hint: {name}");

		return Values.GetValueOrDefault(name, defaultValue);
	}

	public static int GetInt(string name)
	{
		if (!Registry.TryGetValue(name, out var defaultValue))
			throw new ArgumentOutOfRangeException(nameof(name), $"Unknown hint: {name}");

		return int.TryParse(Values.GetValueOrDefault(name, defaultValue), out int value)
			? value
			: int.Parse(defaultValue);
	}

	public static void Set(string name, string? value)
	{
		if (!Registry.TryGetValue(name, out var defaultValue))
			throw new ArgumentOutOfRangeException(nameof(name), $"Unknown hint: {name}");

		if (value != null)
		{
			Values[name] = value;
		}
		else
		{
			Values.TryRemove(name, out _);
		}
	}
}
