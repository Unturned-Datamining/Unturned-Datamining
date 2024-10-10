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
