namespace SDG.Unturned;

public class SleekStockpileLinkButton : SleekWrapper
{
    public int itemdefid;

    internal ISleekButton internalButton;

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

    public SleekStockpileLinkButton()
    {
        internalButton = Glazier.Get().CreateButton();
        internalButton.SizeScale_X = 1f;
        internalButton.SizeScale_Y = 1f;
        internalButton.OnClicked += OnClicked;
        AddChild(internalButton);
    }

    private void OnClicked(ISleekElement button)
    {
        ItemStore.Get().ViewItem(itemdefid);
    }
}
