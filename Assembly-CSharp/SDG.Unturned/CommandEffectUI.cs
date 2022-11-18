using System;
using SDG.NetTransport;
using Steamworks;

namespace SDG.Unturned;

public class CommandEffectUI : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer)
        {
            return;
        }
        if (executorID == CSteamID.Nil && Provider.clients.Count > 0)
        {
            executorID = Provider.clients[0].playerID.steamID;
        }
        ITransportConnection transportConnection = Provider.findTransportConnection(executorID);
        if (transportConnection == null)
        {
            return;
        }
        string[] array = parameter.Split('/');
        string text = ((array.Length != 0) ? array[0] : parameter);
        if (!ushort.TryParse(text, out var result))
        {
            if (text.Equals("clearall", StringComparison.InvariantCultureIgnoreCase))
            {
                UnturnedLog.info("Clearing all effects");
                EffectManager.askEffectClearAll();
            }
        }
        else if (array.Length < 2)
        {
            EffectManager.sendUIEffect(result, 1, transportConnection, reliable: true);
        }
        else if (array.Length == 2)
        {
            if (array[1].Equals("clearbyid", StringComparison.InvariantCulture))
            {
                UnturnedLog.info("Clearing UI effects with ID {0}", result);
                EffectManager.askEffectClearByID(result, transportConnection);
            }
            else
            {
                EffectManager.sendUIEffect(result, 1, transportConnection, reliable: true, array[1]);
            }
        }
        else if (array.Length == 3)
        {
            EffectManager.sendUIEffect(result, 1, transportConnection, reliable: true, array[1], array[2]);
        }
        else if (array.Length == 4)
        {
            EffectManager.sendUIEffect(result, 1, transportConnection, reliable: true, array[1], array[2], array[3]);
        }
        else if (array.Length == 5)
        {
            EffectManager.sendUIEffect(result, 1, transportConnection, reliable: true, array[1], array[2], array[3], array[4]);
        }
    }

    public CommandEffectUI(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("EffectCommandText");
        _info = localization.format("EffectInfoText");
        _help = localization.format("EffectHelpText");
    }
}
