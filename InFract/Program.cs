using InFract.Drivers;
using InFract.Drivers.GameSir;
using InFract.Gamepads;
using InFract.Platforms;
using InFract.SDL3.HidApi;

// ReSharper disable AccessToDisposedClosure
namespace InFract;

internal static class Program
{
	private static readonly IDriver[] Drivers =
	[
		new Cyclone2Driver(),
	];

	private static async Task Main(string[] args)
	{
		// load environment variables
		foreach (string hint in Hints.HintNames)
		{
			Hints.Set(hint, Environment.GetEnvironmentVariable($"INFRACT_{hint}"));
		}
		
		// operating system specifics
		IPlatform? platform = null;
		if (OperatingSystem.IsLinux()) platform = new LinuxPlatform();
		if (OperatingSystem.IsWindows()) platform = new WindowsPlatform();

		if (platform == null)
		{
			Console.Error.WriteLine("Unsupported operating system");
			Environment.Exit(1);
			return;
		}

		await platform.StartAsync();

		// intercept termination to shut down cleanly
		CancellationTokenSource cts = new();
		Console.CancelKeyPress += (_, e) =>
		{
			e.Cancel = true;
			cts.Cancel();
		};

		Hid.Initialize();

		using GamepadConverterManager converterManager = new(g => platform.CreateConverter(g));
		using DriverManager driverManager = new();
		driverManager.DeviceOpened += driver =>
		{
			try
			{
				IGamepadConverter converter = converterManager.Open(driver.Gamepad);
				Console.WriteLine($"Device converted: {GetDevicePrintName(driver)} [{converter.GetType().Name}]");
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Failed to create converter: {GetDevicePrintName(driver)}");
				Console.Error.WriteLine(e);
			}
		};

		driverManager.DeviceClosed += driver =>
		{
			Console.WriteLine($"Device closed: {GetDevicePrintName(driver)}");
			converterManager.Close(driver.Gamepad);
		};

		driverManager.DeviceErrored += (driver, e) =>
		{
			Console.Error.WriteLine($"Device error: {GetDevicePrintName(driver)}");
			Console.Error.WriteLine(e);
		};

		driverManager.DeviceOpenFailed += (driver, device, e) =>
		{
			Console.Error.WriteLine($"Failed to open device: {driver.GetType().Name} [{device.Path}]");
			Console.Error.WriteLine(e);
		};

		// register drivers
		Console.WriteLine("Drivers:");
		foreach (IDriver driver in Drivers)
		{
			Console.WriteLine($"  - {driver.GetType().Name}");
			driverManager.Register(driver);
		}

		Console.WriteLine();

		// main loop
		Console.WriteLine("Started! Waiting for devices...");
		while (!cts.Token.IsCancellationRequested)
		{
			driverManager.Update();
			Thread.Sleep(1000);
		}

		Console.WriteLine("Shutting down...");
		platform.Dispose();
	}

	private static string GetDevicePrintName(IDriverDevice driver) => driver.Gamepad.Descriptor.Name;
}
