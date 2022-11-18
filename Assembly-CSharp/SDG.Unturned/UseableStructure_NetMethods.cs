using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableStructure))]
public static class UseableStructure_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveBuildStructure", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveBuildStructure_Read(in ServerInvocationContext context)
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
        UseableStructure useableStructure = obj as UseableStructure;
        if (!(useableStructure == null))
        {
            if (!context.IsOwnerOf(useableStructure.channel))
            {
                context.Kick($"not owner of {useableStructure}");
                return;
            }
            reader.ReadClampedVector3(out var value2, 13, 11);
            reader.ReadFloat(out var value3);
            useableStructure.ReceiveBuildStructure(in context, value2, value3);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveBuildStructure", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveBuildStructure_Write(NetPakWriter writer, Vector3 newPoint, float newAngle)
    {
        writer.WriteClampedVector3(newPoint, 13, 11);
        writer.WriteFloat(newAngle);
    }

    [NetInvokableGeneratedMethod("ReceivePlayConstruct", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayConstruct_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableStructure useableStructure = obj as UseableStructure;
            if (!(useableStructure == null))
            {
                useableStructure.ReceivePlayConstruct();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayConstruct", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayConstruct_Write(NetPakWriter writer)
    {
    }
}
