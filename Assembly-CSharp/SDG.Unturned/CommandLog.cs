using Steamworks;

namespace SDG.Unturned;

public class CommandLog : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            return;
        }
        string[] componentsFromSerial = Parser.getComponentsFromSerial(parameter, '/');
        if (componentsFromSerial.Length != 4)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
            return;
        }
        bool shouldLogChat;
        if (componentsFromSerial[0].ToLower() == "y")
        {
            shouldLogChat = true;
        }
        else
        {
            if (!(componentsFromSerial[0].ToLower() == "n"))
            {
                CommandWindow.LogError(localization.format("InvalidBooleanErrorText", componentsFromSerial[0]));
                return;
            }
            shouldLogChat = false;
        }
        bool shouldLogJoinLeave;
        if (componentsFromSerial[1].ToLower() == "y")
        {
            shouldLogJoinLeave = true;
        }
        else
        {
            if (!(componentsFromSerial[1].ToLower() == "n"))
            {
                CommandWindow.LogError(localization.format("InvalidBooleanErrorText", componentsFromSerial[1]));
                return;
            }
            shouldLogJoinLeave = false;
        }
        bool shouldLogDeaths;
        if (componentsFromSerial[2].ToLower() == "y")
        {
            shouldLogDeaths = true;
        }
        else
        {
            if (!(componentsFromSerial[2].ToLower() == "n"))
            {
                CommandWindow.LogError(localization.format("InvalidBooleanErrorText", componentsFromSerial[2]));
                return;
            }
            shouldLogDeaths = false;
        }
        bool shouldLogAnticheat;
        if (componentsFromSerial[3].ToLower() == "y")
        {
            shouldLogAnticheat = true;
        }
        else
        {
            if (!(componentsFromSerial[3].ToLower() == "n"))
            {
                CommandWindow.LogError(localization.format("InvalidBooleanErrorText", componentsFromSerial[3]));
                return;
            }
            shouldLogAnticheat = false;
        }
        CommandWindow.shouldLogChat = shouldLogChat;
        CommandWindow.shouldLogJoinLeave = shouldLogJoinLeave;
        CommandWindow.shouldLogDeaths = shouldLogDeaths;
        CommandWindow.shouldLogAnticheat = shouldLogAnticheat;
        CommandWindow.Log(localization.format("LogText"));
    }

    public CommandLog(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("LogCommandText");
        _info = localization.format("LogInfoText");
        _help = localization.format("LogHelpText");
    }
}
