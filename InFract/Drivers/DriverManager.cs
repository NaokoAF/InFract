using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using InFract.SDL3.HidApi;

namespace InFract.Drivers;

public class DriverManager : IDisposable
{
	public event Action<IDriverDevice>? DeviceOpened;
	public event Action<IDriver, HidDeviceInfo, Exception>? DeviceOpenFailed;
	public event Action<IDriverDevice>? DeviceClosed;
	public event Action<IDriverDevice, Exception>? DeviceErrored;

	private uint prevChangeCount;
	private readonly CancellationTokenSource cts = new();
	private readonly List<IDriver> drivers = new();
	private readonly ConcurrentDictionary<string, (IDriverDevice Driver, Task Task)> devices = new();

	public void Register(IDriver driver)
	{
		drivers.Add(driver);
	}

	public bool Update()
	{
		uint changeCount = Hid.DeviceChangeCount;
		if (changeCount == prevChangeCount)
			return false;

		foreach (HidDeviceInfo deviceInfo in Hid.EnumerateDevices())
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
				device = Hid.Open(path);
				driverDevice = driver.Create(device);
			}
			catch (Exception e)
			{
				device?.Dispose(); // in case the device was opened, but the driver failed
				DeviceOpenFailed?.Invoke(driver, deviceInfo, e);
				continue;
			}

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
					DeviceErrored?.Invoke(driverDevice, t.Exception.InnerException ?? t.Exception);

				DeviceClosed?.Invoke(driverDevice);

				driverDevice.Dispose();
			});

			devices.TryAdd(path, (driverDevice, task));
			DeviceOpened?.Invoke(driverDevice);
		}

		prevChangeCount = changeCount;
		return true;
	}

	public void Dispose()
	{
		cts.Cancel();
		
		// wait for all threads to stop
		Task.WaitAll(devices.Values.Select(x => x.Task).ToArray());
		devices.Clear();

		cts.Dispose();
	}

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
