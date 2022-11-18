using UnityEngine;

namespace SDG.Unturned;

public class INPCReward
{
    protected string text;

    public virtual void grantReward(Player player, bool shouldSend)
    {
    }

    public virtual string formatReward(Player player)
    {
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
        sleekBox.sizeOffset_Y = 30;
        sleekBox.sizeScale_X = 1f;
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.positionOffset_X = 5;
        sleekLabel.sizeOffset_X = -10;
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

    public INPCReward(string newText)
    {
        text = newText;
    }
}
