using System;
using System.Runtime.InteropServices;

namespace SDG.NetPak;

public class NetPakWriter
{
    [Flags]
    public enum EErrorFlags
    {
        None = 0,
        BufferOverflow = 1
    }

    public byte[] buffer;

    private ulong scratch;

    public int writeByteIndex;

    public int scratchBitCount;

    public EErrorFlags errors;

    public void Reset()
    {
        scratch = 0uL;
        writeByteIndex = 0;
        scratchBitCount = 0;
        errors = EErrorFlags.None;
    }

    public bool WriteBit(bool value)
    {
        if (value)
        {
            scratch |= (ulong)(1L << scratchBitCount);
        }
        scratchBitCount++;
        if (scratchBitCount >= 32)
        {
            return FlushLowBits();
        }
        return true;
    }

    public bool WriteBits(uint value, int valueBitCount)
    {
        ulong num = (ulong)((1L << valueBitCount) - 1);
        scratch |= (value & num) << scratchBitCount;
        scratchBitCount += valueBitCount;
        if (scratchBitCount >= 32)
        {
            return FlushLowBits();
        }
        return true;
    }

    public bool Flush()
    {
        if (scratchBitCount < 1)
        {
            return true;
        }
        int num = (scratchBitCount - 1) / 8 + 1;
        int num2 = buffer.Length - writeByteIndex;
        if (num > num2)
        {
            errors |= EErrorFlags.BufferOverflow;
            return false;
        }
        switch (num)
        {
        case 1:
            buffer[writeByteIndex] = (byte)scratch;
            break;
        case 2:
            buffer[writeByteIndex] = (byte)scratch;
            buffer[writeByteIndex + 1] = (byte)(scratch >> 8);
            break;
        case 3:
            buffer[writeByteIndex] = (byte)scratch;
            buffer[writeByteIndex + 1] = (byte)(scratch >> 8);
            buffer[writeByteIndex + 2] = (byte)(scratch >> 16);
            break;
        case 4:
            buffer[writeByteIndex] = (byte)scratch;
            buffer[writeByteIndex + 1] = (byte)(scratch >> 8);
            buffer[writeByteIndex + 2] = (byte)(scratch >> 16);
            buffer[writeByteIndex + 3] = (byte)(scratch >> 24);
            break;
        }
        writeByteIndex += num;
        scratch = 0uL;
        scratchBitCount = 0;
        return true;
    }

    public bool AlignToByte()
    {
        int num = scratchBitCount % 8;
        if (num != 0)
        {
            return WriteBits(0u, 8 - num);
        }
        return true;
    }

    public bool WriteBytes(byte[] bytes)
    {
        return WriteBytes(bytes, bytes.Length);
    }

    public bool WriteBytes(byte[] bytes, int length)
    {
        return WriteBytes(bytes, 0, length);
    }

    public unsafe bool WriteBytes(byte[] bytes, int offset, int length)
    {
        if (length < 1)
        {
            return true;
        }
        if (!AlignToByte())
        {
            return false;
        }
        if (!Flush())
        {
            return false;
        }
        if (writeByteIndex + length > buffer.Length)
        {
            errors |= EErrorFlags.BufferOverflow;
            return false;
        }
        fixed (byte* ptr = bytes)
        {
            fixed (byte* ptr2 = buffer)
            {
                byte* source = ptr + offset;
                byte* destination = ptr2 + writeByteIndex;
                long destinationSizeInBytes = buffer.Length - writeByteIndex;
                Buffer.MemoryCopy(source, destination, destinationSizeInBytes, length);
                writeByteIndex += length;
            }
        }
        return true;
    }

    public bool WriteBytes(IntPtr bytesPtr, int length)
    {
        if (length < 1)
        {
            return true;
        }
        if (!AlignToByte())
        {
            return false;
        }
        if (!Flush())
        {
            return false;
        }
        if (writeByteIndex + length > buffer.Length)
        {
            errors |= EErrorFlags.BufferOverflow;
            return false;
        }
        Marshal.Copy(bytesPtr, buffer, writeByteIndex, length);
        writeByteIndex += length;
        return true;
    }

    private bool FlushLowBits()
    {
        if (buffer.Length - writeByteIndex < 4)
        {
            errors |= EErrorFlags.BufferOverflow;
            return false;
        }
        buffer[writeByteIndex] = (byte)scratch;
        buffer[writeByteIndex + 1] = (byte)(scratch >> 8);
        buffer[writeByteIndex + 2] = (byte)(scratch >> 16);
        buffer[writeByteIndex + 3] = (byte)(scratch >> 24);
        writeByteIndex += 4;
        scratch >>= 32;
        scratchBitCount -= 32;
        return true;
    }
}
