using InFract.Gamepads;
using InFract.Platforms.Linux.UHid;
using Microsoft.Extensions.Logging;

namespace InFract.Platforms.Linux;

public class LinuxPlatform : IPlatform
{
	private readonly ILogger<LinuxPlatform> logger;
	private readonly Hints hints;
	private readonly UHidEmulator uhid = new();

	private const string DefaultConverter = "dualsense";

	public LinuxPlatform(ILogger<LinuxPlatform> logger, Hints hints)
	{
		this.logger = logger;
		this.hints = hints;
	}

	public ValueTask StartAsync() => ValueTask.CompletedTask;

	public IGamepadConverter CreateConverter(Gamepad gamepad)
	{
		string converterId = hints.Get(Hints.Converter).ToLowerInvariant();

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
