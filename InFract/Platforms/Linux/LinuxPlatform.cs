using InFract.Gamepads;
using InFract.Platforms.Linux.UHid;
using InFract.Usb.LibUsb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InFract.Platforms.Linux;

public class LinuxPlatform : IPlatform
{
	private readonly ILogger<LinuxPlatform> logger;
	private readonly Hints hints;
	private readonly UHidEmulator uhid = new();
	private readonly LibUsbContext libUsb;

	private const string DefaultConverter = "dualsense";

	public LinuxPlatform(
		ILogger<LinuxPlatform> logger,
		Hints hints,
		LibUsbContext libUsb,
		UHidEmulator uhid
	)
	{
		this.logger = logger;
		this.hints = hints;
		this.libUsb = libUsb;
		this.uhid = uhid;
	}

	public static void AddServices(IServiceCollection collection)
	{
		collection.AddSingleton<IPlatform, LinuxPlatform>();
		collection.AddSingleton<UHidEmulator>();
	}

	public ValueTask StartAsync() => ValueTask.CompletedTask;

	public void Poll()
	{
		libUsb.HandleEvents(PollTimeout);
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

	public void Dispose()
	{
	}
}
