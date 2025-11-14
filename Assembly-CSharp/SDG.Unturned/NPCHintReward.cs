using System;

namespace SDG.Unturned;

public class NPCHintReward : INPCReward
{
    /// <summary>
    /// How many seconds message should popup.
    /// </summary>
    private float duration;

    private CachingAssetRef hintTextAsset;

    private string hintTextLocKey;

    public override void GrantReward(Player player)
    {
        if (hintTextAsset.Get() != null && !string.IsNullOrEmpty(hintTextLocKey))
        {
            player.ServerShowTranslatedHint(hintTextAsset.Get(), hintTextLocKey, duration);
        }
        else
        {
            player.ServerShowHint(text, duration);
        }
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (string.IsNullOrEmpty(text))
        {
            text = p.data.GetString("Text");
        }
        else if (p.errorContext is Asset { Localization: not null } asset)
        {
            hintTextAsset = asset;
            hintTextLocKey = p.data.GetString("TextId");
        }
        duration = p.data.ParseFloat("Duration", 2f);
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        if (string.IsNullOrEmpty(text))
        {
            text = p.data.GetString(p.legacyPrefix + "_Text");
        }
        else if (p.errorContext is Asset { Localization: not null })
        {
            hintTextAsset = p.errorContext as Asset;
            hintTextLocKey = p.legacyPrefix;
        }
        duration = p.data.ParseFloat(p.legacyPrefix + "_Duration", 2f);
    }

    public NPCHintReward()
    {
    }

    [Obsolete]
    public NPCHintReward(float newDuration, string newText)
        : base(newText)
    {
        duration = newDuration;
    }
}
