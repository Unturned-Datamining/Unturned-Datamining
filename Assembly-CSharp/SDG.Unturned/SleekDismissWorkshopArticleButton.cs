namespace SDG.Unturned;

public class SleekDismissWorkshopArticleButton : SleekWrapper
{
    public ulong articleId;

    public ISleekElement targetContent;

    internal ISleekButton internalButton;

    public override bool UseManualLayout
    {
        set
        {
            base.UseManualLayout = value;
            internalButton.UseManualLayout = value;
        }
    }

    public SleekDismissWorkshopArticleButton()
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
        LocalNews.dismissWorkshopItem(articleId);
    }
}
