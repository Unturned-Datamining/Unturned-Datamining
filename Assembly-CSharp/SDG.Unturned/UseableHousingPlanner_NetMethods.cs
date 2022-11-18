using System;
using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableHousingPlanner))]
public static class UseableHousingPlanner_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePlaceHousingItemResult", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlaceHousingItemResult_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableHousingPlanner useableHousingPlanner = obj as UseableHousingPlanner;
            if (!(useableHousingPlanner == null))
            {
                reader.ReadBit(out var value2);
                useableHousingPlanner.ReceivePlaceHousingItemResult(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlaceHousingItemResult", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlaceHousingItemResult_Write(NetPakWriter writer, bool success)
    {
        writer.WriteBit(success);
    }

    [NetInvokableGeneratedMethod("ReceivePlaceHousingItem", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlaceHousingItem_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        UseableHousingPlanner useableHousingPlanner = obj as UseableHousingPlanner;
        if (!(useableHousingPlanner == null))
        {
            if (!context.IsOwnerOf(useableHousingPlanner.channel))
            {
                context.Kick($"not owner of {useableHousingPlanner}");
                return;
            }
            reader.ReadGuid(out var value2);
            reader.ReadClampedVector3(out var value3, 13, 11);
            reader.ReadFloat(out var value4);
            useableHousingPlanner.ReceivePlaceHousingItem(in context, value2, value3, value4);
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlaceHousingItem", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlaceHousingItem_Write(NetPakWriter writer, Guid assetGuid, Vector3 position, float yaw)
    {
        writer.WriteGuid(assetGuid);
        writer.WriteClampedVector3(position, 13, 11);
        writer.WriteFloat(yaw);
    }
}
