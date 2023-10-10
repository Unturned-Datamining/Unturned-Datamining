namespace SDG.Unturned;

public class SleekReadMoreButton : SleekWrapper
{
    public ISleekElement targetContent;

    public string onText;

    public string offText;

    private ISleekButton internalButton;

    public override bool UseManualLayout
    {
        set
        {
            base.UseManualLayout = value;
            internalButton.UseManualLayout = value;
            internalButton.UseChildAutoLayout = ((!value) ? ESleekChildLayout.Horizontal : ESleekChildLayout.None);
            internalButton.ExpandChildren = !value;
        }
    }

    public void Refresh()
    {
        internalButton.Text = (targetContent.IsVisible ? offText : onText);
    }

    public SleekReadMoreButton()
    {
        internalButton = Glazier.Get().CreateButton();
        internalButton.SizeScale_X = 1f;
        internalButton.SizeScale_Y = 1f;
        internalButton.OnClicked += OnClicked;
        AddChild(internalButton);
    }

    private void OnClicked(ISleekElement button)
    {
        targetContent.IsVisible = !targetContent.IsVisible;
        Refresh();
    }
}
