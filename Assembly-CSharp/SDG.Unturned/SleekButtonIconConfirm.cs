using UnityEngine;

namespace SDG.Unturned;

public class SleekButtonIconConfirm : SleekWrapper
{
    public Confirm onConfirmed;

    public Deny onDenied;

    private SleekButtonIcon mainButton;

    private ISleekButton confirmButton;

    private ISleekButton denyButton;

    public string text
    {
        get
        {
            return mainButton.text;
        }
        set
        {
            mainButton.text = value;
        }
    }

    public string tooltip
    {
        get
        {
            return mainButton.tooltip;
        }
        set
        {
            mainButton.tooltip = value;
        }
    }

    public ESleekFontSize fontSize
    {
        get
        {
            return mainButton.fontSize;
        }
        set
        {
            mainButton.fontSize = value;
        }
    }

    public SleekColor iconColor
    {
        get
        {
            return mainButton.iconColor;
        }
        set
        {
            mainButton.iconColor = value;
        }
    }

    public bool isClickable
    {
        get
        {
            return mainButton.isClickable;
        }
        set
        {
            mainButton.isClickable = value;
        }
    }

    public void reset()
    {
        mainButton.isVisible = true;
        confirmButton.isVisible = false;
        denyButton.isVisible = false;
    }

    private void onClickedConfirmButton(ISleekElement button)
    {
        reset();
        if (onConfirmed != null)
        {
            onConfirmed(this);
        }
    }

    private void onClickedDenyButton(ISleekElement button)
    {
        reset();
        if (onDenied != null)
        {
            onDenied(this);
        }
    }

    private void onClickedMainButton(ISleekElement button)
    {
        mainButton.isVisible = false;
        confirmButton.isVisible = true;
        denyButton.isVisible = true;
    }

    public SleekButtonIconConfirm(Texture2D newIcon, string newConfirm, string newConfirmTooltip, string newDeny, string newDenyTooltip)
    {
        mainButton = new SleekButtonIcon(newIcon);
        mainButton.sizeScale_X = 1f;
        mainButton.sizeScale_Y = 1f;
        mainButton.onClickedButton += onClickedMainButton;
        AddChild(mainButton);
        confirmButton = Glazier.Get().CreateButton();
        confirmButton.sizeOffset_X = -5;
        confirmButton.sizeScale_X = 0.5f;
        confirmButton.sizeScale_Y = 1f;
        confirmButton.text = newConfirm;
        confirmButton.tooltipText = newConfirmTooltip;
        confirmButton.onClickedButton += onClickedConfirmButton;
        AddChild(confirmButton);
        confirmButton.isVisible = false;
        denyButton = Glazier.Get().CreateButton();
        denyButton.positionOffset_X = 5;
        denyButton.positionScale_X = 0.5f;
        denyButton.sizeOffset_X = -5;
        denyButton.sizeScale_X = 0.5f;
        denyButton.sizeScale_Y = 1f;
        denyButton.text = newDeny;
        denyButton.tooltipText = newDenyTooltip;
        denyButton.onClickedButton += onClickedDenyButton;
        AddChild(denyButton);
        denyButton.isVisible = false;
    }
}
