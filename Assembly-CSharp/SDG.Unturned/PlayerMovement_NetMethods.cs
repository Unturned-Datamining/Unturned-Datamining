using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(PlayerMovement))]
public static class PlayerMovement_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePluginGravityMultiplier", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePluginGravityMultiplier_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerMovement playerMovement = obj as PlayerMovement;
            if (!(playerMovement == null))
            {
                reader.ReadFloat(out var value2);
                playerMovement.ReceivePluginGravityMultiplier(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePluginGravityMultiplier", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePluginGravityMultiplier_Write(NetPakWriter writer, float newPluginGravityMultiplier)
    {
        writer.WriteFloat(newPluginGravityMultiplier);
    }

    [NetInvokableGeneratedMethod("ReceivePluginJumpMultiplier", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePluginJumpMultiplier_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerMovement playerMovement = obj as PlayerMovement;
            if (!(playerMovement == null))
            {
                reader.ReadFloat(out var value2);
                playerMovement.ReceivePluginJumpMultiplier(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePluginJumpMultiplier", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePluginJumpMultiplier_Write(NetPakWriter writer, float newPluginJumpMultiplier)
    {
        writer.WriteFloat(newPluginJumpMultiplier);
    }

    [NetInvokableGeneratedMethod("ReceivePluginSpeedMultiplier", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePluginSpeedMultiplier_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerMovement playerMovement = obj as PlayerMovement;
            if (!(playerMovement == null))
            {
                reader.ReadFloat(out var value2);
                playerMovement.ReceivePluginSpeedMultiplier(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePluginSpeedMultiplier", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePluginSpeedMultiplier_Write(NetPakWriter writer, float newPluginSpeedMultiplier)
    {
        writer.WriteFloat(newPluginSpeedMultiplier);
    }
}
