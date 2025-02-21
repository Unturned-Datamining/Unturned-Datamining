using Steamworks;

namespace SDG.Unturned;

public class CommandDestroyDrivenVehicle : Command
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
                VehicleManager.askVehicleDestroy(vehicle);
            }
        }
    }

    public CommandDestroyDrivenVehicle(Local newLocalization)
    {
        localization = newLocalization;
        _command = "DestroyDrivenVehicle";
        _info = string.Empty;
        _help = string.Empty;
    }
}
