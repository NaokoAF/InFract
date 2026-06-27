using InFract.Drivers;
using InFract.Gamepads;
using InFract.SDL3.HidApi;
using Microsoft.Extensions.Logging;

namespace InFract;

public class App
{
	private readonly ILogger<App> logger;
	private readonly Hints hints;
	private readonly GamepadConverterManager converterManager;
	private readonly DriverManager driverManager;

	public App(
		ILogger<App> logger,
		Hints hints,
		GamepadConverterManager converterManager,
		DriverManager driverManager
	)
	{
		this.logger = logger;
		this.hints = hints;
		this.converterManager = converterManager;
		this.driverManager = driverManager;

		driverManager.DeviceOpened += driver =>
		{
			try
			{
				IGamepadConverter converter = converterManager.Open(driver.Gamepad);
				logger.LogInformation($"Device converted: {driver.Gamepad.Descriptor.Name} [{converter.GetType().Name}]");
			}
			catch (Exception e)
			{
				logger.LogError(e, $"Failed to create converter: {driver.Gamepad.Descriptor.Name}");
			}
		};

		driverManager.DeviceClosed += driver =>
		{
			converterManager.Close(driver.Gamepad);
		};
	}

	public void Start(CancellationToken cancellationToken)
	{
		// load environment variables
		foreach (string hint in Hints.HintNames)
		{
			hints.Set(hint, Environment.GetEnvironmentVariable($"INFRACT_{hint}"));
		}

		// main loop
		logger.LogInformation("Started! Waiting for devices...");
		while (!cancellationToken.IsCancellationRequested)
		{
			driverManager.Update();
			Thread.Sleep(1000);
		}

		logger.LogInformation("Shutting down...");
	}
}
