using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(LightningWeatherComponent))]
public static class LightningWeatherComponent_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveLightningStrike", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveLightningStrike_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            LightningWeatherComponent lightningWeatherComponent = obj as LightningWeatherComponent;
            if (!(lightningWeatherComponent == null))
            {
                reader.ReadClampedVector3(out var value2);
                lightningWeatherComponent.ReceiveLightningStrike(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveLightningStrike", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveLightningStrike_Write(NetPakWriter writer, Vector3 hitPosition)
    {
        writer.WriteClampedVector3(hitPosition);
    }
}
