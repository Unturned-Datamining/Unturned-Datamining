using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableThrowable))]
public static class UseableThrowable_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveToss", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveToss_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableThrowable useableThrowable = obj as UseableThrowable;
            if (!(useableThrowable == null))
            {
                reader.ReadClampedVector3(out var value2);
                reader.ReadClampedVector3(out var value3);
                useableThrowable.ReceiveToss(value2, value3);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveToss", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveToss_Write(NetPakWriter writer, Vector3 origin, Vector3 force)
    {
        writer.WriteClampedVector3(origin);
        writer.WriteClampedVector3(force);
    }

    [NetInvokableGeneratedMethod("ReceivePlaySwing", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlaySwing_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableThrowable useableThrowable = obj as UseableThrowable;
            if (!(useableThrowable == null))
            {
                useableThrowable.ReceivePlaySwing();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlaySwing", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlaySwing_Write(NetPakWriter writer)
    {
    }
}
