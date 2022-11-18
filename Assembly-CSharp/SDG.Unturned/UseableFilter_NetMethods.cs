using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableFilter))]
public static class UseableFilter_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePlayFilter", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayFilter_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableFilter useableFilter = obj as UseableFilter;
            if (!(useableFilter == null))
            {
                useableFilter.ReceivePlayFilter();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayFilter", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayFilter_Write(NetPakWriter writer)
    {
    }
}
