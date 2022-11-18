using SDG.NetPak;
using Steamworks;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableBed))]
public static class InteractableBed_NetMethods
{
    private static void ReceiveClaim_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableBed interactableBed = voidNetObj as InteractableBed;
        if (!(interactableBed == null))
        {
            context.reader.ReadSteamID(out CSteamID value);
            interactableBed.ReceiveClaim(value);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveClaim", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveClaim_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            NetInvocationDeferralRegistry.Defer(value, in context, ReceiveClaim_DeferredRead);
            return;
        }
        InteractableBed interactableBed = obj as InteractableBed;
        if (!(interactableBed == null))
        {
            reader.ReadSteamID(out CSteamID value2);
            interactableBed.ReceiveClaim(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveClaim", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveClaim_Write(NetPakWriter writer, CSteamID newOwner)
    {
        writer.WriteSteamID(newOwner);
    }

    [NetInvokableGeneratedMethod("ReceiveClaimRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveClaimRequest_Read(in ServerInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            InteractableBed interactableBed = obj as InteractableBed;
            if (!(interactableBed == null))
            {
                interactableBed.ReceiveClaimRequest(in context);
            }
        }
    }
}
