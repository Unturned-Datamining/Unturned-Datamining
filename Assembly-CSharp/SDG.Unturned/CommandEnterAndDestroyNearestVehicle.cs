using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Nelson 2025-01-28: This command reproduces a bug destroying the player gameObject if the vehicle is
/// destroyed on the same frame as the request to enter.
/// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/4760#issuecomment-2613090165
/// </summary>
public class CommandEnterAndDestroyNearestVehicle : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer || !Provider.hasCheats)
        {
            return;
        }
        SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(executorID);
        if (steamPlayer == null || steamPlayer.player == null)
        {
            return;
        }
        if (steamPlayer.player.movement.getVehicle() != null)
        {
            CommandWindow.LogError("Cannot enter and destroy nearest vehicle if already driving");
            return;
        }
        InteractableVehicle interactableVehicle = null;
        float num = 16f;
        foreach (InteractableVehicle vehicle in VehicleManager.vehicles)
        {
            float num2 = Vector3.Distance(vehicle.transform.position, steamPlayer.player.transform.position);
            if (num2 < num)
            {
                interactableVehicle = vehicle;
                num = num2;
            }
        }
        if (interactableVehicle == null)
        {
            CommandWindow.LogError("No nearby vehicle to enter and destroy");
            return;
        }
        if (!interactableVehicle.tryAddPlayer(out var seat, steamPlayer.player))
        {
            CommandWindow.LogError("No seat for vehicle to enter and destroy");
            return;
        }
        VehicleManager.SendEnterVehicle.InvokeAndLoopback(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), interactableVehicle.instanceID, seat, executorID);
        VehicleManager.askVehicleDestroy(interactableVehicle);
    }

    public CommandEnterAndDestroyNearestVehicle(Local newLocalization)
    {
        localization = newLocalization;
        _command = "EnterAndDestroyNearestVehicle";
        _info = string.Empty;
        _help = string.Empty;
    }
}
