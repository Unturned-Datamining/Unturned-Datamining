using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableBarricade))]
public static class UseableBarricade_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveBarricadeVehicle", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveBarricadeVehicle_Read(in ServerInvocationContext context)
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
        UseableBarricade useableBarricade = obj as UseableBarricade;
        if (!(useableBarricade == null))
        {
            if (!context.IsOwnerOf(useableBarricade.channel))
            {
                context.Kick($"not owner of {useableBarricade}");
                return;
            }
            reader.ReadClampedVector3(out var value2, 13, 11);
            reader.ReadFloat(out var value3);
            reader.ReadFloat(out var value4);
            reader.ReadFloat(out var value5);
            reader.ReadNetId(out var value6);
            useableBarricade.ReceiveBarricadeVehicle(in context, value2, value3, value4, value5, value6);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveBarricadeVehicle", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveBarricadeVehicle_Write(NetPakWriter writer, Vector3 newPoint, float newAngle_X, float newAngle_Y, float newAngle_Z, NetId regionNetId)
    {
        writer.WriteClampedVector3(newPoint, 13, 11);
        writer.WriteFloat(newAngle_X);
        writer.WriteFloat(newAngle_Y);
        writer.WriteFloat(newAngle_Z);
        writer.WriteNetId(regionNetId);
    }

    [NetInvokableGeneratedMethod("ReceiveBarricadeNone", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveBarricadeNone_Read(in ServerInvocationContext context)
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
        UseableBarricade useableBarricade = obj as UseableBarricade;
        if (!(useableBarricade == null))
        {
            if (!context.IsOwnerOf(useableBarricade.channel))
            {
                context.Kick($"not owner of {useableBarricade}");
                return;
            }
            reader.ReadClampedVector3(out var value2, 13, 11);
            reader.ReadFloat(out var value3);
            reader.ReadFloat(out var value4);
            reader.ReadFloat(out var value5);
            useableBarricade.ReceiveBarricadeNone(value2, value3, value4, value5);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveBarricadeNone", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveBarricadeNone_Write(NetPakWriter writer, Vector3 newPoint, float newAngle_X, float newAngle_Y, float newAngle_Z)
    {
        writer.WriteClampedVector3(newPoint, 13, 11);
        writer.WriteFloat(newAngle_X);
        writer.WriteFloat(newAngle_Y);
        writer.WriteFloat(newAngle_Z);
    }

    [NetInvokableGeneratedMethod("ReceivePlayBuild", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayBuild_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableBarricade useableBarricade = obj as UseableBarricade;
            if (!(useableBarricade == null))
            {
                useableBarricade.ReceivePlayBuild();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayBuild", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayBuild_Write(NetPakWriter writer)
    {
    }
}
