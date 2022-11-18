using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableFisher))]
public static class UseableFisher_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveCatch", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveCatch_Read(in ServerInvocationContext context)
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
        UseableFisher useableFisher = obj as UseableFisher;
        if (!(useableFisher == null))
        {
            if (!context.IsOwnerOf(useableFisher.channel))
            {
                context.Kick($"not owner of {useableFisher}");
            }
            else
            {
                useableFisher.ReceiveCatch();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveCatch", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveCatch_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceiveLuckTime", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveLuckTime_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableFisher useableFisher = obj as UseableFisher;
            if (!(useableFisher == null))
            {
                reader.ReadFloat(out var value2);
                useableFisher.ReceiveLuckTime(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveLuckTime", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveLuckTime_Write(NetPakWriter writer, float NewLuckTime)
    {
        writer.WriteFloat(NewLuckTime);
    }

    [NetInvokableGeneratedMethod("ReceivePlayReel", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayReel_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableFisher useableFisher = obj as UseableFisher;
            if (!(useableFisher == null))
            {
                useableFisher.ReceivePlayReel();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayReel", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayReel_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceivePlayCast", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayCast_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableFisher useableFisher = obj as UseableFisher;
            if (!(useableFisher == null))
            {
                useableFisher.ReceivePlayCast();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayCast", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayCast_Write(NetPakWriter writer)
    {
    }
}
