using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableOil))]
public static class InteractableOil_NetMethods
{
    private static void ReceiveFuel_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableOil interactableOil = voidNetObj as InteractableOil;
        if (!(interactableOil == null))
        {
            context.reader.ReadUInt16(out var value);
            interactableOil.ReceiveFuel(value);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveFuel", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveFuel_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            NetInvocationDeferralRegistry.Defer(value, in context, ReceiveFuel_DeferredRead);
            return;
        }
        InteractableOil interactableOil = obj as InteractableOil;
        if (!(interactableOil == null))
        {
            reader.ReadUInt16(out var value2);
            interactableOil.ReceiveFuel(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveFuel", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveFuel_Write(NetPakWriter writer, ushort newFuel)
    {
        writer.WriteUInt16(newFuel);
    }
}
