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
            textBox.text = localization.format("Request") + "\n" + url + "\n\n\"" + msg + "\"";
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
        base.positionScale_Y = 1f;
        base.positionOffset_X = 10;
        base.positionOffset_Y = 10;
        base.sizeOffset_X = -20;
        base.sizeOffset_Y = -20;
        base.sizeScale_X = 1f;
        base.sizeScale_Y = 1f;
        isActive = false;
        url = null;
        textBox = Glazier.Get().CreateBox();
        textBox.positionOffset_X = -200;
        textBox.positionOffset_Y = -50;
        textBox.positionScale_X = 0.5f;
        textBox.positionScale_Y = 0.5f;
        textBox.sizeOffset_X = 400;
        textBox.sizeOffset_Y = 100;
        AddChild(textBox);
        yesButton = Glazier.Get().CreateButton();
        yesButton.positionOffset_X = -200;
        yesButton.positionOffset_Y = 60;
        yesButton.positionScale_X = 0.5f;
        yesButton.positionScale_Y = 0.5f;
        yesButton.sizeOffset_X = 195;
        yesButton.sizeOffset_Y = 30;
        yesButton.text = localization.format("Yes_Button");
        yesButton.tooltipText = localization.format("Yes_Button_Tooltip");
        yesButton.onClickedButton += onClickedYesButton;
        AddChild(yesButton);
        noButton = Glazier.Get().CreateButton();
        noButton.positionOffset_X = 5;
        noButton.positionOffset_Y = 60;
        noButton.positionScale_X = 0.5f;
        noButton.positionScale_Y = 0.5f;
        noButton.sizeOffset_X = 195;
        noButton.sizeOffset_Y = 30;
        noButton.text = localization.format("No_Button");
        noButton.tooltipText = localization.format("No_Button_Tooltip");
        noButton.onClickedButton += onClickedNoButton;
        AddChild(noButton);
    }
}
