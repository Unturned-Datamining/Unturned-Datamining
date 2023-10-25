using System;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class CommandTeleport : Command
{
    /// <summary>
    /// Cast a ray from the sky to find highest point.
    /// </summary>
    protected bool raycastFromSkyToPosition(ref Vector3 position)
    {
        position.y = 1024f;
        if (Physics.Raycast(position, Vector3.down, out var hitInfo, 2048f, RayMasks.WAYPOINT))
        {
            position = hitInfo.point + Vector3.up;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Cast a ray from slightly above point so indoor teleport nodes work.
    /// </summary>
    protected void raycastFromNearPosition(ref Vector3 position)
    {
        if (Physics.Raycast(position + new Vector3(0f, 4f, 0f), Vector3.down, out var hitInfo, 8f, RayMasks.WAYPOINT))
        {
            position = hitInfo.point + Vector3.up;
        }
    }

    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer)
        {
            CommandWindow.LogError(localization.format("NotRunningErrorText"));
            return;
        }
        string[] componentsFromSerial = Parser.getComponentsFromSerial(parameter, '/');
        if (componentsFromSerial.Length < 1 || componentsFromSerial.Length > 2)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
            return;
        }
        bool flag = componentsFromSerial.Length == 1;
        SteamPlayer player;
        if (flag)
        {
            player = PlayerTool.getSteamPlayer(executorID);
        }
        else
        {
            PlayerTool.tryGetSteamPlayer(componentsFromSerial[0], out player);
        }
        if (player == null)
        {
            CommandWindow.LogError(localization.format("NoPlayerErrorText", componentsFromSerial[0]));
            return;
        }
        if (PlayerTool.tryGetSteamPlayer(componentsFromSerial[(!flag) ? 1 : 0], out var player2))
        {
            if (player2.player.movement.getVehicle() != null)
            {
                CommandWindow.LogError(localization.format("NoVehicleErrorText"));
            }
            else if (player.player.teleportToPlayer(player2.player))
            {
                CommandWindow.Log(localization.format("TeleportText", player.playerID.playerName, player2.playerID.playerName));
            }
            else
            {
                CommandWindow.LogError(localization.format("TeleportObstructed", player.playerID.playerName, player2.playerID.playerName));
            }
            return;
        }
        if (componentsFromSerial[(!flag) ? 1 : 0].Equals(localization.format("WaypointCommand"), StringComparison.InvariantCultureIgnoreCase) && player.player.quests.isMarkerPlaced)
        {
            Vector3 position = player.player.quests.markerPosition;
            if (raycastFromSkyToPosition(ref position))
            {
                if (player.player.teleportToLocation(position, player.player.transform.rotation.eulerAngles.y))
                {
                    CommandWindow.Log(localization.format("TeleportText", player.playerID.playerName, localization.format("WaypointText")));
                }
                else
                {
                    CommandWindow.LogError(localization.format("TeleportObstructed", player.playerID.playerName, localization.format("WaypointText")));
                }
            }
            return;
        }
        if (componentsFromSerial[(!flag) ? 1 : 0].Equals(localization.format("BedCommand"), StringComparison.InvariantCultureIgnoreCase))
        {
            if (player.player.teleportToBed())
            {
                CommandWindow.Log(localization.format("TeleportText", player.playerID.playerName, localization.format("BedText")));
            }
            else
            {
                CommandWindow.LogError(localization.format("TeleportObstructed", player.playerID.playerName, localization.format("BedText")));
            }
            return;
        }
        LocationDevkitNode locationDevkitNode = null;
        foreach (LocationDevkitNode allNode in LocationDevkitNodeSystem.Get().GetAllNodes())
        {
            if (NameTool.checkNames(componentsFromSerial[(!flag) ? 1 : 0], allNode.locationName))
            {
                locationDevkitNode = allNode;
                break;
            }
        }
        if (locationDevkitNode != null)
        {
            Vector3 position2 = locationDevkitNode.transform.position;
            raycastFromNearPosition(ref position2);
            if (player.player.teleportToLocation(position2, player.player.transform.rotation.eulerAngles.y))
            {
                CommandWindow.Log(localization.format("TeleportText", player.playerID.playerName, locationDevkitNode.name));
            }
            else
            {
                CommandWindow.LogError(localization.format("TeleportObstructed", player.playerID.playerName, locationDevkitNode.name));
            }
        }
        else
        {
            CommandWindow.LogError(localization.format("NoLocationErrorText", componentsFromSerial[(!flag) ? 1 : 0]));
        }
    }

    public CommandTeleport(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("TeleportCommandText");
        _info = localization.format("TeleportInfoText");
        _help = localization.format("TeleportHelpText");
    }
}
