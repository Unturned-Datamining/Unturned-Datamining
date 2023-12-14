using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class CommandDebug : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Dedicator.IsDedicatedServer)
        {
            if (!Provider.isServer)
            {
                CommandWindow.LogError(localization.format("NotRunningErrorText"));
                return;
            }
            CommandWindow.Log(localization.format("DebugText"));
            CommandWindow.Log(localization.format("DebugUPSText", Mathf.CeilToInt((float)Provider.debugUPS / 50f * 100f)));
            CommandWindow.Log(localization.format("DebugTPSText", Mathf.CeilToInt((float)Provider.debugTPS / 50f * 100f)));
            CommandWindow.Log(localization.format("DebugZombiesText", ZombieManager.tickingZombies.Count));
            CommandWindow.Log(localization.format("DebugAnimalsText", AnimalManager.tickingAnimals.Count));
        }
    }

    public CommandDebug(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("DebugCommandText");
        _info = localization.format("DebugInfoText");
        _help = localization.format("DebugHelpText");
    }
}
