using System;

namespace SDG.Unturned;

public class NPCCurrencyReward : INPCReward
{
    public AssetReference<ItemCurrencyAsset> currency { get; protected set; }

    public uint value { get; protected set; }

    public override void GrantReward(Player player)
    {
        currency.Find()?.grantValue(player, value);
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            ItemCurrencyAsset itemCurrencyAsset = currency.Find();
            if (itemCurrencyAsset != null && !string.IsNullOrEmpty(itemCurrencyAsset.valueFormat))
            {
                text = itemCurrencyAsset.valueFormat;
            }
            else
            {
                text = PlayerNPCQuestUI.localization.read("Reward_Currency");
            }
        }
        return Local.FormatText(text, value);
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseGuid("GUID", out var gUID))
        {
            currency = new AssetReference<ItemCurrencyAsset>(gUID);
        }
        else
        {
            p.ReportRequiredOptionInvalid("GUID");
        }
        if (p.data.TryParseUInt32("Value", out var num))
        {
            value = num;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseGuid(p.legacyPrefix + "_GUID", out var gUID))
        {
            currency = new AssetReference<ItemCurrencyAsset>(gUID);
        }
        else
        {
            p.ReportRequiredOptionInvalid("GUID");
        }
        if (p.data.TryParseUInt32(p.legacyPrefix + "_Value", out var num))
        {
            value = num;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    public NPCCurrencyReward()
    {
    }

    [Obsolete]
    public NPCCurrencyReward(AssetReference<ItemCurrencyAsset> newCurrency, uint newValue, string newText)
        : base(newText)
    {
        currency = newCurrency;
        value = newValue;
    }
}
