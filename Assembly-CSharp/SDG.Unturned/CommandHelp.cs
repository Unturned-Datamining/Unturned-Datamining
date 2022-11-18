using Steamworks;

namespace SDG.Unturned;

public class CommandHelp : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (string.IsNullOrEmpty(parameter))
        {
            CommandWindow.Log(localization.format("HelpText"));
            string text = "";
            for (int i = 0; i < Commander.commands.Count; i++)
            {
                if (!string.IsNullOrEmpty(Commander.commands[i].info))
                {
                    text += Commander.commands[i].info;
                    if (i < Commander.commands.Count - 1)
                    {
                        text += "\n";
                    }
                }
            }
            CommandWindow.Log(text);
            return;
        }
        for (int j = 0; j < Commander.commands.Count; j++)
        {
            if (parameter.ToLower() == Commander.commands[j].command.ToLower())
            {
                if (executorID == CSteamID.Nil)
                {
                    CommandWindow.Log(Commander.commands[j].info);
                    CommandWindow.Log(Commander.commands[j].help);
                }
                else
                {
                    ChatManager.say(executorID, Commander.commands[j].info, Palette.SERVER, EChatMode.SAY);
                    ChatManager.say(executorID, Commander.commands[j].help, Palette.SERVER, EChatMode.SAY);
                }
                return;
            }
        }
        if (executorID == CSteamID.Nil)
        {
            CommandWindow.Log(localization.format("NoCommandErrorText", parameter));
        }
        else
        {
            ChatManager.say(executorID, localization.format("NoCommandErrorText", parameter), Palette.SERVER, EChatMode.SAY);
        }
    }

    public CommandHelp(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("HelpCommandText");
        _info = localization.format("HelpInfoText");
        _help = localization.format("HelpHelpText");
    }
}
