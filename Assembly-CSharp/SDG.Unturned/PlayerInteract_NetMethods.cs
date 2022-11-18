using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(PlayerInteract))]
public static class PlayerInteract_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveSalvageTimeOverride", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSalvageTimeOverride_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerInteract playerInteract = obj as PlayerInteract;
            if (!(playerInteract == null))
            {
                reader.ReadFloat(out var value2);
                playerInteract.ReceiveSalvageTimeOverride(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSalvageTimeOverride", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSalvageTimeOverride_Write(NetPakWriter writer, float overrideValue)
    {
        writer.WriteFloat(overrideValue);
    }

    [NetInvokableGeneratedMethod("ReceiveInspectRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveInspectRequest_Read(in ServerInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerInteract playerInteract = obj as PlayerInteract;
        if (!(playerInteract == null))
        {
            if (!context.IsOwnerOf(playerInteract.channel))
            {
                context.Kick($"not owner of {playerInteract}");
            }
            else
            {
                playerInteract.ReceiveInspectRequest();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveInspectRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveInspectRequest_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceivePlayInspect", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayInspect_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerInteract playerInteract = obj as PlayerInteract;
            if (!(playerInteract == null))
            {
                playerInteract.ReceivePlayInspect();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayInspect", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayInspect_Write(NetPakWriter writer)
    {
    }
}
