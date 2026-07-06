using InFract.Drivers;
using InFract.Gamepads;
using InFract.Platforms;
using Microsoft.Extensions.Logging;

namespace InFract;

public class App
{
	private readonly ILogger<App> logger;
	private readonly Hints hints;
	private readonly IPlatform platform;
	private readonly GamepadConverterManager converterManager;
	private readonly DriverManager driverManager;

	public App(
		ILogger<App> logger,
		Hints hints,
		IPlatform platform,
		GamepadConverterManager converterManager,
		DriverManager driverManager
	)
	{
		this.logger = logger;
		this.hints = hints;
		this.platform = platform;
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

		driverManager.DeviceClosed += driver => converterManager.Close(driver.Gamepad);
	}

	public void Start(CancellationToken cancellationToken)
	{
		// load environment variables
		foreach (string hint in Hints.HintNames)
		{
			hints.Set(hint, Environment.GetEnvironmentVariable($"INFRACT_{hint}"));
		}

		driverManager.Start();
		
		// main loop
		while (!cancellationToken.IsCancellationRequested)
		{
			platform.Poll();
			driverManager.Update();
			converterManager.Update();
		}

		logger.LogInformation("Shutting down...");
	}
}
