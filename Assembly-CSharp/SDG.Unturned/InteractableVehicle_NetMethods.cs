using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableVehicle))]
public static class InteractableVehicle_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePaintColor", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePaintColor_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            InteractableVehicle interactableVehicle = obj as InteractableVehicle;
            if (!(interactableVehicle == null))
            {
                reader.ReadColor32RGBA(out Color32 value2);
                interactableVehicle.ReceivePaintColor(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePaintColor", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePaintColor_Write(NetPakWriter writer, Color32 newPaintColor)
    {
        writer.WriteColor32RGBA(newPaintColor);
    }
}
