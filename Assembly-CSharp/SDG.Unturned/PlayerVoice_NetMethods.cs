using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(PlayerVoice))]
public static class PlayerVoice_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePermissions", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePermissions_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerVoice playerVoice = obj as PlayerVoice;
            if (!(playerVoice == null))
            {
                reader.ReadBit(out var value2);
                reader.ReadBit(out var value3);
                playerVoice.ReceivePermissions(value2, value3);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePermissions", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePermissions_Write(NetPakWriter writer, bool allowTalkingWhileDead, bool customAllowTalking)
    {
        writer.WriteBit(allowTalkingWhileDead);
        writer.WriteBit(customAllowTalking);
    }

    [NetInvokableGeneratedMethod("ReceiveVoiceChatRelay", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVoiceChatRelay_Read(in ServerInvocationContext context)
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
        PlayerVoice playerVoice = obj as PlayerVoice;
        if (!(playerVoice == null))
        {
            if (!context.IsOwnerOf(playerVoice.channel))
            {
                context.Kick($"not owner of {playerVoice}");
            }
            else
            {
                playerVoice.ReceiveVoiceChatRelay(in context);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayVoiceChat", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayVoiceChat_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerVoice playerVoice = obj as PlayerVoice;
            if (!(playerVoice == null))
            {
                playerVoice.ReceivePlayVoiceChat(in context);
            }
        }
    }
}
