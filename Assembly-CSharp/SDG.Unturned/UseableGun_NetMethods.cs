using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableGun))]
public static class UseableGun_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveChangeFiremode", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveChangeFiremode_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        UseableGun useableGun = obj as UseableGun;
        if (!(useableGun == null))
        {
            if (!context.IsOwnerOf(useableGun.channel))
            {
                context.Kick($"not owner of {useableGun}");
                return;
            }
            reader.ReadEnum(out var value2);
            useableGun.ReceiveChangeFiremode(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveChangeFiremode", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveChangeFiremode_Write(NetPakWriter writer, EFiremode newFiremode)
    {
        writer.WriteEnum(newFiremode);
    }

    [NetInvokableGeneratedMethod("ReceivePlayProject", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayProject_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableGun useableGun = obj as UseableGun;
            if (!(useableGun == null))
            {
                reader.ReadClampedVector3(out var value2);
                reader.ReadClampedVector3(out var value3);
                reader.ReadUInt16(out var value4);
                reader.ReadUInt16(out var value5);
                useableGun.ReceivePlayProject(value2, value3, value4, value5);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayProject", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayProject_Write(NetPakWriter writer, Vector3 origin, Vector3 direction, ushort barrelId, ushort magazineId)
    {
        writer.WriteClampedVector3(origin);
        writer.WriteClampedVector3(direction);
        writer.WriteUInt16(barrelId);
        writer.WriteUInt16(magazineId);
    }

    [NetInvokableGeneratedMethod("ReceivePlayShoot", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayShoot_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableGun useableGun = obj as UseableGun;
            if (!(useableGun == null))
            {
                useableGun.ReceivePlayShoot();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayShoot", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayShoot_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceiveAttachSight", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAttachSight_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        UseableGun useableGun = obj as UseableGun;
        if (!(useableGun == null))
        {
            if (!context.IsOwnerOf(useableGun.channel))
            {
                context.Kick($"not owner of {useableGun}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            reader.ReadUInt8(out var value5);
            byte[] array = new byte[value5];
            reader.ReadBytes(array);
            useableGun.ReceiveAttachSight(value2, value3, value4, array);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAttachSight", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAttachSight_Write(NetPakWriter writer, byte page, byte x, byte y, byte[] hash)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        byte b = (byte)hash.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(hash, b);
    }

    [NetInvokableGeneratedMethod("ReceiveAttachTactical", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAttachTactical_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        UseableGun useableGun = obj as UseableGun;
        if (!(useableGun == null))
        {
            if (!context.IsOwnerOf(useableGun.channel))
            {
                context.Kick($"not owner of {useableGun}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            reader.ReadUInt8(out var value5);
            byte[] array = new byte[value5];
            reader.ReadBytes(array);
            useableGun.ReceiveAttachTactical(value2, value3, value4, array);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAttachTactical", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAttachTactical_Write(NetPakWriter writer, byte page, byte x, byte y, byte[] hash)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        byte b = (byte)hash.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(hash, b);
    }

    [NetInvokableGeneratedMethod("ReceiveAttachGrip", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAttachGrip_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        UseableGun useableGun = obj as UseableGun;
        if (!(useableGun == null))
        {
            if (!context.IsOwnerOf(useableGun.channel))
            {
                context.Kick($"not owner of {useableGun}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            reader.ReadUInt8(out var value5);
            byte[] array = new byte[value5];
            reader.ReadBytes(array);
            useableGun.ReceiveAttachGrip(value2, value3, value4, array);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAttachGrip", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAttachGrip_Write(NetPakWriter writer, byte page, byte x, byte y, byte[] hash)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        byte b = (byte)hash.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(hash, b);
    }

    [NetInvokableGeneratedMethod("ReceiveAttachBarrel", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAttachBarrel_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        UseableGun useableGun = obj as UseableGun;
        if (!(useableGun == null))
        {
            if (!context.IsOwnerOf(useableGun.channel))
            {
                context.Kick($"not owner of {useableGun}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            reader.ReadUInt8(out var value5);
            byte[] array = new byte[value5];
            reader.ReadBytes(array);
            useableGun.ReceiveAttachBarrel(value2, value3, value4, array);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAttachBarrel", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAttachBarrel_Write(NetPakWriter writer, byte page, byte x, byte y, byte[] hash)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        byte b = (byte)hash.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(hash, b);
    }

    [NetInvokableGeneratedMethod("ReceiveAttachMagazine", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAttachMagazine_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        UseableGun useableGun = obj as UseableGun;
        if (!(useableGun == null))
        {
            if (!context.IsOwnerOf(useableGun.channel))
            {
                context.Kick($"not owner of {useableGun}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            reader.ReadUInt8(out var value5);
            byte[] array = new byte[value5];
            reader.ReadBytes(array);
            useableGun.ReceiveAttachMagazine(value2, value3, value4, array);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAttachMagazine", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAttachMagazine_Write(NetPakWriter writer, byte page, byte x, byte y, byte[] hash)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        byte b = (byte)hash.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(hash, b);
    }

    [NetInvokableGeneratedMethod("ReceivePlayReload", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayReload_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableGun useableGun = obj as UseableGun;
            if (!(useableGun == null))
            {
                reader.ReadBit(out var value2);
                useableGun.ReceivePlayReload(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayReload", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayReload_Write(NetPakWriter writer, bool newHammer)
    {
        writer.WriteBit(newHammer);
    }

    [NetInvokableGeneratedMethod("ReceivePlayChamberJammed", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayChamberJammed_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableGun useableGun = obj as UseableGun;
            if (!(useableGun == null))
            {
                reader.ReadUInt8(out var value2);
                useableGun.ReceivePlayChamberJammed(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayChamberJammed", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayChamberJammed_Write(NetPakWriter writer, byte correctedAmmo)
    {
        writer.WriteUInt8(correctedAmmo);
    }

    [NetInvokableGeneratedMethod("ReceivePlayAimStart", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayAimStart_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableGun useableGun = obj as UseableGun;
            if (!(useableGun == null))
            {
                useableGun.ReceivePlayAimStart();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayAimStart", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayAimStart_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceivePlayAimStop", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayAimStop_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableGun useableGun = obj as UseableGun;
            if (!(useableGun == null))
            {
                useableGun.ReceivePlayAimStop();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayAimStop", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayAimStop_Write(NetPakWriter writer)
    {
    }
}
