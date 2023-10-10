namespace SDG.Unturned;

internal class PlayerBrowserRequestUI : SleekFullscreenBox
{
    private Local localization;

    private ISleekBox textBox;

    private ISleekButton yesButton;

    private ISleekButton noButton;

    private string url;

    public bool isActive { get; private set; }

    public void open(string msg, string url)
    {
        if (!isActive)
        {
            isActive = true;
            this.url = url;
            textBox.Text = localization.format("Request") + "\n" + url + "\n\n\"" + msg + "\"";
            AnimateIntoView();
        }
    }

    public void close()
    {
        if (isActive)
        {
            isActive = false;
            url = null;
            AnimateOutOfView(0f, 1f);
        }
    }

    private void onClickedYesButton(ISleekElement button)
    {
        if (!string.IsNullOrEmpty(url) && Provider.provider.browserService.canOpenBrowser)
        {
            Provider.provider.browserService.open(url);
        }
        PlayerLifeUI.open();
        close();
    }

    private void onClickedNoButton(ISleekElement button)
    {
        PlayerLifeUI.open();
        close();
    }

    public PlayerBrowserRequestUI()
    {
        localization = Localization.read("/Player/PlayerBrowserRequest.dat");
        base.PositionScale_Y = 1f;
        base.PositionOffset_X = 10f;
        base.PositionOffset_Y = 10f;
        base.SizeOffset_X = -20f;
        base.SizeOffset_Y = -20f;
        base.SizeScale_X = 1f;
        base.SizeScale_Y = 1f;
        isActive = false;
        url = null;
        textBox = Glazier.Get().CreateBox();
        textBox.PositionOffset_X = -200f;
        textBox.PositionOffset_Y = -50f;
        textBox.PositionScale_X = 0.5f;
        textBox.PositionScale_Y = 0.5f;
        textBox.SizeOffset_X = 400f;
        textBox.SizeOffset_Y = 100f;
        AddChild(textBox);
        yesButton = Glazier.Get().CreateButton();
        yesButton.PositionOffset_X = -200f;
        yesButton.PositionOffset_Y = 60f;
        yesButton.PositionScale_X = 0.5f;
        yesButton.PositionScale_Y = 0.5f;
        yesButton.SizeOffset_X = 195f;
        yesButton.SizeOffset_Y = 30f;
        yesButton.Text = localization.format("Yes_Button");
        yesButton.TooltipText = localization.format("Yes_Button_Tooltip");
        yesButton.OnClicked += onClickedYesButton;
        AddChild(yesButton);
        noButton = Glazier.Get().CreateButton();
        noButton.PositionOffset_X = 5f;
        noButton.PositionOffset_Y = 60f;
        noButton.PositionScale_X = 0.5f;
        noButton.PositionScale_Y = 0.5f;
        noButton.SizeOffset_X = 195f;
        noButton.SizeOffset_Y = 30f;
        noButton.Text = localization.format("No_Button");
        noButton.TooltipText = localization.format("No_Button_Tooltip");
        noButton.OnClicked += onClickedNoButton;
        AddChild(noButton);
    }
}
