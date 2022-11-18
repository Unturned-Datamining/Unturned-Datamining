using Steamworks;

namespace SDG.Unturned;

public class CommandPlayers : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Provider.clients.Count == 0)
        {
            CommandWindow.LogError(localization.format("NoPlayersErrorText"));
            return;
        }
        CommandWindow.Log(localization.format("PlayersText"));
        for (int i = 0; i < Provider.clients.Count; i++)
        {
            SteamPlayer steamPlayer = Provider.clients[i];
            CommandWindow.Log(localization.format("PlayerIDText", steamPlayer.playerID.steamID, steamPlayer.playerID.playerName, steamPlayer.playerID.characterName, (int)(steamPlayer.ping * 1000f)));
        }
    }

    public CommandPlayers(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("PlayersCommandText");
        _info = localization.format("PlayersInfoText");
        _help = localization.format("PlayersHelpText");
    }
}
