using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableObjectQuestEventHook))]
public static class InteractableObjectQuestEventHook_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveUsedNotification", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUsedNotification_Read(in ClientInvocationContext context)
    {
        context.reader.ReadTransform(out var value);
        InteractableObjectQuestEventHook.ReceiveUsedNotification(value);
    }

    [NetInvokableGeneratedMethod("ReceiveUsedNotification", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUsedNotification_Write(NetPakWriter writer, Transform eventHookTransform)
    {
        writer.WriteTransform(eventHookTransform);
    }
}
