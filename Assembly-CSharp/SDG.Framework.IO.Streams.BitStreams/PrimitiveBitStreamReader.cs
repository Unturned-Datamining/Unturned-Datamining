using System.IO;

namespace SDG.Framework.IO.Streams.BitStreams;

public class PrimitiveBitStreamReader : BitStreamReader
{
    public void readByte(ref byte data)
    {
        readBits(ref data, 8);
    }

    public void readInt16(ref short data)
    {
        byte data2 = 0;
        byte data3 = 0;
        readByte(ref data2);
        readByte(ref data3);
        data = (short)((data2 << 8) | data3);
    }

    public void readInt16(ref short data, byte length)
    {
        if (length == 16)
        {
            readInt16(ref data);
        }
        else if (length > 8)
        {
            byte data2 = 0;
            byte data3 = 0;
            readBits(ref data2, (byte)(length - 8));
            readByte(ref data3);
            data = (short)((data2 << 8) | data3);
        }
        else if (length == 8)
        {
            byte data4 = 0;
            readByte(ref data4);
            data = data4;
        }
        else
        {
            byte data5 = 0;
            readBits(ref data5, length);
            data = data5;
        }
    }

    public void readUInt16(ref ushort data)
    {
        byte data2 = 0;
        byte data3 = 0;
        readByte(ref data2);
        readByte(ref data3);
        data = (ushort)((data2 << 8) | data3);
    }

    public void readUInt16(ref ushort data, byte length)
    {
        if (length == 16)
        {
            readUInt16(ref data);
        }
        else if (length > 8)
        {
            byte data2 = 0;
            byte data3 = 0;
            readBits(ref data2, (byte)(length - 8));
            readByte(ref data3);
            data = (ushort)((data2 << 8) | data3);
        }
        else if (length == 8)
        {
            byte data4 = 0;
            readByte(ref data4);
            data = data4;
        }
        else
        {
            byte data5 = 0;
            readBits(ref data5, length);
            data = data5;
        }
    }

    public void readInt32(ref int data)
    {
        byte data2 = 0;
        byte data3 = 0;
        byte data4 = 0;
        byte data5 = 0;
        readByte(ref data2);
        readByte(ref data3);
        readByte(ref data4);
        readByte(ref data5);
        data = (data2 << 24) | (data3 << 16) | (data4 << 8) | data5;
    }

    public void readInt32(ref int data, byte length)
    {
        if (length == 32)
        {
            readInt32(ref data);
        }
        else if (length > 24)
        {
            byte data2 = 0;
            byte data3 = 0;
            byte data4 = 0;
            byte data5 = 0;
            readBits(ref data2, (byte)(length - 8));
            readByte(ref data3);
            readByte(ref data4);
            readByte(ref data5);
            data = (data2 << 24) | (data3 << 16) | (data4 << 8) | data5;
        }
        else if (length == 24)
        {
            byte data6 = 0;
            byte data7 = 0;
            byte data8 = 0;
            readByte(ref data6);
            readByte(ref data7);
            readByte(ref data8);
            data = (data6 << 16) | (data7 << 8) | data8;
        }
        else if (length > 16)
        {
            byte data9 = 0;
            byte data10 = 0;
            byte data11 = 0;
            readBits(ref data9, (byte)(length - 8));
            readByte(ref data10);
            readByte(ref data11);
            data = (data9 << 16) | (data10 << 8) | data11;
        }
        else if (length == 16)
        {
            byte data12 = 0;
            byte data13 = 0;
            readByte(ref data12);
            readByte(ref data13);
            data = (data12 << 8) | data13;
        }
        else if (length > 8)
        {
            byte data14 = 0;
            byte data15 = 0;
            readBits(ref data14, (byte)(length - 8));
            readByte(ref data15);
            data = (data14 << 8) | data15;
        }
        else if (length == 8)
        {
            byte data16 = 0;
            readByte(ref data16);
            data = data16;
        }
        else
        {
            byte data17 = 0;
            readBits(ref data17, length);
            data = data17;
        }
    }

    public PrimitiveBitStreamReader(Stream newStream)
        : base(newStream)
    {
    }
}
