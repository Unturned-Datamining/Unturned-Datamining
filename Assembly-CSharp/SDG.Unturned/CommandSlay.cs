using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class CommandSlay : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer)
        {
            CommandWindow.LogError(localization.format("NotRunningErrorText"));
            return;
        }
        string[] componentsFromSerial = Parser.getComponentsFromSerial(parameter, '/');
        if (componentsFromSerial.Length != 1 && componentsFromSerial.Length != 2)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
            return;
        }
        if (!PlayerTool.tryGetSteamPlayer(componentsFromSerial[0], out var player))
        {
            CommandWindow.LogError(localization.format("NoPlayerErrorText", componentsFromSerial[0]));
            return;
        }
        uint iPv4AddressOrZero = player.getIPv4AddressOrZero();
        if (componentsFromSerial.Length == 1)
        {
            Provider.requestBanPlayer(executorID, player.playerID.steamID, iPv4AddressOrZero, player.playerID.GetHwids(), localization.format("SlayTextReason"), SteamBlacklist.PERMANENT);
        }
        else if (componentsFromSerial.Length == 2)
        {
            Provider.requestBanPlayer(executorID, player.playerID.steamID, iPv4AddressOrZero, player.playerID.GetHwids(), componentsFromSerial[1], SteamBlacklist.PERMANENT);
        }
        if (player.player != null)
        {
            player.player.life.askDamage(101, Vector3.up * 101f, EDeathCause.KILL, ELimb.SKULL, executorID, out var _);
        }
        CommandWindow.Log(localization.format("SlayText", player.playerID.playerName));
    }

    public CommandSlay(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("SlayCommandText");
        _info = localization.format("SlayInfoText");
        _help = localization.format("SlayHelpText");
    }
}
