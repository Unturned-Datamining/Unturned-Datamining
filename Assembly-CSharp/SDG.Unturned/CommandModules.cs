using SDG.Framework.Modules;
using Steamworks;

namespace SDG.Unturned;

public class CommandModules : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (ModuleHook.modules.Count == 0)
        {
            CommandWindow.LogError(localization.format("NoModulesErrorText"));
            return;
        }
        CommandWindow.Log(localization.format("ModulesText"));
        CommandWindow.Log(localization.format("SeparatorText"));
        for (int i = 0; i < ModuleHook.modules.Count; i++)
        {
            Module module = ModuleHook.modules[i];
            if (module != null)
            {
                ModuleConfig config = module.config;
                if (config != null)
                {
                    Local local = Localization.tryRead(config.DirectoryPath, usePath: false);
                    CommandWindow.Log(local.format("Name"));
                    CommandWindow.Log(localization.format("Version", config.Version));
                    CommandWindow.Log(local.format("Description"));
                    CommandWindow.Log(localization.format("SeparatorText"));
                }
            }
        }
    }

    public CommandModules(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("ModulesCommandText");
        _info = localization.format("ModulesInfoText");
        _help = localization.format("ModulesHelpText");
    }
}
