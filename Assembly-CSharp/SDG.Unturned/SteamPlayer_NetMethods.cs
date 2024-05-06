using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(SteamPlayer))]
public static class SteamPlayer_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveGetSteamAuthTicketForWebApiRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveGetSteamAuthTicketForWebApiRequest_Read(in ClientInvocationContext context)
    {
        context.reader.ReadString(out var value);
        SteamPlayer.ReceiveGetSteamAuthTicketForWebApiRequest(value);
    }

    [NetInvokableGeneratedMethod("ReceiveGetSteamAuthTicketForWebApiRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveGetSteamAuthTicketForWebApiRequest_Write(NetPakWriter writer, string identity)
    {
        writer.WriteString(identity);
    }
}
