using Steamworks;

namespace SDG.Unturned;

public class CommandChatrate : Command
{
    private static readonly float MIN_NUMBER = 0f;

    private static readonly float MAX_NUMBER = 60f;

    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!float.TryParse(parameter, out var result))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
            return;
        }
        if (result < MIN_NUMBER)
        {
            CommandWindow.LogError(localization.format("MinNumberErrorText", MIN_NUMBER));
            return;
        }
        if (result > MAX_NUMBER)
        {
            CommandWindow.LogError(localization.format("MaxNumberErrorText", MAX_NUMBER));
            return;
        }
        ChatManager.chatrate = result;
        CommandWindow.Log(localization.format("ChatrateText", result));
    }

    public CommandChatrate(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("ChatrateCommandText");
        _info = localization.format("ChatrateInfoText");
        _help = localization.format("ChatrateHelpText");
    }
}
