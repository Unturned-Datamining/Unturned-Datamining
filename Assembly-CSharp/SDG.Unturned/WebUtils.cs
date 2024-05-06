using System;

namespace SDG.Unturned;

internal static class WebUtils
{
    /// <summary>
    /// The game uses Process.Start to open web links when the Steam overlay is unavailable, which could be exploited
    /// to e.g. download and execute files. To prevent this we only allow valid http or https urls.
    /// </summary>
    /// <param name="autoPrefix">If true, prefix with https:// if neither http:// or https:// is specified.</param>
    internal static bool ParseThirdPartyUrl(string uriString, out string result, bool autoPrefix = true)
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
