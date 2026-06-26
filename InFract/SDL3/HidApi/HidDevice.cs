using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL;
using static SDL.SDL3;

namespace InFract.SDL3.HidApi;

public unsafe class HidDevice : IDisposable
{
	public HidDeviceInfo Info { get; }

	private readonly SDL_hid_device* device;

	internal HidDevice(SDL_hid_device* device, HidDeviceInfo info)
	{
		this.device = device;
		Info = info;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Read(Span<byte> buffer, int timeout = 0)
	{
		byte* pointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
		return SDL_hid_read_timeout(device, pointer, (nuint)buffer.Length, timeout);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Write(ReadOnlySpan<byte> buffer)
	{
		byte* pointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
		return SDL_hid_write(device, pointer, (nuint)buffer.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetFeatureReport(Span<byte> buffer)
	{
		byte* pointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
		return SDL_hid_get_feature_report(device, pointer, (nuint)buffer.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int SetFeatureReport(Span<byte> buffer)
	{
		byte* pointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
		return SDL_hid_send_feature_report(device, pointer, (nuint)buffer.Length);
	}

	public void Dispose()
	{
		SDL_hid_close(device);
	}
}
