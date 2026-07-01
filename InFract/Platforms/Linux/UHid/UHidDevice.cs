using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Tmds.Linux;

namespace InFract.Platforms.Linux.UHid;

public abstract unsafe class UHidDevice : IDisposable
{
	private readonly int fd;
	private readonly pollfd pollfd;

	private static ReadOnlySpan<byte> UHidPath => "/dev/uhid"u8;

	protected UHidDevice(
		ReadOnlySpan<char> name,
		ushort vendorId,
		ushort productId,
		UHidBusType bus,
		ReadOnlySpan<byte> reportDescriptor
	)
	{
		// open file descriptor
		fixed (byte* uhidPathPtr = UHidPath) fd = LibC.open(uhidPathPtr, LibC.O_RDWR | LibC.O_NONBLOCK);

		if (fd < 0) throw new Exception(Marshal.GetLastPInvokeErrorMessage());

		pollfd = new() { fd = fd, events = LibC.POLLIN };

		// create device
		UHidEvent uhidEvent = new();
		uhidEvent.Type = UHidEventType.Create2;

		ref UHidCreate2 create = ref uhidEvent.Create2;
		create.DescriptorSize = (ushort)reportDescriptor.Length;
		create.Bus = bus;
		create.Vendor = vendorId;
		create.Product = productId;
		Encoding.UTF8.GetBytes(name, create.Name[..127]); // skip last byte to ensure null terminator
		reportDescriptor.CopyTo(create.Descriptor);

		Write(uhidEvent);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void WriteInput(ReadOnlySpan<byte> data)
	{
		UHidEvent uhidEvent = default;
		uhidEvent.Type = UHidEventType.Input2;
		uhidEvent.Input2.Size = (ushort)data.Length;
		data.CopyTo(uhidEvent.Input2.Data);

		Write(uhidEvent);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void WriteInput<T>(byte reportId, T data)
	{
		UHidEvent uhidEvent = default;
		uhidEvent.Type = UHidEventType.Input2;
		uhidEvent.Input2.Size = (ushort)(Unsafe.SizeOf<T>() + 1);

		// create report
		uhidEvent.Input2.Data[0] = reportId;
		Unsafe.CopyBlockUnaligned(
			ref uhidEvent.Input2.Data[1],
			ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in data)),
			(uint)Unsafe.SizeOf<T>()
		);
		
		Write(uhidEvent);
	}

	protected virtual void OnOutputReport(ReadOnlySpan<byte> data)
	{
	}

	protected virtual ReadOnlySpan<byte> OnGetReport(byte reportId) => [];
	protected virtual bool OnSetReport(byte reportId, ReadOnlySpan<byte> data) => false;

	public void Poll(int timeout = 0)
	{
		while ((LibC.poll((pollfd*)Unsafe.AsPointer(ref Unsafe.AsRef(in pollfd)), 1, timeout)) > 0)
		{
			// read from file descriptor
			Unsafe.SkipInit(out UHidEvent uhidEvent);
			ssize_t readBytes = LibC.read(fd, &uhidEvent, sizeof(UHidEvent));
			if (readBytes != sizeof(UHidEvent))
			{
				// -1 means an error. EAGAIN occurs if the read would block.
				if (readBytes == -1 && (Marshal.GetLastPInvokeError()) != LibC.EAGAIN)
					throw new Exception($"Failed to read from UHid: {Marshal.GetLastPInvokeErrorMessage()}");

				// we read a partial event?
				if (readBytes > 0) throw new Exception($"Partial UHid read: {readBytes}");
			}
			else
			{
				// process event
				switch (uhidEvent.Type)
				{
					case UHidEventType.Output: OnOutputReport(uhidEvent.Output.DataSpan); break;
					case UHidEventType.GetReport:
						UHidGetReportReply getReportReply = default;
						getReportReply.Id = uhidEvent.GetReport.Id;

						ReadOnlySpan<byte> getReportData = OnGetReport(uhidEvent.GetReport.ReportNum);
						getReportReply.Err = (ushort)(getReportData.Length > 0 ? 0 : -LibC.EINVAL);
						getReportReply.Size = (ushort)getReportData.Length;
						getReportData.CopyTo(getReportReply.Data);

						Write(
							new()
							{
								Type = UHidEventType.GetReportReply,
								GetReportReply = getReportReply,
							}
						);
						break;
					case UHidEventType.SetReport:
						Write(
							new()
							{
								Type = UHidEventType.SetReportReply,
								SetReportReply = new()
								{
									Id = uhidEvent.SetReport.Id,
									Err = (ushort)(OnSetReport(uhidEvent.SetReport.ReportNum, uhidEvent.SetReport.DataSpan)
										? 0
										: -LibC.EINVAL),
								},
							}
						);
						break;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Write(in UHidEvent uhidEvent)
	{
		ssize_t written = LibC.write(fd, Unsafe.AsPointer(in uhidEvent), Unsafe.SizeOf<UHidEvent>());
		if (written != Unsafe.SizeOf<UHidEvent>()) throw new Exception(Marshal.GetLastPInvokeErrorMessage());
	}

	protected virtual void Dispose(bool disposing)
	{
		LibC.close(fd);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	~UHidDevice()
	{
		Dispose(false);
	}
}
