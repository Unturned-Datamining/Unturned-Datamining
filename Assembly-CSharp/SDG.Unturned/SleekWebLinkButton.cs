using System;

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
        int num = _url.IndexOf("store.steampowered.com/itemstore/304930/detail/", StringComparison.OrdinalIgnoreCase);
        if (num >= 0)
        {
            int num2 = num + "store.steampowered.com/itemstore/304930/detail/".Length;
            int num3 = _url.IndexOf('/', num2 + 1);
            string s = ((num3 < 0) ? _url.Substring(num2) : _url.Substring(num2, num3 - num2));
            if (int.TryParse(s, out var result))
            {
                UnturnedLog.info($"Parsed itemdefid {result} from web link url \"{_url}\"");
                ItemStoreSavedata.MarkNewListingSeen(result);
                ItemStore.Get().ViewItem(result);
                return;
            }
        }
        if (WebUtils.ParseThirdPartyUrl(_url, out var result2))
        {
            Provider.openURL(result2);
            return;
        }
        UnturnedLog.warn("Ignoring potentially unsafe web link button url {0}", _url);
    }
}
