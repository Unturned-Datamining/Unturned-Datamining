using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(PlayerAnimator))]
public static class PlayerAnimator_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveLean", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveLean_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerAnimator playerAnimator = obj as PlayerAnimator;
            if (!(playerAnimator == null))
            {
                reader.ReadUInt8(out var value2);
                playerAnimator.ReceiveLean(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveLean", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveLean_Write(NetPakWriter writer, byte newLean)
    {
        writer.WriteUInt8(newLean);
    }

    [NetInvokableGeneratedMethod("ReceiveGesture", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveGesture_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerAnimator playerAnimator = obj as PlayerAnimator;
            if (!(playerAnimator == null))
            {
                reader.ReadEnum(out var value2);
                playerAnimator.ReceiveGesture(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveGesture", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveGesture_Write(NetPakWriter writer, EPlayerGesture newGesture)
    {
        writer.WriteEnum(newGesture);
    }

    [NetInvokableGeneratedMethod("ReceiveGestureRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveGestureRequest_Read(in ServerInvocationContext context)
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
        PlayerAnimator playerAnimator = obj as PlayerAnimator;
        if (!(playerAnimator == null))
        {
            if (!context.IsOwnerOf(playerAnimator.channel))
            {
                context.Kick($"not owner of {playerAnimator}");
                return;
            }
            reader.ReadEnum(out var value2);
            playerAnimator.ReceiveGestureRequest(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveGestureRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveGestureRequest_Write(NetPakWriter writer, EPlayerGesture newGesture)
    {
        writer.WriteEnum(newGesture);
    }
}
