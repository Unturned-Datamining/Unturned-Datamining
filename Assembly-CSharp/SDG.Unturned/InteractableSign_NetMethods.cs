using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableSign))]
public static class InteractableSign_NetMethods
{
    private static void ReceiveChangeText_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableSign interactableSign = voidNetObj as InteractableSign;
        if (!(interactableSign == null))
        {
            context.reader.ReadString(out var value);
            interactableSign.ReceiveChangeText(value);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveChangeText", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveChangeText_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            NetInvocationDeferralRegistry.Defer(value, in context, ReceiveChangeText_DeferredRead);
            return;
        }
        InteractableSign interactableSign = obj as InteractableSign;
        if (!(interactableSign == null))
        {
            reader.ReadString(out var value2);
            interactableSign.ReceiveChangeText(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveChangeText", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveChangeText_Write(NetPakWriter writer, string newText)
    {
        writer.WriteString(newText);
    }

    [NetInvokableGeneratedMethod("ReceiveChangeTextRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveChangeTextRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            InteractableSign interactableSign = obj as InteractableSign;
            if (!(interactableSign == null))
            {
                reader.ReadString(out var value2);
                interactableSign.ReceiveChangeTextRequest(in context, value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveChangeTextRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveChangeTextRequest_Write(NetPakWriter writer, string newText)
    {
        writer.WriteString(newText);
    }
}
