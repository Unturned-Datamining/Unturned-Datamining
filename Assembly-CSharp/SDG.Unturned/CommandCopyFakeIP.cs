using Steamworks;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class CommandCopyFakeIP : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer || executorID != CSteamID.Nil)
        {
            return;
        }
        if (!Provider.configData.Server.Use_FakeIP)
        {
            CommandWindow.Log("Cannot copy Fake IP to clipboard because it's turned off in the server config.");
            return;
        }
        SteamGameServerNetworkingSockets.GetFakeIP(0, out var pInfo);
        if (pInfo.m_eResult == EResult.k_EResultBusy)
        {
            CommandWindow.Log("Cannot copy Fake IP to clipboard because it's not ready yet.");
        }
        else if (pInfo.m_eResult == EResult.k_EResultOK)
        {
            string arg = new IPv4Address(pInfo.m_unIP).ToString();
            string text2 = (GUIUtility.systemCopyBuffer = $"{arg}:{pInfo.m_unPorts[0]}");
            CommandWindow.Log("Copied Fake IP (" + text2 + ") to clipboard");
        }
        else
        {
            CommandWindow.LogError($"Copy Fake IP to clipboard fatal result: {pInfo.m_eResult}");
        }
    }

    public CommandCopyFakeIP(Local newLocalization)
    {
        localization = newLocalization;
        _command = "CopyFakeIP";
        _info = string.Empty;
        _help = string.Empty;
    }
}
