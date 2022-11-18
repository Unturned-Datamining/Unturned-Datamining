using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class CommandKill : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer)
        {
            CommandWindow.LogError(localization.format("NotRunningErrorText"));
            return;
        }
        if (!PlayerTool.tryGetSteamPlayer(parameter, out var player))
        {
            CommandWindow.LogError(localization.format("NoPlayerErrorText", parameter));
            return;
        }
        if (player.player != null)
        {
            player.player.life.askDamage(101, Vector3.up * 101f, EDeathCause.KILL, ELimb.SKULL, executorID, out var _);
        }
        CommandWindow.Log(localization.format("KillText", player.playerID.playerName));
    }

    public CommandKill(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("KillCommandText");
        _info = localization.format("KillInfoText");
        _help = localization.format("KillHelpText");
    }
}
