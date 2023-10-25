using System;

namespace SDG.NetPak;

/// <summary>
/// Unpacks bits from a byte array into a 32-bit buffer value. GafferOnGames recommends this approach rather than
/// "farting across a buffer at byte level like it's 1985".
/// </summary>
public class NetPakReader
{
    /// <summary>
    /// Lightweight error when exceptions are disabled. Bitwise OR to prevent different errors from clobbering each other. 
    /// </summary>
    [Flags]
    public enum EErrorFlags
    {
        None = 0,
        /// <summary>
        /// Call to ReadBits or ReadBytes would have overflowed our buffer.
        /// </summary>
        SourceBufferOverflow = 1,
        /// <summary>
        /// Buffer passed into ReadBytes would have overflowed.
        /// </summary>
        DestinationBufferOverflow = 2,
        /// <summary>
        /// AlignToByte bits should be zero.
        /// </summary>
        AlignmentPadding = 4,
        /// <summary>
        /// Buffer passed into SaveState would have overflowed.
        /// </summary>
        SaveStateBufferOverflow = 8
    }

    private byte[] buffer;

    private ulong scratch;

    private int bufferLength;

    public int readByteIndex;

    public int scratchBitCount;

    public EErrorFlags errors;

    /// <summary>
    /// Imprecise because sent byte length is rounded up from bit length, but should help find particularly
    /// egregious reading errors.
    /// </summary>
    public bool ReachedEndOfSegment => readByteIndex == bufferLength;

    /// <summary>
    /// Number of bytes until end of segment is reached.
    /// </summary>
    public int RemainingSegmentLength => bufferLength - readByteIndex;

    /// <summary>
    /// Save remaining data to resume reading later. Used by net invokables to defer invocation.
    /// </summary>
    public unsafe bool SaveState(out uint scratch, out int scratchBitCount, byte[] buffer)
    {
        long num = RemainingSegmentLength;
        if (num > buffer.Length)
        {
            scratch = 0u;
            scratchBitCount = 0;
            return false;
        }
        scratch = (uint)this.scratch;
        scratchBitCount = this.scratchBitCount;
        if (num > 0)
        {
            fixed (byte* ptr = this.buffer)
            {
                fixed (byte* destination = buffer)
                {
                    byte* source = ptr + readByteIndex;
                    long destinationSizeInBytes = buffer.LongLength;
                    Buffer.MemoryCopy(source, destination, destinationSizeInBytes, num);
                }
            }
        }
        readByteIndex = bufferLength;
        return true;
    }

    public void LoadState(uint scratch, int scratchBitCount, byte[] buffer, int bufferLength)
    {
        this.scratch = scratch;
        readByteIndex = 0;
        this.scratchBitCount = scratchBitCount;
        errors = EErrorFlags.None;
        this.buffer = buffer;
        this.bufferLength = bufferLength;
    }

    public void Reset()
    {
        scratch = 0uL;
        readByteIndex = 0;
        scratchBitCount = 0;
        errors = EErrorFlags.None;
    }

    /// <summary>
    /// Used by invocation messages to show more error context rather than the default.
    /// </summary>
    public void ResetErrors()
    {
        errors = EErrorFlags.None;
        readByteIndex = bufferLength;
    }

    public int GetBufferSegmentLength()
    {
        return bufferLength;
    }

    public void SetBuffer(byte[] buffer)
    {
        this.buffer = buffer;
        bufferLength = buffer.Length;
    }

    public void SetBufferSegment(byte[] buffer, int bufferLength)
    {
        this.buffer = buffer;
        this.bufferLength = bufferLength;
    }

    /// <summary>
    /// Used by NetInvokable loopback to copy buffer from writer to reader.
    /// </summary>
    public unsafe void SetBufferSegmentCopy(byte[] sourceBuffer, byte[] destinationBuffer, int bufferLength)
    {
        buffer = destinationBuffer;
        this.bufferLength = bufferLength;
        fixed (byte* source = sourceBuffer)
        {
            fixed (byte* destination = destinationBuffer)
            {
                Buffer.MemoryCopy(source, destination, destinationBuffer.Length, bufferLength);
            }
        }
    }

    public bool ReadBit(out bool value)
    {
        uint value2;
        bool result = ReadBits(1, out value2);
        value = value2 == 1;
        return result;
    }

    public bool ReadBits(int valueBitCount, out uint value)
    {
        if (valueBitCount > scratchBitCount)
        {
            int num = bufferLength - readByteIndex;
            ulong num2;
            switch (num)
            {
            case 0:
                value = 0u;
                errors |= EErrorFlags.SourceBufferOverflow;
                return false;
            case 1:
                num2 = buffer[readByteIndex];
                break;
            case 2:
                num2 = buffer[readByteIndex] | ((ulong)buffer[readByteIndex + 1] << 8);
                break;
            case 3:
                num2 = buffer[readByteIndex] | ((ulong)buffer[readByteIndex + 1] << 8) | ((ulong)buffer[readByteIndex + 2] << 16);
                break;
            case 4:
                num2 = buffer[readByteIndex] | ((ulong)buffer[readByteIndex + 1] << 8) | ((ulong)buffer[readByteIndex + 2] << 16) | ((ulong)buffer[readByteIndex + 3] << 24);
                break;
            default:
                num = 4;
                num2 = buffer[readByteIndex] | ((ulong)buffer[readByteIndex + 1] << 8) | ((ulong)buffer[readByteIndex + 2] << 16) | ((ulong)buffer[readByteIndex + 3] << 24);
                break;
            }
            num2 <<= scratchBitCount;
            scratch |= num2;
            scratchBitCount += num * 8;
            readByteIndex += num;
            if (valueBitCount > scratchBitCount)
            {
                value = 0u;
                errors |= EErrorFlags.SourceBufferOverflow;
                return false;
            }
        }
        ulong num3 = (ulong)((1L << valueBitCount) - 1);
        value = (uint)(scratch & num3);
        scratch >>= valueBitCount;
        scratchBitCount -= valueBitCount;
        return true;
    }

    public bool AlignToByte()
    {
        int num = scratchBitCount % 8;
        if (num != 0)
        {
            uint value;
            bool result = ReadBits(num, out value) && value == 0;
            errors |= (EErrorFlags)((value != 0) ? 4 : 0);
            return result;
        }
        return true;
    }

    /// <summary>
    /// Assumes length is greater than zero!
    /// Moves reader forward according to length.
    /// </summary>
    public bool ReadBytesPtr(int length, out byte[] source, out int bufferOffset)
    {
        if (!AlignToByte())
        {
            source = null;
            bufferOffset = 0;
            return false;
        }
        int num = scratchBitCount / 8;
        bufferOffset = readByteIndex - num;
        if (bufferOffset + length > bufferLength)
        {
            source = null;
            errors |= EErrorFlags.SourceBufferOverflow;
            return false;
        }
        if (length >= num)
        {
            readByteIndex = bufferOffset + length;
            scratch = 0uL;
            scratchBitCount = 0;
        }
        else
        {
            int num2 = length * 8;
            scratch >>= num2;
            scratchBitCount -= num2;
        }
        source = buffer;
        return true;
    }

    public bool ReadBytes(byte[] destination)
    {
        return ReadBytes(destination, destination.Length);
    }

    public unsafe bool ReadBytes(byte[] destination, int length)
    {
        if (length > destination.Length)
        {
            errors |= EErrorFlags.DestinationBufferOverflow;
            return false;
        }
        if (length < 1)
        {
            return true;
        }
        if (ReadBytesPtr(length, out var source, out var bufferOffset))
        {
            fixed (byte* ptr = source)
            {
                fixed (byte* destination2 = destination)
                {
                    byte* source2 = ptr + bufferOffset;
                    long destinationSizeInBytes = destination.LongLength;
                    long sourceBytesToCopy = length;
                    Buffer.MemoryCopy(source2, destination2, destinationSizeInBytes, sourceBytesToCopy);
                }
            }
            return true;
        }
        return false;
    }
}
