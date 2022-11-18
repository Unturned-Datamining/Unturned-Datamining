using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableVehicleBattery))]
public static class UseableVehicleBattery_NetMethods
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
            UseableVehicleBattery useableVehicleBattery = obj as UseableVehicleBattery;
            if (!(useableVehicleBattery == null))
            {
                useableVehicleBattery.ReceivePlayReplace();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayReplace", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayReplace_Write(NetPakWriter writer)
    {
    }
}
