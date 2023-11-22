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
        mainButton.IsVisible = true;
        confirmButton.IsVisible = false;
        denyButton.IsVisible = false;
    }

    private void onClickedConfirmButton(ISleekElement button)
    {
        reset();
        onConfirmed?.Invoke(this);
    }

    private void onClickedDenyButton(ISleekElement button)
    {
        reset();
        onDenied?.Invoke(this);
    }

    private void onClickedMainButton(ISleekElement button)
    {
        mainButton.IsVisible = false;
        confirmButton.IsVisible = true;
        denyButton.IsVisible = true;
    }

    public SleekButtonIconConfirm(Texture2D newIcon, string newConfirm, string newConfirmTooltip, string newDeny, string newDenyTooltip)
        : this(newIcon, newConfirm, newConfirmTooltip, newDeny, newDenyTooltip, 0)
    {
    }

    public SleekButtonIconConfirm(Texture2D newIcon, string newConfirm, string newConfirmTooltip, string newDeny, string newDenyTooltip, int iconSize)
    {
        mainButton = new SleekButtonIcon(newIcon, iconSize);
        mainButton.SizeScale_X = 1f;
        mainButton.SizeScale_Y = 1f;
        mainButton.onClickedButton += onClickedMainButton;
        AddChild(mainButton);
        confirmButton = Glazier.Get().CreateButton();
        confirmButton.SizeOffset_X = -5f;
        confirmButton.SizeScale_X = 0.5f;
        confirmButton.SizeScale_Y = 1f;
        confirmButton.Text = newConfirm;
        confirmButton.TooltipText = newConfirmTooltip;
        confirmButton.OnClicked += onClickedConfirmButton;
        AddChild(confirmButton);
        confirmButton.IsVisible = false;
        denyButton = Glazier.Get().CreateButton();
        denyButton.PositionOffset_X = 5f;
        denyButton.PositionScale_X = 0.5f;
        denyButton.SizeOffset_X = -5f;
        denyButton.SizeScale_X = 0.5f;
        denyButton.SizeScale_Y = 1f;
        denyButton.Text = newDeny;
        denyButton.TooltipText = newDenyTooltip;
        denyButton.OnClicked += onClickedDenyButton;
        AddChild(denyButton);
        denyButton.IsVisible = false;
    }
}
