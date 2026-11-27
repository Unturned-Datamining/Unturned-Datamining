using System;
using System.Globalization;
using System.Text;

namespace Unturned.SystemEx;

public static class StringExtension
{
    private static readonly char[] SplitNewLineSeparators = new char[2] { '\r', '\n' };

    private static string[] SplitNewLineSeparatorStrings = new string[2] { "\r\n", "\n" };

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

    public static string[] SplitLinesIncludingEmpty(this string s)
    {
        return s.Split(SplitNewLineSeparatorStrings, StringSplitOptions.None);
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

    public static string CanonicalizeId(this string s)
    {
        try
        {
            s = s.Normalize(NormalizationForm.FormKC);
            s = s.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                int num = char.ConvertToUtf32(s, i);
                if (char.IsHighSurrogate(s[i]))
                {
                    i++;
                }
                if (StringConfuseables.CONFUSEABLES.TryGetValue(num, out var value))
                {
                    int[] array = value;
                    foreach (int utf in array)
                    {
                        stringBuilder.Append(char.ConvertFromUtf32(utf));
                    }
                }
                else
                {
                    stringBuilder.Append(char.ConvertFromUtf32(num));
                }
            }
            s = stringBuilder.ToString();
            s = s.Normalize(NormalizationForm.FormD);
            stringBuilder.Clear();
            for (int k = 0; k < s.Length; k++)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(s, k);
                int utf2 = char.ConvertToUtf32(s, k);
                if (char.IsHighSurrogate(s[k]))
                {
                    k++;
                }
                if (unicodeCategory == UnicodeCategory.UppercaseLetter || unicodeCategory == UnicodeCategory.LowercaseLetter || unicodeCategory == UnicodeCategory.TitlecaseLetter || unicodeCategory == UnicodeCategory.OtherLetter || unicodeCategory == UnicodeCategory.DecimalDigitNumber)
                {
                    stringBuilder.Append(char.ConvertFromUtf32(utf2));
                }
            }
            s = stringBuilder.ToString();
            s = s.Normalize(NormalizationForm.FormC);
            return s;
        }
        catch
        {
            return string.Empty;
        }
    }
}
