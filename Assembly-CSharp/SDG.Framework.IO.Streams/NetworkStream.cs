using System.IO;

namespace SDG.Framework.IO.Streams;

public class NetworkStream
{
    private Stream stream { get; set; }

    public sbyte readSByte()
    {
        return (sbyte)stream.ReadByte();
    }

    public byte readByte()
    {
        return (byte)stream.ReadByte();
    }

    public short readInt16()
    {
        byte num = readByte();
        byte b = readByte();
        return (short)((num << 8) | b);
    }

    public ushort readUInt16()
    {
        byte num = readByte();
        byte b = readByte();
        return (ushort)((num << 8) | b);
    }

    public int readInt32()
    {
        byte num = readByte();
        byte b = readByte();
        byte b2 = readByte();
        byte b3 = readByte();
        return (num << 24) | (b << 16) | (b2 << 8) | b3;
    }

    public uint readUInt32()
    {
        byte num = readByte();
        byte b = readByte();
        byte b2 = readByte();
        byte b3 = readByte();
        return (uint)((num << 24) | (b << 16) | (b2 << 8) | b3);
    }

    public long readInt64()
    {
        byte num = readByte();
        byte b = readByte();
        byte b2 = readByte();
        byte b3 = readByte();
        byte b4 = readByte();
        byte b5 = readByte();
        byte b6 = readByte();
        byte b7 = readByte();
        return (num << 24) | (b << 16) | (b2 << 8) | b3 | (b4 << 24) | (b5 << 16) | (b6 << 8) | b7;
    }

    public ulong readUInt64()
    {
        byte num = readByte();
        byte b = readByte();
        byte b2 = readByte();
        byte b3 = readByte();
        byte b4 = readByte();
        byte b5 = readByte();
        byte b6 = readByte();
        byte b7 = readByte();
        return (ulong)((num << 24) | (b << 16) | (b2 << 8) | b3 | (b4 << 24) | (b5 << 16) | (b6 << 8) | b7);
    }

    public char readChar()
    {
        return (char)readUInt16();
    }

    public string readString()
    {
        ushort num = readUInt16();
        char[] array = new char[num];
        for (ushort num2 = 0; num2 < num; num2++)
        {
            char c = readChar();
            array[num2] = c;
        }
        return new string(array);
    }

    public void readBytes(byte[] data, ulong offset, ulong length)
    {
        stream.Read(data, (int)offset, (int)length);
    }

    public void writeSByte(sbyte data)
    {
        stream.WriteByte((byte)data);
    }

    public void writeByte(byte data)
    {
        stream.WriteByte(data);
    }

    public void writeInt16(short data)
    {
        writeByte((byte)(data >> 8));
        writeByte((byte)data);
    }

    public void writeUInt16(ushort data)
    {
        writeByte((byte)(data >> 8));
        writeByte((byte)data);
    }

    public void writeInt32(int data)
    {
        writeByte((byte)(data >> 24));
        writeByte((byte)(data >> 16));
        writeByte((byte)(data >> 8));
        writeByte((byte)data);
    }

    public void writeUInt32(uint data)
    {
        writeByte((byte)(data >> 24));
        writeByte((byte)(data >> 16));
        writeByte((byte)(data >> 8));
        writeByte((byte)data);
    }

    public void writeInt64(long data)
    {
        writeByte((byte)(data >> 56));
        writeByte((byte)(data >> 48));
        writeByte((byte)(data >> 40));
        writeByte((byte)(data >> 32));
        writeByte((byte)(data >> 24));
        writeByte((byte)(data >> 16));
        writeByte((byte)(data >> 8));
        writeByte((byte)data);
    }

    public void writeUInt64(ulong data)
    {
        writeByte((byte)(data >> 56));
        writeByte((byte)(data >> 48));
        writeByte((byte)(data >> 40));
        writeByte((byte)(data >> 32));
        writeByte((byte)(data >> 24));
        writeByte((byte)(data >> 16));
        writeByte((byte)(data >> 8));
        writeByte((byte)data);
    }

    public void writeChar(char data)
    {
        writeUInt16(data);
    }

    public void writeString(string data)
    {
        ushort num = (ushort)data.Length;
        char[] array = data.ToCharArray();
        writeUInt16(num);
        for (ushort num2 = 0; num2 < num; num2++)
        {
            char data2 = array[num2];
            writeChar(data2);
        }
    }

    public void writeBytes(byte[] data, ulong offset, ulong length)
    {
        stream.Write(data, (int)offset, (int)length);
    }

    public NetworkStream(Stream newStream)
    {
        stream = newStream;
    }
}
