using System;
using SDG.NetPak;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

internal class NetPakBlockImplementation
{
    [Obsolete]
    public bool longBinaryData;

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

    private NetPakReader reader;

    private NetPakWriter writer;

    public object read(Type type)
    {
        if (type == Types.STRING_TYPE)
        {
            reader.ReadString(out var value);
            return value;
        }
        if (type == Types.STRING_ARRAY_TYPE)
        {
            reader.ReadUInt8(out var value2);
            string[] array = new string[value2];
            for (int i = 0; i < array.Length; i++)
            {
                reader.ReadString(out array[i]);
            }
            return array;
        }
        if (type == Types.BOOLEAN_TYPE)
        {
            reader.ReadBit(out var value3);
            return value3;
        }
        if (type == Types.BOOLEAN_ARRAY_TYPE)
        {
            reader.ReadUInt16(out var value4);
            bool[] array2 = new bool[value4];
            for (int j = 0; j < array2.Length; j++)
            {
                reader.ReadBit(out array2[j]);
            }
            return array2;
        }
        if (type == Types.BYTE_TYPE)
        {
            reader.ReadUInt8(out var value5);
            return value5;
        }
        if (type == Types.BYTE_ARRAY_TYPE)
        {
            reader.ReadUInt8(out var value6);
            byte[] array3 = new byte[value6];
            reader.ReadBytes(array3);
            return array3;
        }
        if (type == Types.INT16_TYPE)
        {
            reader.ReadInt16(out var value7);
            return value7;
        }
        if (type == Types.UINT16_TYPE)
        {
            reader.ReadUInt16(out var value8);
            return value8;
        }
        if (type == Types.INT32_TYPE)
        {
            reader.ReadInt32(out var value9);
            return value9;
        }
        if (type == Types.INT32_ARRAY_TYPE)
        {
            reader.ReadUInt16(out var value10);
            int[] array4 = new int[value10];
            for (int k = 0; k < array4.Length; k++)
            {
                reader.ReadInt32(out array4[k]);
            }
            return array4;
        }
        if (type == Types.UINT32_TYPE)
        {
            reader.ReadUInt32(out var value11);
            return value11;
        }
        if (type == Types.SINGLE_TYPE)
        {
            reader.ReadFloat(out var value12);
            return value12;
        }
        if (type == Types.INT64_TYPE)
        {
            reader.ReadInt64(out var value13);
            return value13;
        }
        if (type == Types.UINT64_TYPE)
        {
            reader.ReadUInt64(out var value14);
            return value14;
        }
        if (type == Types.UINT64_ARRAY_TYPE)
        {
            reader.ReadUInt16(out var value15);
            ulong[] array5 = new ulong[value15];
            for (int l = 0; l < array5.Length; l++)
            {
                reader.ReadUInt64(out array5[l]);
            }
            return array5;
        }
        if (type == Types.STEAM_ID_TYPE)
        {
            reader.ReadSteamID(out CSteamID value16);
            return value16;
        }
        if (type == Types.GUID_TYPE)
        {
            reader.ReadGuid(out var value17);
            return value17;
        }
        if (type == Types.VECTOR3_TYPE)
        {
            reader.ReadClampedVector3(out var value18, 13, 9);
            return value18;
        }
        if (type == Types.QUATERNION_TYPE)
        {
            reader.ReadQuaternion(out var value19);
            return value19;
        }
        if (type == Types.COLOR_TYPE)
        {
            reader.ReadColor32RGB(out Color32 value20);
            return (Color)value20;
        }
        if (type == typeof(NetId))
        {
            reader.ReadNetId(out var value21);
            return value21;
        }
        if (type.IsEnum)
        {
            reader.ReadUInt8(out var value22);
            return Enum.ToObject(type, value22);
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

    public object[] readForLegacyRPC(int offset, Type[] types)
    {
        object[] array = new object[types.Length];
        for (int i = offset; i < types.Length; i++)
        {
            array[i] = read(types[i]);
        }
        return array;
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

    public void write(object objects)
    {
        Type type = objects.GetType();
        if (type == Types.STRING_TYPE)
        {
            writer.WriteString((string)objects);
        }
        else if (type == Types.STRING_ARRAY_TYPE)
        {
            string[] array = (string[])objects;
            byte b = (byte)array.Length;
            writer.WriteUInt8(b);
            for (int i = 0; i < b; i++)
            {
                writer.WriteString(array[i]);
            }
        }
        else if (type == Types.BOOLEAN_TYPE)
        {
            writer.WriteBit((bool)objects);
        }
        else if (type == Types.BOOLEAN_ARRAY_TYPE)
        {
            bool[] array2 = (bool[])objects;
            ushort num = (ushort)array2.Length;
            writer.WriteUInt16(num);
            for (int j = 0; j < num; j++)
            {
                writer.WriteBit(array2[j]);
            }
        }
        else if (type == Types.BYTE_TYPE)
        {
            writer.WriteUInt8((byte)objects);
        }
        else if (type == Types.BYTE_ARRAY_TYPE)
        {
            byte[] array3 = (byte[])objects;
            byte b2 = (byte)array3.Length;
            writer.WriteUInt8(b2);
            writer.WriteBytes(array3, b2);
        }
        else if (type == Types.INT16_TYPE)
        {
            writer.WriteInt16((short)objects);
        }
        else if (type == Types.UINT16_TYPE)
        {
            writer.WriteUInt16((ushort)objects);
        }
        else if (type == Types.INT32_TYPE)
        {
            writer.WriteInt32((int)objects);
        }
        else if (type == Types.INT32_ARRAY_TYPE)
        {
            int[] array4 = (int[])objects;
            ushort num2 = (ushort)array4.Length;
            writer.WriteUInt16(num2);
            for (int k = 0; k < num2; k++)
            {
                writer.WriteInt32(array4[k]);
            }
        }
        else if (type == Types.UINT32_TYPE)
        {
            writer.WriteUInt32((uint)objects);
        }
        else if (type == Types.SINGLE_TYPE)
        {
            writer.WriteFloat((float)objects);
        }
        else if (type == Types.INT64_TYPE)
        {
            writer.WriteInt64((long)objects);
        }
        else if (type == Types.UINT64_TYPE)
        {
            writer.WriteUInt64((ulong)objects);
        }
        else if (type == Types.UINT64_ARRAY_TYPE)
        {
            ulong[] array5 = (ulong[])objects;
            ushort num3 = (ushort)array5.Length;
            writer.WriteUInt16(num3);
            for (int l = 0; l < num3; l++)
            {
                writer.WriteUInt64(array5[l]);
            }
        }
        else if (type == Types.STEAM_ID_TYPE)
        {
            writer.WriteSteamID((CSteamID)objects);
        }
        else if (type == Types.GUID_TYPE)
        {
            writer.WriteGuid((Guid)objects);
        }
        else if (type == Types.VECTOR3_TYPE)
        {
            writer.WriteClampedVector3((Vector3)objects, 13, 9);
        }
        else if (type == Types.QUATERNION_TYPE)
        {
            Quaternion value = (Quaternion)objects;
            writer.WriteQuaternion(value);
        }
        else if (type == Types.COLOR_TYPE)
        {
            Color color = (Color)objects;
            writer.WriteColor32RGB(color);
        }
        else
        {
            if (!(type == typeof(NetId)))
            {
                throw new NotSupportedException($"Cannot write {objects} of type {type}");
            }
            NetId value2 = (NetId)objects;
            writer.WriteNetId(value2);
        }
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

    public void resetForRead(int prefix, byte[] buffer, int size)
    {
        reader.SetBuffer(buffer);
        reader.Reset();
        reader.readByteIndex = prefix;
    }

    public void resetForWrite(int prefix)
    {
        writer.Reset();
        writer.writeByteIndex = prefix;
    }

    public byte[] getBytes(out int size)
    {
        writer.Flush();
        size = writer.writeByteIndex;
        return writer.buffer;
    }

    public NetPakBlockImplementation()
    {
        reader = new NetPakReader();
        reader.SetBuffer(Provider.buffer);
        writer = new NetPakWriter();
        writer.buffer = Block.buffer;
    }

    private static object[] getObjects(int index)
    {
        object[] array = objects[index];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = null;
        }
        return array;
    }
}
