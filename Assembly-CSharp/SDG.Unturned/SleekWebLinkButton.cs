namespace SDG.Unturned;

public class SleekWebLinkButton : SleekWrapper
{
    private string _url;

    private ISleekButton internalButton;

    public string Text
    {
        get
        {
            return internalButton.Text;
        }
        set
        {
            internalButton.Text = value;
        }
    }

    public string Url
    {
        get
        {
            return _url;
        }
        set
        {
            _url = value;
            internalButton.TooltipText = _url;
        }
    }

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

    public SleekWebLinkButton()
    {
        internalButton = Glazier.Get().CreateButton();
        internalButton.SizeScale_X = 1f;
        internalButton.SizeScale_Y = 1f;
        internalButton.OnClicked += OnClicked;
        AddChild(internalButton);
    }

    private void OnClicked(ISleekElement button)
    {
        if (WebUtils.ParseThirdPartyUrl(_url, out var result))
        {
            Provider.openURL(result);
            return;
        }
        UnturnedLog.warn("Ignoring potentially unsafe web link button url {0}", _url);
    }
}
