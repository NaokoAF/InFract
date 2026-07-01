using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Viiper.Client;

namespace InFract.Platforms.Windows.Viiper;

public abstract class ViiperTarget<TInput, TOutput> : IDisposable
{
	private readonly ViiperDevice device;
	private readonly CancellationTokenSource cts = new();
	private readonly ManualResetEventSlim manualReset = new(false);
	private readonly ConcurrentQueue<TInput> inputQueue = new();
	private readonly byte[] inputBuffer = new byte[Unsafe.SizeOf<TInput>()];
	private readonly byte[] outputBuffer = new byte[Unsafe.SizeOf<TOutput>()];

	protected ViiperTarget(ViiperDevice device)
	{
		this.device = device;
		device.OnDisconnect += OnDeviceDisconnect;
		device.OnOutput += OnDeviceOutput;
	}

	public Task StartAsync() => Task.Factory.StartNew(
		WriteLoop,
		cts.Token,
		TaskCreationOptions.LongRunning,
		TaskScheduler.Default
	);

	protected abstract void OnOutputReceived(TOutput output);
	
	protected void EnqueueInput(in TInput input)
	{
		inputQueue.Enqueue(input);
		manualReset.Set();
	}

	private async Task WriteLoop()
	{
		while (!cts.Token.IsCancellationRequested)
		{
			manualReset.Reset();

			while (inputQueue.TryDequeue(out TInput? input) && !cts.Token.IsCancellationRequested)
			{
				Unsafe.As<byte, TInput>(ref inputBuffer[0]) = input;
				await device.SendRawAsync(inputBuffer, cts.Token);
			}

			manualReset.Wait(cts.Token);
		}
	}

	private void OnDeviceDisconnect() => cts.Cancel();

	private async Task OnDeviceOutput(Stream stream)
	{
		await stream.ReadExactlyAsync(outputBuffer, cts.Token);
		
		ref TOutput output = ref Unsafe.As<byte, TOutput>(ref outputBuffer[0]);
		OnOutputReceived(output);
	}

	public void Dispose()
	{
		device.OnDisconnect = null;
		device.OnOutput = null;
		
		cts.Cancel();
		cts.Dispose();
		inputQueue.Clear();
		manualReset.Set();
		manualReset.Dispose();
		
		device.Dispose();
	}
}
