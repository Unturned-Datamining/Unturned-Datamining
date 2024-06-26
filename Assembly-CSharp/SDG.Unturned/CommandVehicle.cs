using System;
using System.Collections.Generic;
using Steamworks;

namespace SDG.Unturned;

public class CommandVehicle : Command
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
        string text = componentsFromSerial[(!flag) ? 1u : 0u];
        Guid result;
        ushort result2;
        Asset asset = (Guid.TryParse(text, out result) ? Assets.find(result) : ((!ushort.TryParse(text, out result2)) ? FindByString(text) : Assets.find(EAssetType.VEHICLE, result2)));
        if (asset == null)
        {
            CommandWindow.LogError(localization.format("NoVehicleIDErrorText", text));
            return;
        }
        InteractableVehicle interactableVehicle = VehicleTool.SpawnVehicleForPlayer(player.player, asset);
        if (interactableVehicle == null)
        {
            CommandWindow.LogError(localization.format("NoVehicleIDErrorText", asset.FriendlyName));
        }
        else
        {
            CommandWindow.Log(localization.format("VehicleText", player.playerID.playerName, interactableVehicle.asset.FriendlyName));
        }
    }

    public CommandVehicle(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("VehicleCommandText");
        _info = localization.format("VehicleInfoText");
        _help = localization.format("VehicleHelpText");
    }

    private Asset FindByString(string input)
    {
        input = input.Trim();
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }
        List<VehicleAsset> list = new List<VehicleAsset>();
        Assets.find(list);
        foreach (VehicleAsset item in list)
        {
            if (string.Equals(input, item.name, StringComparison.InvariantCultureIgnoreCase))
            {
                return item;
            }
        }
        foreach (VehicleAsset item2 in list)
        {
            if (string.Equals(input, item2.vehicleName, StringComparison.InvariantCultureIgnoreCase))
            {
                return item2;
            }
        }
        foreach (VehicleAsset item3 in list)
        {
            if (item3.name.Contains(input, StringComparison.InvariantCultureIgnoreCase))
            {
                return item3;
            }
        }
        foreach (VehicleAsset item4 in list)
        {
            if (item4.vehicleName.Contains(input, StringComparison.InvariantCultureIgnoreCase))
            {
                return item4;
            }
        }
        return null;
    }
}
