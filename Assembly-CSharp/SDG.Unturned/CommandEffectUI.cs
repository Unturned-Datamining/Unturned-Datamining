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
        if (text.Equals("clearall", StringComparison.InvariantCultureIgnoreCase))
        {
            UnturnedLog.info("Clearing all effects");
            EffectManager.askEffectClearAll();
            return;
        }
        EffectAsset effectAsset;
        if (Guid.TryParse(text, out var result))
        {
            effectAsset = Assets.find(result) as EffectAsset;
        }
        else
        {
            if (!ushort.TryParse(text, out var result2))
            {
                return;
            }
            effectAsset = Assets.find(EAssetType.EFFECT, result2) as EffectAsset;
        }
        if (array.Length < 2)
        {
            EffectManager.SendUIEffect(effectAsset, 1, transportConnection, reliable: true);
        }
        else if (array.Length == 2)
        {
            if (array[1].Equals("clearbyid", StringComparison.InvariantCulture))
            {
                UnturnedLog.info("Clearing UI effects with GUID {0}", effectAsset.GUID);
                EffectManager.ClearEffectByGuid(effectAsset.GUID, transportConnection);
            }
            else
            {
                EffectManager.SendUIEffect(effectAsset, 1, transportConnection, reliable: true, array[1]);
            }
        }
        else if (array.Length == 3)
        {
            EffectManager.SendUIEffect(effectAsset, 1, transportConnection, reliable: true, array[1], array[2]);
        }
        else if (array.Length == 4)
        {
            EffectManager.SendUIEffect(effectAsset, 1, transportConnection, reliable: true, array[1], array[2], array[3]);
        }
        else if (array.Length == 5)
        {
            EffectManager.SendUIEffect(effectAsset, 1, transportConnection, reliable: true, array[1], array[2], array[3], array[4]);
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
