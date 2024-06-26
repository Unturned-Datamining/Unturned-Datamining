using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableVehiclePaint))]
public static class UseableVehiclePaint_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePlayReplace", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayReplace_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableVehiclePaint useableVehiclePaint = obj as UseableVehiclePaint;
            if (!(useableVehiclePaint == null))
            {
                useableVehiclePaint.ReceivePlayReplace();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayReplace", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayReplace_Write(NetPakWriter writer)
    {
    }
}
