using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(PlayerLook))]
public static class PlayerLook_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveFreecamAllowed", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveFreecamAllowed_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLook playerLook = obj as PlayerLook;
            if (!(playerLook == null))
            {
                reader.ReadBit(out var value2);
                playerLook.ReceiveFreecamAllowed(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveFreecamAllowed", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveFreecamAllowed_Write(NetPakWriter writer, bool isAllowed)
    {
        writer.WriteBit(isAllowed);
    }

    [NetInvokableGeneratedMethod("ReceiveWorkzoneAllowed", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveWorkzoneAllowed_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLook playerLook = obj as PlayerLook;
            if (!(playerLook == null))
            {
                reader.ReadBit(out var value2);
                playerLook.ReceiveWorkzoneAllowed(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveWorkzoneAllowed", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveWorkzoneAllowed_Write(NetPakWriter writer, bool isAllowed)
    {
        writer.WriteBit(isAllowed);
    }

    [NetInvokableGeneratedMethod("ReceiveSpecStatsAllowed", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSpecStatsAllowed_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLook playerLook = obj as PlayerLook;
            if (!(playerLook == null))
            {
                reader.ReadBit(out var value2);
                playerLook.ReceiveSpecStatsAllowed(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSpecStatsAllowed", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSpecStatsAllowed_Write(NetPakWriter writer, bool isAllowed)
    {
        writer.WriteBit(isAllowed);
    }
}
