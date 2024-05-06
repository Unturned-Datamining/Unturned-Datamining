using System;
using System.IO;
using System.Text;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class River
{
    private static byte[] buffer = new byte[Block.BUFFER_SIZE];

    private int water;

    private string path;

    private Stream stream;

    private Block block;

    public string readString()
    {
        if (block != null)
        {
            return block.readString();
        }
        int num = readByte();
        if (num > 0)
        {
            stream.Read(buffer, 0, num);
            return Encoding.UTF8.GetString(buffer, 0, num);
        }
        return string.Empty;
    }

    public bool readBoolean()
    {
        if (block != null)
        {
            return block.readBoolean();
        }
        return readByte() > 0;
    }

    public byte readByte()
    {
        if (block != null)
        {
            return block.readByte();
        }
        return MathfEx.ClampToByte(stream.ReadByte());
    }

    public byte[] readBytes()
    {
        if (block != null)
        {
            return block.readByteArray();
        }
        byte[] array = new byte[readUInt16()];
        stream.Read(array, 0, array.Length);
        return array;
    }

    public short readInt16()
    {
        if (block != null)
        {
            return block.readInt16();
        }
        stream.Read(buffer, 0, 2);
        return BitConverter.ToInt16(buffer, 0);
    }

    public ushort readUInt16()
    {
        if (block != null)
        {
            return block.readUInt16();
        }
        stream.Read(buffer, 0, 2);
        return BitConverter.ToUInt16(buffer, 0);
    }

    public int readInt32()
    {
        if (block != null)
        {
            return block.readInt32();
        }
        stream.Read(buffer, 0, 4);
        return BitConverter.ToInt32(buffer, 0);
    }

    public uint readUInt32()
    {
        if (block != null)
        {
            return block.readUInt32();
        }
        stream.Read(buffer, 0, 4);
        return BitConverter.ToUInt32(buffer, 0);
    }

    public float readSingle()
    {
        if (block != null)
        {
            return block.readSingle();
        }
        stream.Read(buffer, 0, 4);
        return BitConverter.ToSingle(buffer, 0);
    }

    public long readInt64()
    {
        if (block != null)
        {
            return block.readInt64();
        }
        stream.Read(buffer, 0, 8);
        return BitConverter.ToInt64(buffer, 0);
    }

    public ulong readUInt64()
    {
        if (block != null)
        {
            return block.readUInt64();
        }
        stream.Read(buffer, 0, 8);
        return BitConverter.ToUInt64(buffer, 0);
    }

    public CSteamID readSteamID()
    {
        return new CSteamID(readUInt64());
    }

    public Guid readGUID()
    {
        if (block != null)
        {
            return block.readGUID();
        }
        GuidBuffer guidBuffer = default(GuidBuffer);
        guidBuffer.Read(readBytes(), 0);
        return guidBuffer.GUID;
    }

    public Vector3 readSingleVector3()
    {
        return new Vector3(readSingle(), readSingle(), readSingle());
    }

    public Quaternion readSingleQuaternion()
    {
        return Quaternion.Euler(readSingle(), readSingle(), readSingle());
    }

    public Color readColor()
    {
        return new Color((float)(int)readByte() / 255f, (float)(int)readByte() / 255f, (float)(int)readByte() / 255f);
    }

    public void writeString(string value)
    {
        if (block != null)
        {
            block.writeString(value);
            return;
        }
        if (value == null)
        {
            value = string.Empty;
        }
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        byte b = MathfEx.ClampToByte(bytes.Length);
        stream.WriteByte(b);
        stream.Write(bytes, 0, b);
        water += 1 + b;
    }

    public void writeBoolean(bool value)
    {
        if (block != null)
        {
            block.writeBoolean(value);
            return;
        }
        stream.WriteByte((byte)(value ? 1 : 0));
        water++;
    }

    public void writeByte(byte value)
    {
        if (block != null)
        {
            block.writeByte(value);
            return;
        }
        stream.WriteByte(value);
        water++;
    }

    public void writeBytes(byte[] values)
    {
        if (block != null)
        {
            block.writeByteArray(values);
            return;
        }
        ushort num = MathfEx.ClampToUShort(values.Length);
        writeUInt16(num);
        stream.Write(values, 0, num);
        water += num;
    }

    public void writeInt16(short value)
    {
        if (block != null)
        {
            block.writeInt16(value);
            return;
        }
        byte[] bytes = BitConverter.GetBytes(value);
        stream.Write(bytes, 0, 2);
        water += 2;
    }

    public void writeUInt16(ushort value)
    {
        if (block != null)
        {
            block.writeUInt16(value);
            return;
        }
        byte[] bytes = BitConverter.GetBytes(value);
        stream.Write(bytes, 0, 2);
        water += 2;
    }

    public void writeInt32(int value)
    {
        if (block != null)
        {
            block.writeInt32(value);
            return;
        }
        byte[] bytes = BitConverter.GetBytes(value);
        stream.Write(bytes, 0, 4);
        water += 4;
    }

    public void writeUInt32(uint value)
    {
        if (block != null)
        {
            block.writeUInt32(value);
            return;
        }
        byte[] bytes = BitConverter.GetBytes(value);
        stream.Write(bytes, 0, 4);
        water += 4;
    }

    public void writeSingle(float value)
    {
        if (block != null)
        {
            block.writeSingle(value);
            return;
        }
        byte[] bytes = BitConverter.GetBytes(value);
        stream.Write(bytes, 0, 4);
        water += 4;
    }

    public void writeInt64(long value)
    {
        if (block != null)
        {
            block.writeInt64(value);
            return;
        }
        byte[] bytes = BitConverter.GetBytes(value);
        stream.Write(bytes, 0, 8);
        water += 8;
    }

    public void writeUInt64(ulong value)
    {
        if (block != null)
        {
            block.writeUInt64(value);
            return;
        }
        byte[] bytes = BitConverter.GetBytes(value);
        stream.Write(bytes, 0, 8);
        water += 8;
    }

    public void writeSteamID(CSteamID steamID)
    {
        writeUInt64(steamID.m_SteamID);
    }

    public void writeGUID(Guid GUID)
    {
        new GuidBuffer(GUID).Write(GuidBuffer.GUID_BUFFER, 0);
        writeBytes(GuidBuffer.GUID_BUFFER);
    }

    public void writeSingleVector3(Vector3 value)
    {
        writeSingle(value.x);
        writeSingle(value.y);
        writeSingle(value.z);
    }

    public void writeSingleQuaternion(Quaternion value)
    {
        Vector3 eulerAngles = value.eulerAngles;
        writeSingle(eulerAngles.x);
        writeSingle(eulerAngles.y);
        writeSingle(eulerAngles.z);
    }

    public void writeColor(Color value)
    {
        writeByte((byte)(value.r * 255f));
        writeByte((byte)(value.g * 255f));
        writeByte((byte)(value.b * 255f));
    }

    public byte[] getHash()
    {
        stream.Position = 0L;
        return Hash.SHA1(stream);
    }

    public void closeRiver()
    {
        if (block != null)
        {
            ReadWrite.writeBlock(path, useCloud: true, block);
            return;
        }
        if (water > 0)
        {
            stream.SetLength(water);
        }
        stream.Flush();
        stream.Close();
        stream.Dispose();
    }

    public River(string newPath)
    {
        path = ReadWrite.PATH + newPath;
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        water = 0;
    }

    public River(string newPath, bool usePath)
    {
        path = newPath;
        if (usePath)
        {
            path = ReadWrite.PATH + path;
        }
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        water = 0;
    }

    public River(string newPath, bool usePath, bool useCloud, bool isReading)
    {
        path = newPath;
        if (useCloud)
        {
            if (isReading)
            {
                block = ReadWrite.readBlock(path, useCloud, 0);
            }
            if (block == null)
            {
                block = new Block();
            }
            return;
        }
        if (usePath)
        {
            path = ReadWrite.PATH + path;
        }
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        water = 0;
    }
}
