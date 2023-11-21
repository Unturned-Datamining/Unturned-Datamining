using System;
using Steamworks;

namespace SDG.Unturned;

public class CommandGive : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer)
        {
            CommandWindow.LogError(localization.format("NotRunningErrorText"));
            return;
        }
        if (!Provider.hasCheats)
        {
            CommandWindow.LogError(localization.format("CheatsErrorText"));
            return;
        }
        string[] componentsFromSerial = Parser.getComponentsFromSerial(parameter, '/');
        if (componentsFromSerial.Length < 1 || componentsFromSerial.Length > 3)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
            return;
        }
        bool flag = false;
        if (!PlayerTool.tryGetSteamPlayer(componentsFromSerial[0], out var player))
        {
            player = PlayerTool.getSteamPlayer(executorID);
            if (player == null)
            {
                CommandWindow.LogError(localization.format("NoPlayerErrorText", componentsFromSerial[0]));
                return;
            }
            flag = true;
        }
        uint result = 1u;
        if (flag)
        {
            if (componentsFromSerial.Length > 1 && !uint.TryParse(componentsFromSerial[1], out result))
            {
                CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[1]));
                return;
            }
        }
        else if (componentsFromSerial.Length > 2 && !uint.TryParse(componentsFromSerial[2], out result))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[2]));
            return;
        }
        string text = componentsFromSerial[(!flag) ? 1u : 0u];
        ushort result3;
        if (Guid.TryParse(text, out var result2))
        {
            Asset asset = Assets.find(result2);
            if (asset is ItemAsset)
            {
                giveItem(player, asset.id, (byte)result);
            }
            else if (asset is ItemCurrencyAsset itemCurrencyAsset)
            {
                itemCurrencyAsset.grantValue(player.player, result);
            }
        }
        else if (!ushort.TryParse(text, out result3))
        {
            CommandWindow.LogError(localization.format("InvalidItemIDErrorText", text));
        }
        else
        {
            giveItem(player, result3, (byte)result);
        }
    }

    private void giveItem(SteamPlayer player, ushort itemID, byte amount)
    {
        if (!ItemTool.tryForceGiveItem(player.player, itemID, amount))
        {
            CommandWindow.LogError(localization.format("NoItemIDErrorText", itemID));
        }
        else
        {
            CommandWindow.Log(localization.format("GiveText", player.playerID.playerName, itemID, amount));
        }
    }

    public CommandGive(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("GiveCommandText");
        _info = localization.format("GiveInfoText");
        _help = localization.format("GiveHelpText");
    }
}
