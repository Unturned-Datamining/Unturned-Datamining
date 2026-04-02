using System;
using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableFisher))]
public static class UseableFisher_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveBobberInWaterConfirmation", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveBobberInWaterConfirmation_Read(in ServerInvocationContext context)
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
        UseableFisher useableFisher = obj as UseableFisher;
        if (!(useableFisher == null))
        {
            if (!context.IsOwnerOf(useableFisher.channel))
            {
                context.Kick($"not owner of {useableFisher}");
                return;
            }
            reader.ReadNetId(out var value2);
            useableFisher.ReceiveBobberInWaterConfirmation(in context, value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveBobberInWaterConfirmation", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveBobberInWaterConfirmation_Write(NetPakWriter writer, NetId waterVolumeNetId)
    {
        writer.WriteNetId(waterVolumeNetId);
    }

    [NetInvokableGeneratedMethod("ReceiveCatchConfirmation", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveCatchConfirmation_Read(in ServerInvocationContext context)
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
                useableFisher.ReceiveCatchConfirmation(in context);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveFishNotification", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveFishNotification_Read(in ClientInvocationContext context)
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
                reader.ReadGuid(out var value2);
                reader.ReadInt32(out var value3);
                useableFisher.ReceiveFishNotification(value2, value3);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveFishNotification", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveFishNotification_Write(NetPakWriter writer, Guid nextRewardGuid, int newSeed)
    {
        writer.WriteGuid(nextRewardGuid);
        writer.WriteInt32(newSeed);
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
