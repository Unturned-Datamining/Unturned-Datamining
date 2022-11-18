using System;
using Steamworks;

namespace SDG.Unturned;

public class SteamPacker
{
    [Obsolete]
    public static Block block = new Block();

    private static NetPakBlockImplementation luggageBlock = new NetPakBlockImplementation();

    [Obsolete]
    public static bool longBinaryData
    {
        get
        {
            return luggageBlock.longBinaryData;
        }
        set
        {
            luggageBlock.longBinaryData = value;
        }
    }

    [Obsolete]
    public static object read(Type type)
    {
        return luggageBlock.read(type);
    }

    [Obsolete]
    public static object[] read(Type type_0, Type type_1, Type type_2)
    {
        return luggageBlock.read(type_0, type_1, type_2);
    }

    [Obsolete]
    public static object[] read(Type type_0, Type type_1, Type type_2, Type type_3)
    {
        return luggageBlock.read(type_0, type_1, type_2, type_3);
    }

    [Obsolete]
    public static object[] read(Type type_0, Type type_1, Type type_2, Type type_3, Type type_4, Type type_5)
    {
        return luggageBlock.read(type_0, type_1, type_2, type_3, type_4, type_5);
    }

    [Obsolete]
    public static object[] read(Type type_0, Type type_1, Type type_2, Type type_3, Type type_4, Type type_5, Type type_6)
    {
        return luggageBlock.read(type_0, type_1, type_2, type_3, type_4, type_5, type_6);
    }

    [Obsolete]
    public static object[] read(params Type[] types)
    {
        return luggageBlock.read(types);
    }

    [Obsolete]
    public static void openRead(int prefix, byte[] bytes)
    {
        openRead(prefix, bytes.Length, bytes);
    }

    [Obsolete]
    public static void openRead(int prefix, int size, byte[] bytes)
    {
        luggageBlock.resetForRead(prefix, bytes, size);
    }

    [Obsolete]
    public static void closeRead()
    {
    }

    [Obsolete]
    public static void write(object objects)
    {
        luggageBlock.write(objects);
    }

    [Obsolete]
    public static void write(object object_0, object object_1)
    {
        luggageBlock.write(object_0, object_1);
    }

    [Obsolete]
    public static void write(object object_0, object object_1, object object_2)
    {
        luggageBlock.write(object_0, object_1, object_2);
    }

    [Obsolete]
    public static void write(object object_0, object object_1, object object_2, object object_3)
    {
        luggageBlock.write(object_0, object_1, object_2, object_3);
    }

    [Obsolete]
    public static void write(object object_0, object object_1, object object_2, object object_3, object object_4, object object_5)
    {
        luggageBlock.write(object_0, object_1, object_2, object_3, object_4, object_5);
    }

    [Obsolete]
    public static void write(object object_0, object object_1, object object_2, object object_3, object object_4, object object_5, object object_6)
    {
        luggageBlock.write(object_0, object_1, object_2, object_3, object_4, object_5, object_6);
    }

    [Obsolete]
    public static void write(params object[] objects)
    {
        luggageBlock.write(objects);
    }

    [Obsolete]
    public static void openWrite(int prefix)
    {
        luggageBlock.resetForWrite(prefix);
    }

    [Obsolete]
    public static byte[] closeWrite(out int size)
    {
        return luggageBlock.getBytes(out size);
    }

    public static byte[] getBytes(int prefix, out int size, params object[] objects)
    {
        luggageBlock.resetForWrite(prefix);
        luggageBlock.write(objects);
        return luggageBlock.getBytes(out size);
    }

    [Obsolete]
    public static object[] getObjects(CSteamID steamID, int offset, int prefix, byte[] bytes, params Type[] types)
    {
        return getObjects(steamID, offset, prefix, bytes.Length, bytes, types);
    }

    [Obsolete]
    public static object[] getObjects(CSteamID steamID, int offset, int prefix, int size, byte[] bytes, params Type[] types)
    {
        luggageBlock.resetForRead(offset + prefix, bytes, size);
        if (types[0].GetElementType() == typeof(ClientInvocationContext))
        {
            object[] array = luggageBlock.read(1, types);
            array[0] = default(ClientInvocationContext);
            return array;
        }
        if (types[0].GetElementType() == typeof(ServerInvocationContext))
        {
            object[] array2 = luggageBlock.read(1, types);
            ServerInvocationContext serverInvocationContext = ServerInvocationContext.FromSteamIDForBackwardsCompatibility(steamID);
            array2[0] = serverInvocationContext;
            return array2;
        }
        return luggageBlock.read(types);
    }

    internal static object[] getObjectsForLegacyRPC(int offset, int prefix, int size, byte[] bytes, Type[] types, int typesOffset)
    {
        luggageBlock.resetForRead(offset + prefix, bytes, size);
        return luggageBlock.readForLegacyRPC(typesOffset, types);
    }
}
