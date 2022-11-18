using System.IO;

namespace SDG.Framework.IO.Streams.BitStreams;

public class BitStreamReader
{
    public Stream stream { get; protected set; }

    private byte buffer { get; set; }

    private byte bitIndex { get; set; }

    private byte bitsAvailable { get; set; }

    public void readBit(ref byte data)
    {
        readBits(ref data, 1);
    }

    public void readBits(ref byte data, byte length)
    {
        if (bitIndex == 8 && bitsAvailable == 0)
        {
            fillBuffer();
        }
        if (length > bitsAvailable)
        {
            byte b = (byte)(length - bitsAvailable);
            readBits(ref data, bitsAvailable);
            data <<= (int)b;
            readBits(ref data, b);
        }
        else
        {
            byte b2 = (byte)(8 - length - bitIndex);
            byte b3 = (byte)(255 >> 8 - length);
            data |= (byte)((buffer >> (int)b2) & b3);
            bitIndex += length;
            bitsAvailable -= length;
        }
    }

    private void fillBuffer()
    {
        buffer = (byte)stream.ReadByte();
        bitIndex = 0;
        bitsAvailable = 8;
    }

    public void reset()
    {
        buffer = 0;
        bitIndex = 8;
        bitsAvailable = 0;
    }

    public BitStreamReader(Stream newStream)
    {
        stream = newStream;
        reset();
    }
}
