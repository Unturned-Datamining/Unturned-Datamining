using Steamworks;

namespace SDG.Unturned;

/// <summary>
/// Nelson 2025-01-28: This command reproduces a bug destroying the player gameObject if the vehicle is
/// destroyed on the same frame as the request to exit.
/// https://github.com/SmartlyDressedGames/Unturned-3.x-Community/issues/4760#issuecomment-2613090165
/// </summary>
public class CommandExitAndDestroyDrivenVehicle : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer || !Provider.hasCheats)
        {
            return;
        }
        SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(executorID);
        if (steamPlayer != null && !(steamPlayer.player == null))
        {
            InteractableVehicle vehicle = steamPlayer.player.movement.getVehicle();
            if (!(vehicle == null))
            {
                vehicle.forceRemovePlayer(out var seat, executorID, out var point, out var angle);
                VehicleManager.sendExitVehicle(vehicle, seat, point, angle, forceUpdate: false);
                VehicleManager.askVehicleDestroy(vehicle);
            }
        }
    }

    public CommandExitAndDestroyDrivenVehicle(Local newLocalization)
    {
        localization = newLocalization;
        _command = "ExitAndDestroyDrivenVehicle";
        _info = string.Empty;
        _help = string.Empty;
    }
}
