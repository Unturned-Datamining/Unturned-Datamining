using System;
using Steamworks;

namespace SDG.Unturned;

public class CommandQuest : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer)
        {
            CommandWindow.LogError(localization.format("NotRunningErrorText"));
            return;
        }
        if (!Provider.hasCheats)
        {
            CommandWindow.LogError(localization.format("CheatsErrorText"));
            return;
        }
        string[] componentsFromSerial = Parser.getComponentsFromSerial(parameter, '/');
        if (componentsFromSerial.Length < 1 || componentsFromSerial.Length > 2)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
            return;
        }
        bool flag = false;
        if (!PlayerTool.tryGetSteamPlayer(componentsFromSerial[0], out var player))
        {
            player = PlayerTool.getSteamPlayer(executorID);
            if (player == null)
            {
                CommandWindow.LogError(localization.format("NoPlayerErrorText", componentsFromSerial[0]));
                return;
            }
            flag = true;
        }
        QuestAsset questAsset = null;
        string text = componentsFromSerial[(!flag) ? 1u : 0u];
        ushort result2;
        if (Guid.TryParse(text, out var result))
        {
            questAsset = Assets.find<QuestAsset>(result);
        }
        else if (!ushort.TryParse(text, out result2))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", text));
            return;
        }
        if (questAsset != null)
        {
            player.player.quests.ServerAddQuest(questAsset);
        }
        CommandWindow.Log(localization.format("QuestText", player.playerID.playerName, questAsset?.FriendlyName ?? text));
    }

    public CommandQuest(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("QuestCommandText");
        _info = localization.format("QuestInfoText");
        _help = localization.format("QuestHelpText");
    }
}
