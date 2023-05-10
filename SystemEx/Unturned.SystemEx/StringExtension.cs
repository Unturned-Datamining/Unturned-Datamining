using System;

namespace Unturned.SystemEx;

public static class StringExtension
{
    private static readonly char[] SplitNewLineSeparators = new char[2] { '\r', '\n' };

    public static bool ContainsChar(this string s, char value)
    {
        if (string.IsNullOrEmpty(s))
        {
            return false;
        }
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == value)
            {
                return true;
            }
        }
        return false;
    }

    public static string[] SplitLines(this string s)
    {
        return s.Split(SplitNewLineSeparators, StringSplitOptions.RemoveEmptyEntries);
    }

    public static bool ContainsNewLine(this string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return false;
        }
        foreach (char c in s)
        {
            if (c == '\n' || c == '\r')
            {
                return true;
            }
        }
        return false;
    }

    public static int CountChar(this string s, char value)
    {
        if (string.IsNullOrEmpty(s))
        {
            return 0;
        }
        int num = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == value)
            {
                num++;
            }
        }
        return num;
    }

    public static int CountNewlines(this string s)
    {
        return s.CountChar('\n');
    }

    public static bool Contains(this string s, string value, StringComparison comparisonType)
    {
        return s.IndexOf(value, comparisonType) >= 0;
    }
}
