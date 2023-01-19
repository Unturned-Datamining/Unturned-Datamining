using System;
using Steamworks;

namespace SDG.Unturned;

public class CommandAllowP2PRelay : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Dedicator.IsDedicatedServer)
        {
            bool flag = true;
            if (parameter == "0" || parameter.Equals("no", StringComparison.InvariantCultureIgnoreCase) || parameter.Equals("off", StringComparison.InvariantCultureIgnoreCase) || parameter.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                flag = false;
            }
            SteamGameServerNetworking.AllowP2PPacketRelay(flag);
            CommandWindow.Log(localization.format(flag ? "On" : "Off"));
        }
    }

    public CommandAllowP2PRelay(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("CommandText");
        _info = localization.format("InfoText");
        _help = localization.format("HelpText");
    }
}
