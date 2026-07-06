using System.Runtime.InteropServices;
using InFract.Platforms.Linux.Native;
using InFract.Platforms.Linux.Native.LibC;
using InFract.Usb.Hid;
using static InFract.Platforms.Linux.Native.LibC.LibC;

namespace InFract.Platforms.Linux.HidRaw;

public unsafe class HidRawContext
{
	public event Action<HidRawDevice>? DeviceClosed;

	private readonly List<HidRawDevice> devices = new();
	private readonly List<pollfd> pollfds = new();

	public HidRawDevice Open(Utf8String path)
	{
		HidRawDevice device = HidRawDevice.Open(path, false);
		pollfd pollfd = new()
		{
			fd = device.FileDescriptor,
			events = POLLIN,
		};

		devices.Add(device);
		pollfds.Add(pollfd);
		return device;
	}

	public bool Poll(int timeout = 0)
	{
		int ret;
		fixed (pollfd* pollfdsPtr = CollectionsMarshal.AsSpan(pollfds)) ret = poll(pollfdsPtr, (ulong)pollfds.Count, timeout);
		if (ret < 0) throw new ErrnoException();
		if (ret == 0) return false;

		// loop backwards for removal
		for (int i = pollfds.Count - 1; i >= 0; i--)
		{
			pollfd pollfd = pollfds[i];
			if (pollfd.revents == 0) continue;

			HidRawDevice device = devices[i];
			if ((pollfd.revents & POLLIN) == POLLIN)
			{
				if (device.Poll()) continue;
			}

			// if we get here, either the device read failed, or a POLLERR/POLLHUP/POLLNVAL happened
			DeviceClosed?.Invoke(device);
			device.Dispose();

			devices.RemoveAt(i);
			pollfds.RemoveAt(i);
		}

		return true;
	}
}
