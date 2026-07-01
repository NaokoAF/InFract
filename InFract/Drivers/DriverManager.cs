using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using InFract.Usb.LibUsb;
using InFract.Usb.LibUsb.Native;
using Microsoft.Extensions.Logging;
using static InFract.Usb.LibUsb.Native.libusb_error;
using static InFract.Usb.LibUsb.Native.libusb_hotplug_event;
using static InFract.Usb.LibUsb.Native.libusb_hotplug_flag;

namespace InFract.Drivers;

public class DriverManager : IDisposable
{
	public ImmutableArray<IDriver> Drivers => drivers;

	public event Action<IDriverDevice>? DeviceOpened;
	public event Action<IDriverDevice>? DeviceClosed;

	private readonly ILogger<DriverManager> logger;
	private readonly LibUsbContext libUsb;
	private readonly ImmutableArray<IDriver> drivers;
	private readonly List<IDriverDevice> devices = new();
	private readonly Dictionary<(byte, byte), IDriverDevice> deviceMap = new();
	private readonly Lock lockObject = new();
	private libusb_hotplug_callback_handle? hotplugCallbackHandle;

	public DriverManager(ILogger<DriverManager> logger, LibUsbContext libUsb, IEnumerable<IDriver> drivers)
	{
		this.logger = logger;
		this.libUsb = libUsb;
		this.drivers = drivers.ToImmutableArray();

		// list drivers
		logger.LogInformation($"Drivers: {string.Join(", ", this.drivers.Select(d => d.GetType().Name))}");
	}

	public void Start()
	{
		hotplugCallbackHandle = libUsb.RegisterHotPlugCallback(
			LIBUSB_HOTPLUG_EVENT_DEVICE_ARRIVED | LIBUSB_HOTPLUG_EVENT_DEVICE_LEFT,
			LIBUSB_HOTPLUG_ENUMERATE,
			OnHotplug
		);
	}

	public void Update()
	{
		libUsb.HandleEvents(500);

		lock (lockObject)
		{
			for (int i = devices.Count - 1; i >= 0; i--)
			{
				IDriverDevice driver = devices[i];
				bool close = false;

				try
				{
					driver.Update();
				}
				catch (LibUsbException e)
				{
					if (e.Error != LIBUSB_ERROR_NO_DEVICE) throw;
					close = true;
				}
				catch (Exception e)
				{
					logger.LogError(e, $"Driver error: {driver.Gamepad.Descriptor.Name}");
					close = true;
				}

				if (close) Close(driver);
			}
		}
	}

	private bool OnHotplug(LibUsbDevice device, libusb_hotplug_event type)
	{
		if (type != LIBUSB_HOTPLUG_EVENT_DEVICE_ARRIVED) return false;

		(byte, byte) identifier = (device.BusNumber, device.DeviceAddress);

		IDriverDevice driverDevice;
		lock (lockObject)
		{
			// skip already opened devices
			if (deviceMap.ContainsKey(identifier)) return false;

			// get driver for device
			if (!TryGetDriverForDevice(device, out IDriver? driver)) return false;

			// try to open driver
			LibUsbDeviceHandle? deviceHandle = null;
			try
			{
				deviceHandle = device.Open();
				driverDevice = driver.Open(deviceHandle);
			}
			catch (Exception e)
			{
				deviceHandle?.Dispose();
				logger.LogError(e, $"Failed to open driver: {driver.GetType().Name} [{device.BusNumber}-{device.DeviceAddress}]");
				return false;
			}

			logger.LogInformation($"Driver opened: {driverDevice.Gamepad.Descriptor.Name}");

			devices.Add(driverDevice);
			deviceMap.Add(identifier, driverDevice);
		}

		DeviceOpened?.Invoke(driverDevice);
		return false;
	}

	private void Close(IDriverDevice driver)
	{
		logger.LogInformation($"Driver closed: {driver.Gamepad.Descriptor.Name}");
		DeviceClosed?.Invoke(driver);
		
		LibUsbDevice usbDevice = driver.Device.Device;
		lock (lockObject)
		{
			deviceMap.Remove((usbDevice.BusNumber, usbDevice.DeviceAddress));
			devices.Remove(driver);
		}

		driver.Close();
		driver.Dispose();
	}

	private bool TryGetDriverForDevice(LibUsbDevice device, [NotNullWhen(true)] out IDriver? driver)
	{
		LibUsbDeviceDescriptor descriptor = device.GetDeviceDescriptor();
		foreach (IDriver other in drivers)
		{
			if (!other.IsSupported(device, descriptor)) continue;

			driver = other;
			return true;
		}

		driver = null;
		return false;
	}

	public void Dispose()
	{
		if (hotplugCallbackHandle != null) libUsb.DeregisterHotPlugCallback(hotplugCallbackHandle.Value);

		lock (lockObject)
			foreach (var device in devices)
				device.Dispose();
	}
}
