using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(PlayerInput))]
public static class PlayerInput_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveSimulateMispredictedInputs", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSimulateMispredictedInputs_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerInput playerInput = obj as PlayerInput;
            if (!(playerInput == null))
            {
                reader.ReadUInt32(out var value2);
                reader.ReadEnum(out var value3);
                reader.ReadClampedVector3(out var value4);
                reader.ReadClampedVector3(out var value5);
                reader.ReadUInt8(out var value6);
                reader.ReadInt32(out var value7);
                reader.ReadInt32(out var value8);
                playerInput.ReceiveSimulateMispredictedInputs(value2, value3, value4, value5, value6, value7, value8);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSimulateMispredictedInputs", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSimulateMispredictedInputs_Write(NetPakWriter writer, uint frameNumber, EPlayerStance stance, Vector3 position, Vector3 velocity, byte stamina, int lastTireOffset, int lastRestOffset)
    {
        writer.WriteUInt32(frameNumber);
        writer.WriteEnum(stance);
        writer.WriteClampedVector3(position);
        writer.WriteClampedVector3(velocity);
        writer.WriteUInt8(stamina);
        writer.WriteInt32(lastTireOffset);
        writer.WriteInt32(lastRestOffset);
    }

    [NetInvokableGeneratedMethod("ReceiveAckGoodInputs", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAckGoodInputs_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerInput playerInput = obj as PlayerInput;
            if (!(playerInput == null))
            {
                reader.ReadUInt32(out var value2);
                playerInput.ReceiveAckGoodInputs(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAckGoodInputs", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAckGoodInputs_Write(NetPakWriter writer, uint frameNumber)
    {
        writer.WriteUInt32(frameNumber);
    }

    [NetInvokableGeneratedMethod("ReceiveInputs", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveInputs_Read(in ServerInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerInput playerInput = obj as PlayerInput;
        if (!(playerInput == null))
        {
            if (!context.IsOwnerOf(playerInput.channel))
            {
                context.Kick($"not owner of {playerInput}");
            }
            else
            {
                playerInput.ReceiveInputs(in context);
            }
        }
    }
}
