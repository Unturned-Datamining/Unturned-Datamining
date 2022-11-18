using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableConsumeable))]
public static class UseableConsumeable_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePlayConsume", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayConsume_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableConsumeable useableConsumeable = obj as UseableConsumeable;
            if (!(useableConsumeable == null))
            {
                reader.ReadEnum(out var value2);
                useableConsumeable.ReceivePlayConsume(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayConsume", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayConsume_Write(NetPakWriter writer, EConsumeMode mode)
    {
        writer.WriteEnum(mode);
    }
}
