using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using InFract.Gamepads;
using InFract.Gamepads.Microsoft.Xbox360;
using InFract.Gamepads.Sony.DualSense;
using InFract.Gamepads.Sony.DualShock4;
using InFract.Platforms.Windows.Viiper.DualSense;
using InFract.Platforms.Windows.Viiper.DualShock4;
using InFract.Platforms.Windows.Viiper.Xbox360;
using Viiper.Client;
using Viiper.Client.Types;

namespace InFract.Platforms.Windows.Viiper;

public class ViiperEmulator : IEmulator
{
	public string? ServerName => serverName;
	public string? ServerVersion => serverVersion;
	public IEnumerable<string> ConverterIds => Converters.Keys;

	private readonly ViiperClient client;
	private readonly ConcurrentDictionary<uint, uint> buses = new();
	private string? serverName;
	private string? serverVersion;

	private static readonly Dictionary<string, Func<ViiperEmulator, GamepadDescriptor, Task<IGamepadConverter>>> Converters =
		new(StringComparer.OrdinalIgnoreCase)
		{
			{ "xbox360", (v, d) => v.CreateXbox360(d) },
			{ "dualshock4", (v, d) => v.CreateDualShock4(d) },
			{ "dualsense", (v, d) => v.CreateDualSense(d, false) },
			{ "dualsenseedge", (v, d) => v.CreateDualSense(d, true) },
		};

	public ViiperEmulator(string address, int port, string password)
	{
		client = new(address, port, password);
	}

	public async Task StartAsync()
	{
		PingResponse ping = await client.PingAsync();
		serverName = ping.Server;
		serverVersion = ping.Version;
	}
	
	public bool HasConverter(string id) => Converters.ContainsKey(id);

	public bool TryCreateConverter(string id, GamepadDescriptor descriptor, [NotNullWhen(true)] out IGamepadConverter? converter)
	{
		Func<ViiperEmulator, GamepadDescriptor, Task<IGamepadConverter>>? factory;
		if (!Converters.TryGetValue(id, out factory))
		{
			converter = null;
			return false;
		}

		converter = factory(this, descriptor).Result;
		return true;
	}

	private async Task<uint> FindOrCreateBus()
	{
		BusListResponse list = await client.BusListAsync();
		if (list.Buses.Length > 0) return list.Buses[0];

		uint id = (await client.BusCreateAsync(null)).BusID;
		buses.TryAdd(id, id);
		return id;
	}

	private async Task<ViiperDevice> CreateDevice(string type, ushort? vendorId, ushort? productId)
	{
		uint busId = await FindOrCreateBus();
		DeviceCreateRequest deviceReq = new()
		{
			Type = type,
			IDVendor = vendorId,
			IDProduct = productId,
		};

		Device busDevice = await client.BusDeviceAddAsync(busId, deviceReq);
		return await client.ConnectDeviceAsync(busId, busDevice.DevID);
	}
	
	private async Task<IGamepadConverter> CreateXbox360(GamepadDescriptor descriptor)
	{
		ViiperDevice device = await CreateDevice("xbox360", UsbIds.MicrosoftVendorId, UsbIds.MicrosoftXbox360WiredProductId);
		Xbox360ViiperTarget target = new(device, descriptor);
		_ = target.StartAsync();
		return new Xbox360Converter(target);
	}

	private async Task<IGamepadConverter> CreateDualShock4(GamepadDescriptor descriptor)
	{
		ViiperDevice device = await CreateDevice("dualshock4", UsbIds.SonyVendorId, UsbIds.SonyDualShock4Zct2ProductId);
		DualShock4ViiperTarget target = new(device, descriptor);
		_ = target.StartAsync();
		return new DualShock4Converter(target);
	}
	
	private async Task<IGamepadConverter> CreateDualSense(GamepadDescriptor descriptor, bool edge)
	{
		string name = edge ? "dualsenseedge" : "dualsense";
		ushort vid = edge ? UsbIds.SonyDualSenseEdgeProductId : UsbIds.SonyDualSenseProductId;
		
		ViiperDevice device = await CreateDevice(name, UsbIds.SonyVendorId, vid);
		DualSenseViiperTarget target = new(device, edge, descriptor);
		_ = target.StartAsync();
		return new DualSenseConverter(target);
	}

	public void Dispose()
	{
		foreach (uint bus in buses.Keys) client.BusRemoveAsync(bus).Wait();
		buses.Clear();

		client.Dispose();
	}
}
