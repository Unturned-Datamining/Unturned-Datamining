using System;

namespace SDG.Unturned;

public static class SearchFilterUtil
{
    public static bool parseKeyValue(string filter, string key, out string value)
    {
        value = null;
        if (string.IsNullOrEmpty(filter))
        {
            return false;
        }
        int num = filter.IndexOf(key, StringComparison.InvariantCultureIgnoreCase);
        if (num < 0)
        {
            return false;
        }
        int num2 = num + key.Length;
        int num3 = filter.IndexOf(' ', num2);
        if (num3 < 0)
        {
            value = filter.Substring(num2);
        }
        else
        {
            value = filter.Substring(num2, num3 - num2);
        }
        return !string.IsNullOrEmpty(value);
    }
}
