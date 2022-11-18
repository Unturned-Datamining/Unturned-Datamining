using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public struct TokenSearchFilter
{
    private string[] tokens;

    private static List<string> workingTokens = new List<string>();

    public static TokenSearchFilter? parse(string filter)
    {
        if (string.IsNullOrEmpty(filter))
        {
            return null;
        }
        workingTokens.Clear();
        int num = 0;
        while (num < filter.Length)
        {
            int num2 = filter.IndexOf(' ', num);
            int num3 = filter.IndexOf(':', num);
            if (num2 < 0)
            {
                num2 = filter.Length;
            }
            if ((num3 < 0 || num3 >= num2) && num2 - num > 0)
            {
                string item = filter.Substring(num, num2 - num);
                workingTokens.Add(item);
            }
            num = num2 + 1;
        }
        if (workingTokens.Count < 1)
        {
            return null;
        }
        return new TokenSearchFilter(workingTokens.ToArray());
    }

    public bool ignores(string name)
    {
        string[] array = tokens;
        foreach (string value in array)
        {
            if (name.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                return true;
            }
        }
        return false;
    }

    public bool matches(string name)
    {
        return !ignores(name);
    }

    public TokenSearchFilter(string[] tokens)
    {
        this.tokens = tokens;
    }
}
