using Steamworks;

namespace SDG.Unturned;

public class CommandLoadout : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        string[] componentsFromSerial = Parser.getComponentsFromSerial(parameter, '/');
        if (componentsFromSerial.Length < 1)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
            return;
        }
        if (!byte.TryParse(componentsFromSerial[0], out var result) || (result != byte.MaxValue && result > 10))
        {
            CommandWindow.LogError(localization.format("InvalidSkillsetIDErrorText", componentsFromSerial[0]));
            return;
        }
        ushort[] array = new ushort[componentsFromSerial.Length - 1];
        for (int i = 1; i < componentsFromSerial.Length; i++)
        {
            if (!ushort.TryParse(componentsFromSerial[i], out var result2))
            {
                CommandWindow.LogError(localization.format("InvalidItemIDErrorText", componentsFromSerial[i]));
                return;
            }
            array[i - 1] = result2;
        }
        if (result == byte.MaxValue)
        {
            PlayerInventory.loadout = array;
        }
        else
        {
            PlayerInventory.skillsets[result] = array;
        }
        CommandWindow.Log(localization.format("LoadoutText", result));
    }

    public CommandLoadout(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("LoadoutCommandText");
        _info = localization.format("LoadoutInfoText");
        _help = localization.format("LoadoutHelpText");
    }
}
