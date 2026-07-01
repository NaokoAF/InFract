using InFract.Gamepads;
using InFract.Platforms.Windows.Vigem;
using InFract.Platforms.Windows.Vigem.Native;
using InFract.Platforms.Windows.Viiper;
using Microsoft.Extensions.Logging;

namespace InFract.Platforms.Windows;

public class WindowsPlatform : IPlatform
{
	private readonly ILogger<WindowsPlatform> logger;
	private readonly Hints hints;
	private VigemEmulator? vigem;
	private ViiperEmulator? viiper;

	private const string DefaultVigemConverter = "dualshock4";
	private const string DefaultViiperConverter = "dualsense";

	public WindowsPlatform(ILogger<WindowsPlatform> logger, Hints hints)
	{
		this.logger = logger;
		this.hints = hints;
	}

	public async ValueTask StartAsync()
	{
		string emulatorHint = hints.Get(Hints.Emulator).ToLowerInvariant();
		if (emulatorHint is "viiper" or "")
		{
			string viiperHost = hints.Get(Hints.ViiperAddress);
			int viiperPort = hints.GetInt(Hints.ViiperPort);
			string viiperPassword = hints.Get(Hints.ViiperPassword);
			try
			{
				viiper = new(viiperHost, viiperPort, viiperPassword);
				await viiper.StartAsync();

				logger.LogInformation($"VIIPER connected: {viiper.ServerName} ({viiper.ServerVersion})");
			}
			catch (Exception e)
			{
				logger.LogError(e, "Failed to connect to VIIPER");
				viiper = null;
			}
		}

		if (emulatorHint is "vigem" or "" && viiper == null)
		{
			try
			{
				vigem = new();
				logger.LogInformation("ViGEm connected");
			}
			catch (VigemException e)
			{
				logger.LogError(e, "Failed to connect to ViGEm");
				vigem = null;
			}
		}

		if (vigem == null && viiper == null) throw new Exception("No input emulator installed!");
	}

	public IGamepadConverter CreateConverter(Gamepad gamepad)
	{
		string converterId = hints.Get(Hints.Converter).ToLowerInvariant();

		IEmulator emulator;
		string defaultConverter;
		if (viiper != null)
		{
			emulator = viiper;
			defaultConverter = DefaultViiperConverter;
		}
		else if (vigem != null)
		{
			emulator = vigem;
			defaultConverter = DefaultVigemConverter;
		}
		else
		{
			throw new Exception("No input emulator installed!");
		}

		IGamepadConverter? converter;
		if (!emulator.HasConverter(converterId)) converterId = defaultConverter;

		if (!emulator.TryCreateConverter(converterId, gamepad.Descriptor, out converter))
			throw new Exception($"Failed to create converter: {converterId}");

		return converter;
	}

	public void Dispose()
	{
		vigem?.Dispose();
		viiper?.Dispose();
	}
}
