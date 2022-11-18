using System;
using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableStereo))]
public static class InteractableStereo_NetMethods
{
    private static void ReceiveTrack_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableStereo interactableStereo = voidNetObj as InteractableStereo;
        if (!(interactableStereo == null))
        {
            context.reader.ReadGuid(out var value);
            interactableStereo.ReceiveTrack(value);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveTrack", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveTrack_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            NetInvocationDeferralRegistry.Defer(value, in context, ReceiveTrack_DeferredRead);
            return;
        }
        InteractableStereo interactableStereo = obj as InteractableStereo;
        if (!(interactableStereo == null))
        {
            reader.ReadGuid(out var value2);
            interactableStereo.ReceiveTrack(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveTrack", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveTrack_Write(NetPakWriter writer, Guid newTrack)
    {
        writer.WriteGuid(newTrack);
    }

    [NetInvokableGeneratedMethod("ReceiveTrackRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveTrackRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            InteractableStereo interactableStereo = obj as InteractableStereo;
            if (!(interactableStereo == null))
            {
                reader.ReadGuid(out var value2);
                interactableStereo.ReceiveTrackRequest(in context, value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveTrackRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveTrackRequest_Write(NetPakWriter writer, Guid newTrack)
    {
        writer.WriteGuid(newTrack);
    }

    private static void ReceiveChangeVolume_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableStereo interactableStereo = voidNetObj as InteractableStereo;
        if (!(interactableStereo == null))
        {
            context.reader.ReadUInt8(out var value);
            interactableStereo.ReceiveChangeVolume(value);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveChangeVolume", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveChangeVolume_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            NetInvocationDeferralRegistry.Defer(value, in context, ReceiveChangeVolume_DeferredRead);
            return;
        }
        InteractableStereo interactableStereo = obj as InteractableStereo;
        if (!(interactableStereo == null))
        {
            reader.ReadUInt8(out var value2);
            interactableStereo.ReceiveChangeVolume(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveChangeVolume", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveChangeVolume_Write(NetPakWriter writer, byte newVolume)
    {
        writer.WriteUInt8(newVolume);
    }

    [NetInvokableGeneratedMethod("ReceiveChangeVolumeRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveChangeVolumeRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            InteractableStereo interactableStereo = obj as InteractableStereo;
            if (!(interactableStereo == null))
            {
                reader.ReadUInt8(out var value2);
                interactableStereo.ReceiveChangeVolumeRequest(in context, value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveChangeVolumeRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveChangeVolumeRequest_Write(NetPakWriter writer, byte newVolume)
    {
        writer.WriteUInt8(newVolume);
    }
}
