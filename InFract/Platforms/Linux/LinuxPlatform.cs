using InFract.Gamepads;
using InFract.Platforms.Linux.HidRaw;
using InFract.Platforms.Linux.UHid;
using InFract.Usb.Hid;
using InFract.Usb.LibUsb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using InFract.Platforms.Linux.Systemd;
using InFract.Usb.XUsb;

namespace InFract.Platforms.Linux;

public class LinuxPlatform : IPlatform
{
	private readonly ILogger<LinuxPlatform> logger;
	private readonly Hints hints;
	private readonly LibUsbContext libUsb;
	private readonly HidRawContext hidRaw;
	private readonly UHidEmulator uhid;
	private readonly ManualResetEventSlim manualReset = new(false);
	private readonly CancellationTokenSource cts = new();

	private const int PollTimeout = 500;
	private const string DefaultConverter = "dualsense";

	public LinuxPlatform(
		ILogger<LinuxPlatform> logger,
		Hints hints,
		LibUsbContext libUsb,
		HidRawContext hidRaw,
		UHidEmulator uhid
	)
	{
		this.logger = logger;
		this.hints = hints;
		this.libUsb = libUsb;
		this.hidRaw = hidRaw;
		this.uhid = uhid;
	}

	public static void AddServices(IServiceCollection collection)
	{
		collection.AddSingleton<IPlatform, LinuxPlatform>();
		collection.AddSingleton<UHidEmulator>();
		collection.AddSingleton<HidRawContext>();
	}

	public ValueTask StartAsync()
	{
		Task.Factory.StartNew(
			LibUsbLoop,
			cts.Token,
			TaskCreationOptions.LongRunning,
			TaskScheduler.Default
		);
		
		Task.Factory.StartNew(
			HidRawLoop,
			cts.Token,
			TaskCreationOptions.LongRunning,
			TaskScheduler.Default
		);
		
		return ValueTask.CompletedTask;
	}

	public void Poll()
	{
		manualReset.Wait(PollTimeout, cts.Token);
		manualReset.Reset();
	}

	public IGamepadConverter CreateConverter(Gamepad gamepad)
	{
		string converterId = hints.Get(Hints.Converter).ToLowerInvariant();

		IGamepadConverter? converter;
		if (!uhid.HasConverter(converterId)) converterId = DefaultConverter;

		if (!uhid.TryCreateConverter(converterId, gamepad.Descriptor, out converter))
			throw new Exception($"Failed to create converter: {converterId}");

		return converter;
	}

	public IXUsbInterface OpenXUsb(LibUsbDeviceHandle device, byte interfaceNumber)
	{
		return XUsbLibUsbInterface.Open(device, interfaceNumber);
	}

	public IHidInterface OpenHid(LibUsbDeviceHandle device, byte interfaceNumber)
	{
		// get device data through systemd
		string sysName = $"{device.Device.BusNumber}-{string.Join('.', device.Device.GetPortNumbers())}";
		using SystemdDevice root = SystemdDevice.FromSubSystemSysName("usb"u8, sysName);

		// search for hidraw interfaces
		using SystemdDeviceEnumerator enumerator = root.EnumerateChildren()
			.MatchSubSystem("hidraw"u8)
			.AllowUninitialized();

		SystemdDevice? hidrawDevice = enumerator.GetDevices().FirstOrDefault();
		foreach (SystemdDevice child in enumerator.GetDevices())
		{
			hidrawDevice = child;
			break;
		}

		if(hidrawDevice == null) throw new InvalidOperationException("HIDRAW interface not found");

		for (int i = 0; i <= 10; i++)
		{
			try
			{
				return hidRaw.Open(hidrawDevice.DevName);
			}
			catch
			{
				Thread.Sleep(100);
			}
		}

		throw new InvalidOperationException("Failed to open HIDRAW device");
	}

	private void LibUsbLoop()
	{
		while (!cts.Token.IsCancellationRequested)
		{
			if (!libUsb.HandleEvents(PollTimeout)) continue;
			
			manualReset.Set();
		}
	}

	private void HidRawLoop()
	{
		while (!cts.Token.IsCancellationRequested)
		{
			if (!hidRaw.Poll(PollTimeout)) continue;
			
			manualReset.Set();
		}
	}

	public void Dispose()
	{
		cts.Cancel();
		cts.Dispose();
		manualReset.Set();
		manualReset.Dispose();
	}
}
