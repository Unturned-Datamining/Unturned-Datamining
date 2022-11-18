using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class CommandQueue : Command
{
    public static readonly byte MAX_NUMBER = 64;

    protected override void execute(CSteamID executorID, string parameter)
    {
        switch (parameter)
        {
        case "a":
        {
            SteamPending steamPending = new SteamPending();
            if (Provider.pending.Count == 1)
            {
                steamPending.sendVerifyPacket();
            }
            Provider.pending.Add(steamPending);
            CommandWindow.Log("add dummy");
            return;
        }
        case "r":
            Provider.reject(CSteamID.Nil, ESteamRejection.PING);
            CommandWindow.Log("rmv dummy");
            return;
        case "ad":
        {
            for (int i = 0; i < 12; i++)
            {
                Provider.pending.Add(new SteamPending(null, new SteamPlayerID(CSteamID.Nil, 0, "dummy", "dummy", "dummy", CSteamID.Nil), newPro: true, 0, 0, 0, Color.white, Color.white, Color.white, newHand: false, 0uL, 0uL, 0uL, 0uL, 0uL, 0uL, 0uL, new ulong[0], EPlayerSkillset.NONE, "english", CSteamID.Nil, EClientPlatform.Windows));
                Provider.accept(new SteamPlayerID(CSteamID.Nil, 1, "dummy", "dummy", "dummy", CSteamID.Nil), isPro: true, isAdmin: true, 0, 0, 0, Color.white, Color.white, Color.white, hand: false, 0, 0, 0, 0, 0, 0, 0, new int[0], new string[0], new string[0], EPlayerSkillset.NONE, "english", CSteamID.Nil, EClientPlatform.Windows);
            }
            break;
        }
        case "rd":
        {
            for (int num = Provider.clients.Count - 1; num >= 0; num--)
            {
                Provider.kick(CSteamID.Nil, "");
            }
            break;
        }
        }
        if (!byte.TryParse(parameter, out var result))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
            return;
        }
        if (result > MAX_NUMBER)
        {
            CommandWindow.LogError(localization.format("MaxNumberErrorText", MAX_NUMBER));
            return;
        }
        Provider.queueSize = result;
        CommandWindow.Log(localization.format("QueueText", result));
    }

    public CommandQueue(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("QueueCommandText");
        _info = localization.format("QueueInfoText");
        _help = localization.format("QueueHelpText");
    }
}
