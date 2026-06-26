using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Viiper.Client;

namespace InFract.Emulators.Viiper;

public abstract class ViiperTarget<TInput, TOutput> : IDisposable
{
	private readonly ViiperDevice device;
	private readonly CancellationTokenSource cts = new();
	private readonly ManualResetEventSlim manualReset = new(false);
	private readonly ConcurrentQueue<TInput> inputQueue = new();
	private readonly byte[] inputBuffer = new byte[Unsafe.SizeOf<TInput>()];

	protected ViiperTarget(ViiperDevice device)
	{
		this.device = device;
		device.OnDisconnect += OnDeviceDisconnect;
		device.OnOutput += OnDeviceOutput;
	}

	public Task StartAsync() => Task.Factory.StartNew(
		() => Worker(cts.Token),
		cts.Token,
		TaskCreationOptions.LongRunning,
		TaskScheduler.Default
	);

	protected void EnqueueInput(in TInput input)
	{
		inputQueue.Enqueue(input);
		manualReset.Set();
	}

	protected virtual void OnOutputReceived(TOutput output)
	{
	}

	private async Task Worker(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			manualReset.Reset();

			while (inputQueue.TryDequeue(out TInput? input))
			{
				Unsafe.As<byte, TInput>(ref inputBuffer[0]) = input;
				await device.SendRawAsync(inputBuffer, token);
			}

			manualReset.Wait(token);
		}
	}

	private void OnDeviceDisconnect() => cts.Cancel();

	private Task OnDeviceOutput(Stream stream)
	{
		Unsafe.SkipInit(out TOutput output);
		Span<byte> bytes = MemoryMarshal.CreateSpan(
			ref Unsafe.As<TOutput, byte>(ref output),
			Unsafe.SizeOf<TOutput>()
		);
		stream.ReadExactly(bytes);

		OnOutputReceived(output);
		return Task.CompletedTask;
	}

	public void Dispose()
	{
		cts.Cancel();
		cts.Dispose();
		device.Dispose();
	}
}
