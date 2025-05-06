using System;
using UnityEngine;

namespace SDG.Unturned;

public class INPCReward
{
    /// <summary>
    /// If &gt;0 the game will start a coroutine to grant the reward after waiting.
    /// </summary>
    public float grantDelaySeconds = -1f;

    /// <summary>
    /// If true and player has this reward pending when they die or disconnect it will be granted.
    /// </summary>
    public bool grantDelayApplyWhenInterrupted;

    protected string text;

    public virtual void GrantReward(Player player)
    {
    }

    public virtual string formatReward(Player player)
    {
        if (!string.IsNullOrEmpty(text))
        {
            return text;
        }
        return null;
    }

    public virtual ISleekElement createUI(Player player)
    {
        string value = formatReward(player);
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        sleekBox.SizeOffset_Y = 30f;
        sleekBox.SizeScale_X = 1f;
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.PositionOffset_X = 5f;
        sleekLabel.SizeOffset_X = -10f;
        sleekLabel.SizeScale_X = 1f;
        sleekLabel.SizeScale_Y = 1f;
        sleekLabel.TextAlignment = TextAnchor.MiddleLeft;
        sleekLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        sleekLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        sleekLabel.AllowRichText = true;
        sleekLabel.Text = value;
        sleekBox.AddChild(sleekLabel);
        return sleekBox;
    }

    /// <summary>
    /// Intended to replace filling data from constructor.
    /// </summary>
    internal virtual void PopulateV2(in PopulateRewardParameters p)
    {
        string @string = p.data.GetString("TextId");
        if (!string.IsNullOrEmpty(@string))
        {
            string text = p.localization.read(@string);
            if (!string.IsNullOrEmpty(text))
            {
                text = ItemTool.filterRarityRichText(text);
                this.text = text;
            }
            else
            {
                p.ReportError("no text for reward text ID \"" + @string + "\"");
            }
        }
        grantDelaySeconds = p.data.ParseFloat("GrantDelaySeconds", -1f);
        if (grantDelaySeconds > 0f)
        {
            grantDelayApplyWhenInterrupted = p.data.ParseBool("GrantDelayApplyWhenInterrupted");
        }
    }

    /// <summary>
    /// Intended to replace filling data from constructor. Legacy is for backwards compatibility with Reward_#_Key
    /// format, whereas V2 uses the list and dictionary features.
    /// </summary>
    internal virtual void PopulateLegacy(in PopulateRewardParameters p)
    {
        string desc = p.localization.read(p.legacyPrefix);
        desc = ItemTool.filterRarityRichText(desc);
        text = desc;
        grantDelaySeconds = p.data.ParseFloat(p.legacyPrefix + "_GrantDelaySeconds", -1f);
        if (grantDelaySeconds > 0f)
        {
            grantDelayApplyWhenInterrupted = p.data.ParseBool(p.legacyPrefix + "_GrantDelayApplyWhenInterrupted");
        }
    }

    public INPCReward()
    {
    }

    [Obsolete]
    public INPCReward(string newText)
    {
        text = newText;
    }

    [Obsolete("Removed shouldSend parameter because GrantReward is only called on the server now")]
    public virtual void grantReward(Player player, bool shouldSend)
    {
        GrantReward(player);
    }
}
