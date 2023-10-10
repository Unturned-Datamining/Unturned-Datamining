using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class INPCCondition
{
    protected string text;

    protected bool shouldReset;

    internal List<int> uiRequirementIndices;

    public virtual bool isConditionMet(Player player)
    {
        return false;
    }

    public virtual void ApplyCondition(Player player)
    {
    }

    public virtual string formatCondition(Player player)
    {
        if (!string.IsNullOrEmpty(text))
        {
            return text;
        }
        return null;
    }

    public virtual ISleekElement createUI(Player player, Texture2D icon)
    {
        string value = formatCondition(player);
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        sleekBox.SizeOffset_Y = 30f;
        sleekBox.SizeScale_X = 1f;
        if (icon != null)
        {
            ISleekImage sleekImage = Glazier.Get().CreateImage(icon);
            sleekImage.PositionOffset_X = 5f;
            sleekImage.PositionOffset_Y = 5f;
            sleekImage.SizeOffset_X = 20f;
            sleekImage.SizeOffset_Y = 20f;
            sleekBox.AddChild(sleekImage);
        }
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        if (icon != null)
        {
            sleekLabel.PositionOffset_X = 30f;
            sleekLabel.SizeOffset_X = -35f;
        }
        else
        {
            sleekLabel.PositionOffset_X = 5f;
            sleekLabel.SizeOffset_X = -10f;
        }
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

    public virtual bool isAssociatedWithFlag(ushort flagID)
    {
        return false;
    }

    internal virtual void GatherAssociatedFlags(HashSet<ushort> associatedFlags)
    {
    }

    public bool AreUIRequirementsMet(List<bool> areConditionsMet)
    {
        if (uiRequirementIndices == null || uiRequirementIndices.Count < 1)
        {
            return true;
        }
        foreach (int uiRequirementIndex in uiRequirementIndices)
        {
            if (uiRequirementIndex >= 0 && uiRequirementIndex < areConditionsMet.Count && !areConditionsMet[uiRequirementIndex])
            {
                return false;
            }
        }
        return true;
    }

    public INPCCondition(string newText, bool newShouldReset)
    {
        text = newText;
        shouldReset = newShouldReset;
    }

    [Obsolete("Removed shouldSend parameter because ApplyCondition is only called on the server now")]
    public virtual void applyCondition(Player player, bool shouldSend)
    {
        ApplyCondition(player);
    }
}
