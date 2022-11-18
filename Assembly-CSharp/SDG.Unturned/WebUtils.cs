using System;

namespace SDG.Unturned;

internal static class WebUtils
{
    internal static bool ParseThirdPartyUrl(string uriString, out string result)
    {
        if (string.IsNullOrEmpty(uriString))
        {
            result = null;
            return false;
        }
        if (!uriString.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !uriString.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            uriString = "https://" + uriString;
        }
        if (Uri.TryCreate(uriString, UriKind.Absolute, out var result2))
        {
            if (result2.Scheme == Uri.UriSchemeHttp || result2.Scheme == Uri.UriSchemeHttps)
            {
                result = result2.AbsoluteUri;
                return true;
            }
            result = null;
            return false;
        }
        result = null;
        return false;
    }
}
