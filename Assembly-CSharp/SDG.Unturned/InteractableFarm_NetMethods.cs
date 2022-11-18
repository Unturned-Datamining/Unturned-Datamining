using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableFarm))]
public static class InteractableFarm_NetMethods
{
    private static void ReceivePlanted_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableFarm interactableFarm = voidNetObj as InteractableFarm;
        if (!(interactableFarm == null))
        {
            context.reader.ReadUInt32(out var value);
            interactableFarm.ReceivePlanted(value);
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlanted", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlanted_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            NetInvocationDeferralRegistry.Defer(value, in context, ReceivePlanted_DeferredRead);
            return;
        }
        InteractableFarm interactableFarm = obj as InteractableFarm;
        if (!(interactableFarm == null))
        {
            reader.ReadUInt32(out var value2);
            interactableFarm.ReceivePlanted(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlanted", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlanted_Write(NetPakWriter writer, uint newPlanted)
    {
        writer.WriteUInt32(newPlanted);
    }

    [NetInvokableGeneratedMethod("ReceiveHarvestRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveHarvestRequest_Read(in ServerInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            InteractableFarm interactableFarm = obj as InteractableFarm;
            if (!(interactableFarm == null))
            {
                interactableFarm.ReceiveHarvestRequest(in context);
            }
        }
    }
}
