using InFract;
using InFract.Drivers;
using InFract.Drivers.GameSir;
using InFract.Gamepads;
using InFract.Platforms;
using InFract.Usb.LibUsb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

ServiceCollection collection = new();
collection.AddLogging(x =>
	{
		x.AddConsole();
		x.SetMinimumLevel(LogLevel.Debug);
	}
);

// operating system
if (OperatingSystem.IsLinux())
	collection.AddSingleton<IPlatform, LinuxPlatform>();
else if (OperatingSystem.IsWindows())
	collection.AddSingleton<IPlatform, WindowsPlatform>();
else
	throw new NotSupportedException("Unsupported operating system");

// drivers
collection.AddSingleton<IDriver, Cyclone2Driver>();
collection.AddSingleton<IDriver, TegenariaDriver>();

// app
collection.AddSingleton<LibUsbContext>();
collection.AddSingleton<GamepadConverterManager>();
collection.AddSingleton<DriverManager>();
collection.AddSingleton<Hints>();
collection.AddSingleton<App>();

await using ServiceProvider services = collection.BuildServiceProvider();

// start
CancellationTokenSource cts = new();
Console.CancelKeyPress += (_, e) =>
{
	e.Cancel = true;
	cts.Cancel();
};

await services.GetRequiredService<IPlatform>().StartAsync();

services.GetRequiredService<App>().Start(cts.Token);
