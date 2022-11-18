using System.Runtime.InteropServices;

namespace System;

[StructLayout(LayoutKind.Explicit)]
internal struct GuidBuffer
{
    public static readonly byte[] GUID_BUFFER = new byte[16];

    [FieldOffset(0)]
    private unsafe fixed ulong buffer[2];

    [FieldOffset(0)]
    public Guid GUID;

    public GuidBuffer(Guid GUID)
    {
        this = default(GuidBuffer);
        this.GUID = GUID;
    }

    public unsafe void Read(byte[] source, int offset)
    {
        if (offset + 16 > source.Length)
        {
            throw new ArgumentException("Destination buffer is too small!");
        }
        fixed (byte* ptr = source)
        {
            fixed (ulong* ptr3 = buffer)
            {
                ulong* ptr2 = (ulong*)(ptr + offset);
                *ptr3 = *ptr2;
                ptr3[1] = ptr2[1];
            }
        }
    }

    public unsafe void Write(byte[] destination, int offset)
    {
        if (offset + 16 > destination.Length)
        {
            throw new ArgumentException("Destination buffer is too small!");
        }
        fixed (byte* ptr = destination)
        {
            fixed (ulong* ptr3 = buffer)
            {
                ulong* ptr2 = (ulong*)(ptr + offset);
                *ptr2 = *ptr3;
                ptr2[1] = ptr3[1];
            }
        }
    }
}
