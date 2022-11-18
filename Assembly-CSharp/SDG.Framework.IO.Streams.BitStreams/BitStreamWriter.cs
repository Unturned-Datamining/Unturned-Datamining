using System.IO;

namespace SDG.Framework.IO.Streams.BitStreams;

public class BitStreamWriter
{
    public Stream stream { get; protected set; }

    private byte buffer { get; set; }

    private byte bitIndex { get; set; }

    private byte bitsAvailable { get; set; }

    public void writeBit(byte data)
    {
        writeBits(data, 1);
    }

    public void writeBits(byte data, byte length)
    {
        if (length > bitsAvailable)
        {
            byte b = (byte)(length - bitsAvailable);
            writeBits((byte)(data >> (int)b), bitsAvailable);
            writeBits(data, b);
            return;
        }
        byte b2 = (byte)(8 - length - bitIndex);
        byte b3 = (byte)(255 >> 8 - length);
        buffer |= (byte)((data & b3) << (int)b2);
        bitIndex += length;
        bitsAvailable -= length;
        if (bitIndex == 8 && bitsAvailable == 0)
        {
            emptyBuffer();
        }
    }

    private void emptyBuffer()
    {
        stream.WriteByte(buffer);
        reset();
    }

    public void flush()
    {
        if (bitsAvailable != 8)
        {
            emptyBuffer();
        }
    }

    public void reset()
    {
        buffer = 0;
        bitIndex = 0;
        bitsAvailable = 8;
    }

    public BitStreamWriter(Stream newStream)
    {
        stream = newStream;
        reset();
    }
}
