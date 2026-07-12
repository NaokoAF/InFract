using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using InFract.Platforms.Linux.Native;
using InFract.Platforms.Linux.Native.LibC;
using static InFract.Platforms.Linux.Native.LibC.LibC;

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
		fixed (byte* uhidPathPtr = UHidPath) fd = open(uhidPathPtr, O_RDWR | O_NONBLOCK, 0);

		if (fd < 0) throw new Exception(Marshal.GetLastPInvokeErrorMessage());

		pollfd = new() { fd = fd, events = POLLIN };

		// create device
		uhid_event uhidEvent = new();
		uhidEvent.type = (uint)uhid_event_type.UHID_CREATE2;

		ref uhid_create2_req create = ref uhidEvent.create2;
		create.rd_size = (ushort)reportDescriptor.Length;
		create.bus = (ushort)bus;
		create.vendor = vendorId;
		create.product = productId;
		Encoding.UTF8.GetBytes(name, create.name[..127]); // skip last byte to ensure null terminator
		reportDescriptor.CopyTo(create.rd_data);

		Write(uhidEvent);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void WriteInput(ReadOnlySpan<byte> data)
	{
		Unsafe.SkipInit(out uhid_event uhidEvent);
		uhidEvent.type = (uint)uhid_event_type.UHID_INPUT2;
		uhidEvent.input2.size = (ushort)data.Length;
		data.CopyTo(uhidEvent.input2.data);

		Write(uhidEvent);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void WriteInput<T>(byte reportId, T data)
	{
		Unsafe.SkipInit(out uhid_event uhidEvent);
		uhidEvent.type = (uint)uhid_event_type.UHID_INPUT2;
		uhidEvent.input2.size = (ushort)(Unsafe.SizeOf<T>() + 1);

		// create report
		uhidEvent.input2.data[0] = reportId;
		Unsafe.CopyBlockUnaligned(
			ref uhidEvent.input2.data[1],
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
		pollfd pollfd = this.pollfd;
		while (poll((pollfd*)Unsafe.AsPointer(in pollfd), 1, timeout) > 0)
		{
			// read from file descriptor
			Unsafe.SkipInit(out uhid_event uhidEvent);
			nint readBytes = read(fd, &uhidEvent, (nuint)sizeof(uhid_event));
			if (readBytes != sizeof(uhid_event))
			{
				if (readBytes == -1)
				{
					int error = Marshal.GetLastPInvokeError();
					if(error != EWOULDBLOCK)
						throw new ErrnoException(error, "Failed to read from UHid");
				}
				continue;
			}
			
			ReadOnlySpan<byte> data;
			switch ((uhid_event_type)uhidEvent.type)
			{
				case uhid_event_type.UHID_OUTPUT:
					data = MemoryMarshal.CreateReadOnlySpan(ref uhidEvent.output.data.e0, uhidEvent.output.size);
					OnOutputReport(data);
					break;
				case uhid_event_type.UHID_GET_REPORT:
					Unsafe.SkipInit(out uhid_get_report_reply_req getReportReply);
					getReportReply.id = uhidEvent.get_report.id;

					data = OnGetReport(uhidEvent.get_report.rnum);
					data.CopyTo(getReportReply.data);
					getReportReply.err = (ushort)(data.Length > 0 ? 0 : -EINVAL);
					getReportReply.size = (ushort)data.Length;

					Write(
						new()
						{
							type = (uint)uhid_event_type.UHID_GET_REPORT_REPLY,
							get_report_reply = getReportReply,
						}
					);
					break;
				case uhid_event_type.UHID_SET_REPORT:
					data = MemoryMarshal.CreateReadOnlySpan(ref uhidEvent.set_report.data.e0, uhidEvent.set_report.size);

					Write(
						new()
						{
							type = (uint)uhid_event_type.UHID_SET_REPORT_REPLY,
							set_report_reply = new()
							{
								id = uhidEvent.set_report.id,
								err = (ushort)(OnSetReport(uhidEvent.set_report.rnum, data)
									? 0
									: -EINVAL),
							},
						}
					);
					break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Write(in uhid_event uhidEvent)
	{
		nint written = write(fd, Unsafe.AsPointer(in uhidEvent), (nuint)sizeof(uhid_event));
		if (written != sizeof(uhid_event)) throw new Exception(Marshal.GetLastPInvokeErrorMessage());
	}

	public void Close() => close(fd);

	public void Dispose()
	{
		Close();
		GC.SuppressFinalize(this);
	}

	~UHidDevice()
	{
		Close();
	}
}
