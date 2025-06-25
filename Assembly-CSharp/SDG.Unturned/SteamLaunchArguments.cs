using Steamworks;

namespace SDG.Unturned;

public class SteamLaunchArguments
{
    private static string commandLine = string.Empty;

    public static string Get()
    {
        return commandLine;
    }

    internal static void Init()
    {
        if (SteamApps.GetLaunchCommandLine(out var pszCommandLine, 2048) > 0 && !string.IsNullOrEmpty(pszCommandLine))
        {
            commandLine = pszCommandLine;
            UnturnedLog.info("Steam launch command-line: \"" + commandLine + "\"");
            if (string.IsNullOrEmpty(CommandLine.commandLineOverride))
            {
                UnturnedLog.info("Overriding environment command-line with Steam's because environment's is empty");
                CommandLine.commandLineOverride = pszCommandLine;
            }
            else if (CommandLine.commandLineOverride.Contains(commandLine))
            {
                UnturnedLog.info("Skipping Steam's command-line because environment's already contains it");
            }
            else
            {
                UnturnedLog.info("Appending Steam's command-line to environment's because environment's is not empty");
                CommandLine.commandLineOverride = CommandLine.commandLineOverride + " " + pszCommandLine;
            }
        }
        else
        {
            UnturnedLog.info("Steam launch command-line is empty");
        }
    }
}
