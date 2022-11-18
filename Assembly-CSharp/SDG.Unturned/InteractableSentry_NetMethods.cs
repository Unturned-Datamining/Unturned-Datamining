using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableSentry))]
public static class InteractableSentry_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveShoot", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveShoot_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            InteractableSentry interactableSentry = obj as InteractableSentry;
            if (!(interactableSentry == null))
            {
                interactableSentry.ReceiveShoot();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveShoot", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveShoot_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceiveAlert", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAlert_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            InteractableSentry interactableSentry = obj as InteractableSentry;
            if (!(interactableSentry == null))
            {
                reader.ReadUInt8(out var value2);
                reader.ReadUInt8(out var value3);
                interactableSentry.ReceiveAlert(value2, value3);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAlert", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAlert_Write(NetPakWriter writer, byte yaw, byte pitch)
    {
        writer.WriteUInt8(yaw);
        writer.WriteUInt8(pitch);
    }
}
