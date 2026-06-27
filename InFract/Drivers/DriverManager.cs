using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using InFract.SDL3.HidApi;
using Microsoft.Extensions.Logging;

namespace InFract.Drivers;

public class DriverManager : IAsyncDisposable, IDisposable
{
	public ImmutableArray<IDriver> Drivers => drivers;
	
	public event Action<IDriverDevice>? DeviceOpened;
	public event Action<IDriverDevice>? DeviceClosed;

	private readonly ILogger<DriverManager> logger;
	private readonly HidContext hid;
	private readonly ImmutableArray<IDriver> drivers;
	private readonly CancellationTokenSource cts = new();
	private readonly ConcurrentDictionary<string, (IDriverDevice Driver, Task Task)> devices = new();
	private uint prevChangeCount;

	public DriverManager(
		ILogger<DriverManager> logger,
		HidContext hid,
		IEnumerable<IDriver> drivers
	)
	{
		this.logger = logger;
		this.hid = hid;
		this.drivers = drivers.ToImmutableArray();
		
		// list drivers
		logger.LogInformation($"Drivers: {string.Join(", ", this.drivers.Select(d => d.GetType().Name))}");

	}

	public void Update()
	{
		uint changeCount = hid.DeviceChangeCount;
		if (changeCount == prevChangeCount) return;

		foreach (HidDeviceInfo deviceInfo in hid.EnumerateDevices())
		{
			string path = deviceInfo.Path;

			// skip already opened devices, and devices on cooldown
			if (devices.ContainsKey(path)) continue;

			// get driver for device
			if (!TryGetDriverForDevice(deviceInfo, out IDriver? driver)) continue;

			// try to open device and create driver
			HidDevice? device = null;
			IDriverDevice driverDevice;
			try
			{
				device = hid.Open(path);
				driverDevice = driver.Create(device);
			}
			catch (Exception e)
			{
				logger.LogError(e, $"Failed to open device: {driver.GetType().Name} [{path}]");
				
				device?.Dispose(); // in case the device was opened, but the driver failed
				continue;
			}
			
			logger.LogInformation($"Device opened: {driverDevice.Gamepad.Descriptor.Name}");

			// start driver in its own thead
			Task task = Task.Factory.StartNew(
				() => driverDevice.Start(cts.Token),
				cts.Token,
				TaskCreationOptions.LongRunning,
				TaskScheduler.Default
			);

			// clean up when the thread exits
			task.ContinueWith(t =>
			{
				devices.TryRemove(path, out _);

				if (t.IsFaulted && t.Exception != null)
				{
					Exception? exception = t.Exception.InnerException ?? t.Exception;
					logger.LogError(exception, $"Device error: {driverDevice.Gamepad.Descriptor.Name}");
				}

				DeviceClosed?.Invoke(driverDevice);

				driverDevice.Dispose();
			});

			devices.TryAdd(path, (driverDevice, task));
			DeviceOpened?.Invoke(driverDevice);
		}

		prevChangeCount = changeCount;
	}

	public async ValueTask DisposeAsync()
	{
		await cts.CancelAsync();
		await Task.WhenAll(devices.Values.Select(x => x.Task).ToArray());
		cts.Dispose();
	}
	
	public void Dispose() => DisposeAsync().GetAwaiter().GetResult();

	private bool TryGetDriverForDevice(in HidDeviceInfo deviceInfo, [NotNullWhen(true)] out IDriver? driver)
	{
		foreach (IDriver other in drivers)
		{
			if (other.IsSupported(deviceInfo))
			{
				driver = other;
				return true;
			}
		}

		driver = null;
		return false;
	}
}
