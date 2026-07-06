using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace InFract.Platforms.Linux.Native;

public unsafe ref struct Utf8String
{
	private readonly ref byte pointer;

	private Utf8String(ref byte pointer) => this.pointer = ref pointer;

	private Utf8String(ReadOnlySpan<char> str)
	{
		Span<byte> bytes = new byte[Encoding.UTF8.GetByteCount(str) + 1];
		int length = Encoding.UTF8.GetBytes(str, bytes);
		bytes[length] = 0x00;

		pointer = ref bytes[0];
	}
	
	public byte* AsPointer() => (byte*)(nint)Unsafe.AsPointer(ref pointer);
	
	public ReadOnlySpan<byte> AsSpan() => MemoryMarshal.CreateReadOnlySpanFromNullTerminated(AsPointer());
	
	public override string ToString() => Marshal.PtrToStringUTF8((nint)Unsafe.AsPointer(ref pointer))!;

	public static implicit operator byte*(Utf8String self) => self.AsPointer();
	public static implicit operator ReadOnlySpan<byte>(Utf8String self) => self.AsSpan();
	public static implicit operator Utf8String(byte* pointer) => new(ref Unsafe.AsRef<byte>(pointer));
	public static implicit operator Utf8String(ReadOnlySpan<char> str) => new(str);
	public static implicit operator Utf8String(ReadOnlySpan<byte> str) => new(ref MemoryMarshal.GetReference(str));
}
