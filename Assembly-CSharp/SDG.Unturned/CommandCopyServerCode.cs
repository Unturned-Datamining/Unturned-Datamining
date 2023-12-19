using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class CommandCopyServerCode : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Provider.isServer && !(executorID != CSteamID.Nil))
        {
            string text2 = (GUIUtility.systemCopyBuffer = SteamGameServer.GetSteamID().ToString());
            CommandWindow.Log("Copied server code (" + text2 + ") to clipboard");
        }
    }

    public CommandCopyServerCode(Local newLocalization)
    {
        localization = newLocalization;
        _command = "CopyServerCode";
        _info = "CopyServerCode";
        _help = "Copies the Server Code to the system clipboard. Your friends can join the server by Server Code without port forwarding.";
    }
}
