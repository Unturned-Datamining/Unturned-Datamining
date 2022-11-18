using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(PlayerStance))]
public static class PlayerStance_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveClimbRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveClimbRequest_Read(in ServerInvocationContext context)
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
        PlayerStance playerStance = obj as PlayerStance;
        if (!(playerStance == null))
        {
            if (!context.IsOwnerOf(playerStance.channel))
            {
                context.Kick($"not owner of {playerStance}");
                return;
            }
            reader.ReadClampedVector3(out var value2);
            playerStance.ReceiveClimbRequest(in context, value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveClimbRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveClimbRequest_Write(NetPakWriter writer, Vector3 direction)
    {
        writer.WriteClampedVector3(direction);
    }

    [NetInvokableGeneratedMethod("ReceiveStance", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveStance_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerStance playerStance = obj as PlayerStance;
            if (!(playerStance == null))
            {
                reader.ReadEnum(out var value2);
                playerStance.ReceiveStance(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveStance", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveStance_Write(NetPakWriter writer, EPlayerStance newStance)
    {
        writer.WriteEnum(newStance);
    }
}
