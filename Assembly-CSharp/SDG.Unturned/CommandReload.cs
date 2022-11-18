using System;
using System.IO;
using Steamworks;

namespace SDG.Unturned;

public class CommandReload : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer)
        {
            return;
        }
        if (Guid.TryParse(parameter, out var result))
        {
            Asset asset = Assets.find(result);
            if (asset != null)
            {
                Assets.reload(Path.GetDirectoryName(asset.absoluteOriginFilePath));
            }
        }
        else if (Directory.Exists(parameter))
        {
            Assets.reload(Path.GetFullPath(parameter));
        }
    }

    public CommandReload(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("ReloadCommandText");
        _info = localization.format("ReloadInfoText");
        _help = localization.format("ReloadHelpText");
    }
}
