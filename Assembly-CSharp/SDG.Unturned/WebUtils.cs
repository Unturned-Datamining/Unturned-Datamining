using System;
using UnityEngine.Networking;

namespace SDG.Unturned;

internal static class WebUtils
{
    /// <summary>
    /// The game uses Process.Start to open web links when the Steam overlay is unavailable, which could be exploited
    /// to e.g. download and execute files. To prevent this we only allow valid http or https urls.
    /// </summary>
    /// <param name="autoPrefix">If true, prefix with https:// if neither http:// or https:// is specified.</param>
    internal static bool ParseThirdPartyUrl(string uriString, out string result, bool autoPrefix = true, bool useLinkFiltering = true)
    {
        if (string.IsNullOrEmpty(uriString))
        {
            result = null;
            return false;
        }
        uriString = uriString.Trim();
        if (autoPrefix && !uriString.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !uriString.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            uriString = "https://" + uriString;
        }
        if (Uri.TryCreate(uriString, UriKind.Absolute, out var result2))
        {
            if (result2.Scheme == Uri.UriSchemeHttp || result2.Scheme == Uri.UriSchemeHttps)
            {
                if (useLinkFiltering)
                {
                    switch (LiveConfig.Get().linkFiltering.Match(result2.Host, result2.AbsolutePath))
                    {
                    case ELinkFilteringAction.Deny:
                        result = null;
                        return false;
                    case ELinkFilteringAction.UseSteamLinkFilter:
                    {
                        string text = UnityWebRequest.EscapeURL(result2.AbsoluteUri);
                        result = "https://steamcommunity.com/linkfilter/?u=" + text;
                        return true;
                    }
                    }
                }
                result = result2.AbsoluteUri;
                return true;
            }
            result = null;
            return false;
        }
        result = null;
        return false;
    }

    /// <summary>
    /// This version just doesn't return the parsed URL.
    /// </summary>
    internal static bool CanParseThirdPartyUrl(string uriString, bool autoPrefix = true, bool useLinkFiltering = true)
    {
        string result;
        return ParseThirdPartyUrl(uriString, out result, autoPrefix, useLinkFiltering);
    }
}
