using InFract.Emulators;
using InFract.Emulators.Vigem;
using InFract.Emulators.Vigem.Native;
using InFract.Emulators.Viiper;
using InFract.Gamepads;

namespace InFract.Platforms;

public class WindowsPlatform : IPlatform
{
	private VigemEmulator? vigem;
	private ViiperEmulator? viiper;

	private const string DefaultVigemConverter = "dualshock4";
	private const string DefaultViiperConverter = "dualsense";

	public async ValueTask StartAsync()
	{
		string emulatorHint = Hints.Get(Hints.Emulator).ToLowerInvariant();
		if (emulatorHint is "viiper" or "")
		{
			string viiperHost = Hints.Get(Hints.ViiperAddress);
			int viiperPort = Hints.GetInt(Hints.ViiperPort);
			string viiperPassword = Hints.Get(Hints.ViiperPassword);
			try
			{
				viiper = new(viiperHost, viiperPort, viiperPassword);
				await viiper.StartAsync();

				Console.WriteLine($"VIIPER connected: {viiper.ServerName} ({viiper.ServerVersion})");
			}
			catch (Exception e)
			{
				viiper = null;
				Console.Error.WriteLine($"Failed to connect to VIIPER: {e}");
			}
		}

		if (emulatorHint is "vigem" or "" && viiper == null)
		{
			try
			{
				vigem = new();
				Console.WriteLine("ViGEm connected");
			}
			catch (VigemException e)
			{
				vigem = null;
				Console.Error.WriteLine($"Failed to connect to ViGEm: {e.Error}");
			}
		}

		if (vigem == null && viiper == null) throw new Exception("No input emulator installed!");
	}


	public IGamepadConverter CreateConverter(Gamepad gamepad)
	{
		string converterId = Hints.Get(Hints.Converter).ToLowerInvariant();
		
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
