using System.Collections.Generic;
using SDG.NetTransport;
using Steamworks;

namespace SDG.Unturned;

public class CommandBan : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            return;
        }
        if (!Provider.isServer)
        {
            CommandWindow.LogError(localization.format("NotRunningErrorText"));
            return;
        }
        string[] componentsFromSerial = Parser.getComponentsFromSerial(parameter, '/');
        if (componentsFromSerial.Length != 1 && componentsFromSerial.Length != 2 && componentsFromSerial.Length != 3)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
            return;
        }
        if (!PlayerTool.tryGetSteamID(componentsFromSerial[0], out var steamID))
        {
            CommandWindow.LogError(localization.format("NoPlayerErrorText", componentsFromSerial[0]));
            return;
        }
        ITransportConnection transportConnection = Provider.findTransportConnection(steamID);
        uint address = 0u;
        transportConnection?.TryGetIPv4Address(out address);
        IEnumerable<byte[]> hwidsToBan = PlayerTool.getSteamPlayer(steamID)?.playerID.GetHwids();
        uint result;
        if (componentsFromSerial.Length == 1)
        {
            Provider.requestBanPlayer(executorID, steamID, address, hwidsToBan, localization.format("BanTextReason"), SteamBlacklist.PERMANENT);
            CommandWindow.Log(localization.format("BanTextPermanent", steamID));
        }
        else if (componentsFromSerial.Length == 2)
        {
            Provider.requestBanPlayer(executorID, steamID, address, hwidsToBan, componentsFromSerial[1], SteamBlacklist.PERMANENT);
            CommandWindow.Log(localization.format("BanTextPermanent", steamID));
        }
        else if (!uint.TryParse(componentsFromSerial[2], out result))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[2]));
        }
        else
        {
            Provider.requestBanPlayer(executorID, steamID, address, hwidsToBan, componentsFromSerial[1], result);
            CommandWindow.Log(localization.format("BanText", steamID, result));
        }
    }

    public CommandBan(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("BanCommandText");
        _info = localization.format("BanInfoText");
        _help = localization.format("BanHelpText");
    }
}
