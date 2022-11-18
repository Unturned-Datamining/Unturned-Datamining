using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

public class WebLink : MonoBehaviour
{
    public Button targetButton;

    public string url;

    private void onClick()
    {
        if (WebUtils.ParseThirdPartyUrl(url, out var result))
        {
            Provider.openURL(result);
            return;
        }
        UnturnedLog.warn("Ignoring potentially unsafe web link component url {0}", url);
    }

    private void Start()
    {
        targetButton.onClick.AddListener(onClick);
    }
}
