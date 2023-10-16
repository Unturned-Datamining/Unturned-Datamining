using System;
using System.Text;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class Block
{
    public static readonly int BUFFER_SIZE = 65535;

    public static byte[] buffer = new byte[BUFFER_SIZE];

    private static object[][] objects = new object[7][]
    {
        new object[1],
        new object[2],
        new object[3],
        new object[4],
        new object[5],
        new object[6],
        new object[7]
    };

    public bool longBinaryData;

    public int step;

    public byte[] block;

    private static object[] getObjects(int index)
    {
        object[] array = objects[index];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = null;
        }
        return array;
    }

    public string readString()
    {
        if (block != null && step < block.Length)
        {
            byte b = block[step];
            step++;
            string result = ((step + b > block.Length) ? string.Empty : Encoding.UTF8.GetString(block, step, b));
            step += b;
            return result;
        }
        return string.Empty;
    }

    public string[] readStringArray()
    {
        if (block != null && step < block.Length)
        {
            string[] array = new string[readByte()];
            for (byte b = 0; b < array.Length; b++)
            {
                array[b] = readString();
            }
            return array;
        }
        return new string[0];
    }

    public bool readBoolean()
    {
        if (block != null && step <= block.Length - 1)
        {
            bool result = BitConverter.ToBoolean(block, step);
            step++;
            return result;
        }
        return false;
    }

    public bool[] readBooleanArray()
    {
        if (block != null && step < block.Length)
        {
            bool[] array = new bool[readUInt16()];
            ushort num = (ushort)Mathf.CeilToInt((float)array.Length / 8f);
            for (ushort num2 = 0; num2 < num; num2++)
            {
                byte b = 0;
                while (b < 8 && num2 * 8 + b < array.Length)
                {
                    array[num2 * 8 + b] = (block[step + num2] & Types.SHIFTS[b]) == Types.SHIFTS[b];
                    b++;
                }
            }
            step += num;
            return array;
        }
        return new bool[0];
    }

    public byte readByte()
    {
        if (block != null && step <= block.Length - 1)
        {
            byte result = block[step];
            step++;
            return result;
        }
        return 0;
    }

    public byte[] readByteArray()
    {
        if (block != null && step < block.Length)
        {
            byte[] array;
            if (longBinaryData)
            {
                int num = readInt32();
                if (num >= 30000)
                {
                    return new byte[0];
                }
                array = new byte[num];
            }
            else
            {
                array = new byte[block[step]];
                step++;
            }
            if (step + array.Length <= block.Length)
            {
                try
                {
                    Buffer.BlockCopy(block, step, array, 0, array.Length);
                }
                catch
                {
                }
            }
            step += array.Length;
            return array;
        }
        return new byte[0];
    }

    public short readInt16()
    {
        if (block != null && step <= block.Length - 2)
        {
            readBitConverterBytes(2);
            short result = BitConverter.ToInt16(block, step);
            step += 2;
            return result;
        }
        return 0;
    }

    public ushort readUInt16()
    {
        if (block != null && step <= block.Length - 2)
        {
            readBitConverterBytes(2);
            ushort result = BitConverter.ToUInt16(block, step);
            step += 2;
            return result;
        }
        return 0;
    }

    public int readInt32()
    {
        if (block != null && step <= block.Length - 4)
        {
            readBitConverterBytes(4);
            int result = BitConverter.ToInt32(block, step);
            step += 4;
            return result;
        }
        return 0;
    }

    public int[] readInt32Array()
    {
        ushort num = readUInt16();
        int[] array = new int[num];
        for (ushort num2 = 0; num2 < num; num2++)
        {
            int num3 = readInt32();
            array[num2] = num3;
        }
        return array;
    }

    public uint readUInt32()
    {
        if (block != null && step <= block.Length - 4)
        {
            readBitConverterBytes(4);
            uint result = BitConverter.ToUInt32(block, step);
            step += 4;
            return result;
        }
        return 0u;
    }

    public float readSingle()
    {
        if (block != null && step <= block.Length - 4)
        {
            readBitConverterBytes(4);
            float result = BitConverter.ToSingle(block, step);
            step += 4;
            return result;
        }
        return 0f;
    }

    public long readInt64()
    {
        if (block != null && step <= block.Length - 8)
        {
            readBitConverterBytes(8);
            long result = BitConverter.ToInt64(block, step);
            step += 8;
            return result;
        }
        return 0L;
    }

    public ulong readUInt64()
    {
        if (block != null && step <= block.Length - 8)
        {
            readBitConverterBytes(8);
            ulong result = BitConverter.ToUInt64(block, step);
            step += 8;
            return result;
        }
        return 0uL;
    }

    public ulong[] readUInt64Array()
    {
        ushort num = readUInt16();
        ulong[] array = new ulong[num];
        for (ushort num2 = 0; num2 < num; num2++)
        {
            ulong num3 = readUInt64();
            array[num2] = num3;
        }
        return array;
    }

    public CSteamID readSteamID()
    {
        return new CSteamID(readUInt64());
    }

    public Guid readGUID()
    {
        GuidBuffer guidBuffer = default(GuidBuffer);
        guidBuffer.Read(readByteArray(), 0);
        return guidBuffer.GUID;
    }

    public Vector3 readUInt16RVector3()
    {
        byte num = readByte();
        double num2 = (double)(int)readUInt16() / 65535.0;
        double num3 = (double)(int)readUInt16() / 65535.0;
        byte b = readByte();
        double num4 = (double)(int)readUInt16() / 65535.0;
        num2 = (double)(num * Regions.REGION_SIZE) + num2 * (double)(int)Regions.REGION_SIZE - 4096.0;
        num3 = num3 * 2048.0 - 1024.0;
        num4 = (double)(b * Regions.REGION_SIZE) + num4 * (double)(int)Regions.REGION_SIZE - 4096.0;
        return new Vector3((float)num2, (float)num3, (float)num4);
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

    public object read(Type type)
    {
        if (type == Types.STRING_TYPE)
        {
            return readString();
        }
        if (type == Types.STRING_ARRAY_TYPE)
        {
            return readStringArray();
        }
        if (type == Types.BOOLEAN_TYPE)
        {
            return readBoolean();
        }
        if (type == Types.BOOLEAN_ARRAY_TYPE)
        {
            return readBooleanArray();
        }
        if (type == Types.BYTE_TYPE)
        {
            return readByte();
        }
        if (type == Types.BYTE_ARRAY_TYPE)
        {
            return readByteArray();
        }
        if (type == Types.INT16_TYPE)
        {
            return readInt16();
        }
        if (type == Types.UINT16_TYPE)
        {
            return readUInt16();
        }
        if (type == Types.INT32_TYPE)
        {
            return readInt32();
        }
        if (type == Types.INT32_ARRAY_TYPE)
        {
            return readInt32Array();
        }
        if (type == Types.UINT32_TYPE)
        {
            return readUInt32();
        }
        if (type == Types.SINGLE_TYPE)
        {
            return readSingle();
        }
        if (type == Types.INT64_TYPE)
        {
            return readInt64();
        }
        if (type == Types.UINT64_TYPE)
        {
            return readUInt64();
        }
        if (type == Types.UINT64_ARRAY_TYPE)
        {
            return readUInt64Array();
        }
        if (type == Types.STEAM_ID_TYPE)
        {
            return readSteamID();
        }
        if (type == Types.GUID_TYPE)
        {
            return readGUID();
        }
        if (type == Types.VECTOR3_TYPE)
        {
            return readSingleVector3();
        }
        if (type == Types.COLOR_TYPE)
        {
            return readColor();
        }
        throw new NotSupportedException($"Cannot read type {type}");
    }

    public object[] read(int offset, Type type_0)
    {
        object[] array = getObjects(0);
        if (offset < 1)
        {
            array[0] = read(type_0);
        }
        return array;
    }

    public object[] read(int offset, Type type_0, Type type_1)
    {
        object[] array = getObjects(1);
        if (offset < 1)
        {
            array[0] = read(type_0);
        }
        if (offset < 2)
        {
            array[1] = read(type_1);
        }
        return array;
    }

    public object[] read(Type type_0, Type type_1)
    {
        return read(0, type_0, type_1);
    }

    public object[] read(int offset, Type type_0, Type type_1, Type type_2)
    {
        object[] array = getObjects(2);
        if (offset < 1)
        {
            array[0] = read(type_0);
        }
        if (offset < 2)
        {
            array[1] = read(type_1);
        }
        if (offset < 3)
        {
            array[2] = read(type_2);
        }
        return array;
    }

    public object[] read(Type type_0, Type type_1, Type type_2)
    {
        return read(0, type_0, type_1, type_2);
    }

    public object[] read(int offset, Type type_0, Type type_1, Type type_2, Type type_3)
    {
        object[] array = getObjects(3);
        if (offset < 1)
        {
            array[0] = read(type_0);
        }
        if (offset < 2)
        {
            array[1] = read(type_1);
        }
        if (offset < 3)
        {
            array[2] = read(type_2);
        }
        if (offset < 4)
        {
            array[3] = read(type_3);
        }
        return array;
    }

    public object[] read(Type type_0, Type type_1, Type type_2, Type type_3)
    {
        return read(0, type_0, type_1, type_2, type_3);
    }

    public object[] read(int offset, Type type_0, Type type_1, Type type_2, Type type_3, Type type_4)
    {
        object[] array = getObjects(4);
        if (offset < 1)
        {
            array[0] = read(type_0);
        }
        if (offset < 2)
        {
            array[1] = read(type_1);
        }
        if (offset < 3)
        {
            array[2] = read(type_2);
        }
        if (offset < 4)
        {
            array[3] = read(type_3);
        }
        if (offset < 5)
        {
            array[4] = read(type_4);
        }
        return array;
    }

    public object[] read(Type type_0, Type type_1, Type type_2, Type type_3, Type type_4)
    {
        return read(0, type_0, type_1, type_2, type_3, type_4);
    }

    public object[] read(int offset, Type type_0, Type type_1, Type type_2, Type type_3, Type type_4, Type type_5)
    {
        object[] array = getObjects(5);
        if (offset < 1)
        {
            array[0] = read(type_0);
        }
        if (offset < 2)
        {
            array[1] = read(type_1);
        }
        if (offset < 3)
        {
            array[2] = read(type_2);
        }
        if (offset < 4)
        {
            array[3] = read(type_3);
        }
        if (offset < 5)
        {
            array[4] = read(type_4);
        }
        if (offset < 6)
        {
            array[5] = read(type_5);
        }
        return array;
    }

    public object[] read(Type type_0, Type type_1, Type type_2, Type type_3, Type type_4, Type type_5)
    {
        return read(0, type_0, type_1, type_2, type_3, type_4, type_5);
    }

    public object[] read(int offset, Type type_0, Type type_1, Type type_2, Type type_3, Type type_4, Type type_5, Type type_6)
    {
        object[] array = getObjects(6);
        if (offset < 1)
        {
            array[0] = read(type_0);
        }
        if (offset < 2)
        {
            array[1] = read(type_1);
        }
        if (offset < 3)
        {
            array[2] = read(type_2);
        }
        if (offset < 4)
        {
            array[3] = read(type_3);
        }
        if (offset < 5)
        {
            array[4] = read(type_4);
        }
        if (offset < 6)
        {
            array[5] = read(type_5);
        }
        if (offset < 7)
        {
            array[6] = read(type_6);
        }
        return array;
    }

    public object[] read(Type type_0, Type type_1, Type type_2, Type type_3, Type type_4, Type type_5, Type type_6)
    {
        return read(0, type_0, type_1, type_2, type_3, type_4, type_5, type_6);
    }

    public object[] read(int offset, params Type[] types)
    {
        object[] array = new object[types.Length];
        for (int i = offset; i < types.Length; i++)
        {
            array[i] = read(types[i]);
        }
        return array;
    }

    public object[] read(params Type[] types)
    {
        return read(0, types);
    }

    protected void readBitConverterBytes(int length)
    {
    }

    protected void writeBitConverterBytes(byte[] bytes)
    {
        int dstOffset = step;
        int count = bytes.Length;
        Buffer.BlockCopy(bytes, 0, buffer, dstOffset, count);
    }

    public void writeString(string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        byte b = (byte)bytes.Length;
        buffer[step] = b;
        step++;
        Buffer.BlockCopy(bytes, 0, buffer, step, b);
        step += b;
    }

    public void writeStringArray(string[] values)
    {
        byte b = (byte)values.Length;
        writeByte(b);
        for (byte b2 = 0; b2 < b; b2++)
        {
            writeString(values[b2]);
        }
    }

    public void writeBoolean(bool value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        buffer[step] = bytes[0];
        step++;
    }

    public void writeBooleanArray(bool[] values)
    {
        writeUInt16((ushort)values.Length);
        ushort num = (ushort)Mathf.CeilToInt((float)values.Length / 8f);
        for (ushort num2 = 0; num2 < num; num2++)
        {
            buffer[step + num2] = 0;
            byte b = 0;
            while (b < 8 && num2 * 8 + b < values.Length)
            {
                if (values[num2 * 8 + b])
                {
                    buffer[step + num2] |= Types.SHIFTS[b];
                }
                b++;
            }
        }
        step += num;
    }

    public void writeByte(byte value)
    {
        buffer[step] = value;
        step++;
    }

    public void writeByteArray(byte[] values)
    {
        if (values.Length < 30000)
        {
            if (longBinaryData)
            {
                writeInt32(values.Length);
                Buffer.BlockCopy(values, 0, buffer, step, values.Length);
                step += values.Length;
            }
            else
            {
                byte b = (byte)values.Length;
                buffer[step] = b;
                step++;
                Buffer.BlockCopy(values, 0, buffer, step, b);
                step += b;
            }
        }
    }

    public void writeInt16(short value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        writeBitConverterBytes(bytes);
        step += 2;
    }

    public void writeUInt16(ushort value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        writeBitConverterBytes(bytes);
        step += 2;
    }

    public void writeInt32(int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        writeBitConverterBytes(bytes);
        step += 4;
    }

    public void writeInt32Array(int[] values)
    {
        writeUInt16((ushort)values.Length);
        for (ushort num = 0; num < values.Length; num++)
        {
            writeInt32(values[num]);
        }
    }

    public void writeUInt32(uint value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        writeBitConverterBytes(bytes);
        step += 4;
    }

    public void writeSingle(float value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        writeBitConverterBytes(bytes);
        step += 4;
    }

    public void writeInt64(long value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        writeBitConverterBytes(bytes);
        step += 8;
    }

    public void writeUInt64(ulong value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        writeBitConverterBytes(bytes);
        step += 8;
    }

    public void writeUInt64Array(ulong[] values)
    {
        writeUInt16((ushort)values.Length);
        for (ushort num = 0; num < values.Length; num++)
        {
            writeUInt64(values[num]);
        }
    }

    public void writeSteamID(CSteamID steamID)
    {
        writeUInt64(steamID.m_SteamID);
    }

    public void writeGUID(Guid GUID)
    {
        new GuidBuffer(GUID).Write(GuidBuffer.GUID_BUFFER, 0);
        writeByteArray(GuidBuffer.GUID_BUFFER);
    }

    public void writeUInt16RVector3(Vector3 value)
    {
        double num = (double)value.x + 4096.0;
        double num2 = (double)value.y + 1024.0;
        double num3 = (double)value.z + 4096.0;
        byte value2 = (byte)(num / (double)(int)Regions.REGION_SIZE);
        byte value3 = (byte)(num3 / (double)(int)Regions.REGION_SIZE);
        num %= (double)(int)Regions.REGION_SIZE;
        num2 %= 2048.0;
        num3 %= (double)(int)Regions.REGION_SIZE;
        num /= (double)(int)Regions.REGION_SIZE;
        num2 /= 2048.0;
        num3 /= (double)(int)Regions.REGION_SIZE;
        writeByte(value2);
        writeUInt16((ushort)(num * 65535.0));
        writeUInt16((ushort)(num2 * 65535.0));
        writeByte(value3);
        writeUInt16((ushort)(num3 * 65535.0));
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

    public void write(object objects)
    {
        Type type = objects.GetType();
        if (type == Types.STRING_TYPE)
        {
            writeString((string)objects);
            return;
        }
        if (type == Types.STRING_ARRAY_TYPE)
        {
            writeStringArray((string[])objects);
            return;
        }
        if (type == Types.BOOLEAN_TYPE)
        {
            writeBoolean((bool)objects);
            return;
        }
        if (type == Types.BOOLEAN_ARRAY_TYPE)
        {
            writeBooleanArray((bool[])objects);
            return;
        }
        if (type == Types.BYTE_TYPE)
        {
            writeByte((byte)objects);
            return;
        }
        if (type == Types.BYTE_ARRAY_TYPE)
        {
            writeByteArray((byte[])objects);
            return;
        }
        if (type == Types.INT16_TYPE)
        {
            writeInt16((short)objects);
            return;
        }
        if (type == Types.UINT16_TYPE)
        {
            writeUInt16((ushort)objects);
            return;
        }
        if (type == Types.INT32_TYPE)
        {
            writeInt32((int)objects);
            return;
        }
        if (type == Types.INT32_ARRAY_TYPE)
        {
            writeInt32Array((int[])objects);
            return;
        }
        if (type == Types.UINT32_TYPE)
        {
            writeUInt32((uint)objects);
            return;
        }
        if (type == Types.SINGLE_TYPE)
        {
            writeSingle((float)objects);
            return;
        }
        if (type == Types.INT64_TYPE)
        {
            writeInt64((long)objects);
            return;
        }
        if (type == Types.UINT64_TYPE)
        {
            writeUInt64((ulong)objects);
            return;
        }
        if (type == Types.UINT64_ARRAY_TYPE)
        {
            writeUInt64Array((ulong[])objects);
            return;
        }
        if (type == Types.STEAM_ID_TYPE)
        {
            writeSteamID((CSteamID)objects);
            return;
        }
        if (type == Types.GUID_TYPE)
        {
            writeGUID((Guid)objects);
            return;
        }
        if (type == Types.VECTOR3_TYPE)
        {
            writeSingleVector3((Vector3)objects);
            return;
        }
        if (type == Types.COLOR_TYPE)
        {
            writeColor((Color)objects);
            return;
        }
        throw new NotSupportedException($"Cannot write {objects} of type {type}");
    }

    public void write(object object_0, object object_1)
    {
        write(object_0);
        write(object_1);
    }

    public void write(object object_0, object object_1, object object_2)
    {
        write(object_0, object_1);
        write(object_2);
    }

    public void write(object object_0, object object_1, object object_2, object object_3)
    {
        write(object_0, object_1, object_2);
        write(object_3);
    }

    public void write(object object_0, object object_1, object object_2, object object_3, object object_4)
    {
        write(object_0, object_1, object_2, object_3);
        write(object_4);
    }

    public void write(object object_0, object object_1, object object_2, object object_3, object object_4, object object_5)
    {
        write(object_0, object_1, object_2, object_3, object_4);
        write(object_5);
    }

    public void write(object object_0, object object_1, object object_2, object object_3, object object_4, object object_5, object object_6)
    {
        write(object_0, object_1, object_2, object_3, object_4, object_5);
        write(object_6);
    }

    public void write(params object[] objects)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            write(objects[i]);
        }
    }

    public byte[] getBytes(out int size)
    {
        if (block == null)
        {
            size = step;
            return buffer;
        }
        size = block.Length;
        return block;
    }

    public byte[] getHash()
    {
        if (block == null)
        {
            return Hash.SHA1(buffer);
        }
        return Hash.SHA1(block);
    }

    public void reset(int prefix, byte[] contents)
    {
        step = prefix;
        block = contents;
    }

    public void reset(byte[] contents)
    {
        step = 0;
        block = contents;
    }

    public void reset(int prefix)
    {
        step = prefix;
        block = null;
    }

    public void reset()
    {
        step = 0;
        block = null;
    }

    public Block(int prefix, byte[] contents)
    {
        reset(prefix, contents);
    }

    public Block(byte[] contents)
    {
        reset(contents);
    }

    public Block(int prefix)
    {
        reset(prefix);
    }

    public Block()
    {
        reset();
    }
}
