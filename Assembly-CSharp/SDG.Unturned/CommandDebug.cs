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
            CommandWindow.Log(localization.format("DebugIPPortText", SteamGameServer.GetSteamID(), SteamGameServer.GetPublicIP(), Provider.port));
            CommandWindow.Log(localization.format("DebugBytesSentText", Provider.bytesSent + "B"));
            CommandWindow.Log(localization.format("DebugBytesReceivedText", Provider.bytesReceived + "B"));
            CommandWindow.Log(localization.format("DebugAverageBytesSentText", (uint)((float)Provider.bytesSent / Time.realtimeSinceStartup) + "B"));
            CommandWindow.Log(localization.format("DebugAverageBytesReceivedText", (uint)((float)Provider.bytesReceived / Time.realtimeSinceStartup) + "B"));
            CommandWindow.Log(localization.format("DebugPacketsSentText", Provider.packetsSent));
            CommandWindow.Log(localization.format("DebugPacketsReceivedText", Provider.packetsReceived));
            CommandWindow.Log(localization.format("DebugAveragePacketsSentText", (uint)((float)Provider.packetsSent / Time.realtimeSinceStartup)));
            CommandWindow.Log(localization.format("DebugAveragePacketsReceivedText", (uint)((float)Provider.packetsReceived / Time.realtimeSinceStartup)));
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
