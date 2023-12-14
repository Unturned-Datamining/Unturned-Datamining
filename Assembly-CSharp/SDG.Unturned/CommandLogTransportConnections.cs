using SDG.NetTransport;
using Steamworks;

namespace SDG.Unturned;

public class CommandLogTransportConnections : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer || executorID != CSteamID.Nil)
        {
            return;
        }
        foreach (SteamPlayer client in Provider.clients)
        {
            ITransportConnection transportConnection = client.transportConnection;
            if (transportConnection == null)
            {
                CommandWindow.Log($"Client {client.playerID} has no transport connection");
            }
            else
            {
                CommandWindow.Log($"{transportConnection} - {transportConnection.GetAddressString(withPort: true)}");
            }
        }
    }

    public CommandLogTransportConnections(Local newLocalization)
    {
        localization = newLocalization;
        _command = "LogTransportConnections";
        _info = string.Empty;
        _help = string.Empty;
    }
}
