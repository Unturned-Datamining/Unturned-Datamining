using System.Text.RegularExpressions;

namespace SDG.HostBans;

public struct HostBanRegexFilter
{
    public Regex regex;

    public EHostBanFlags flags;

    public override string ToString()
    {
        return regex.ToString();
    }
}
