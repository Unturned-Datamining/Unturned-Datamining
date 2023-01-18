using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class INPCCondition
{
    protected string text;

    protected bool shouldReset;

    public virtual bool isConditionMet(Player player)
    {
        return false;
    }

    public virtual void applyCondition(Player player, bool shouldSend)
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
        sleekBox.sizeOffset_Y = 30;
        sleekBox.sizeScale_X = 1f;
        if (icon != null)
        {
            ISleekImage sleekImage = Glazier.Get().CreateImage(icon);
            sleekImage.positionOffset_X = 5;
            sleekImage.positionOffset_Y = 5;
            sleekImage.sizeOffset_X = 20;
            sleekImage.sizeOffset_Y = 20;
            sleekBox.AddChild(sleekImage);
        }
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        if (icon != null)
        {
            sleekLabel.positionOffset_X = 30;
            sleekLabel.sizeOffset_X = -35;
        }
        else
        {
            sleekLabel.positionOffset_X = 5;
            sleekLabel.sizeOffset_X = -10;
        }
        sleekLabel.sizeScale_X = 1f;
        sleekLabel.sizeScale_Y = 1f;
        sleekLabel.fontAlignment = TextAnchor.MiddleLeft;
        sleekLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        sleekLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        sleekLabel.enableRichText = true;
        sleekLabel.text = value;
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

    public INPCCondition(string newText, bool newShouldReset)
    {
        text = newText;
        shouldReset = newShouldReset;
    }
}
