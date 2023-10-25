using System;

namespace SDG.Unturned;

public class NameTool
{
    public static bool checkNames(string input, string name)
    {
        if (input.Length <= name.Length)
        {
            return name.ToLower().IndexOf(input.ToLower()) != -1;
        }
        return false;
    }

    /// <summary>
    /// If updating this method please remember to update the support article:
    /// https://support.smartlydressedgames.com/hc/en-us/articles/13452208765716
    /// </summary>
    public static bool isValid(string name)
    {
        foreach (char c in name)
        {
            if (c <= '\u001f')
            {
                return false;
            }
            if (c >= '~')
            {
                return false;
            }
            if (c == '/' || c == '\\' || c == '`' || c == '\'' || c == '"')
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Does name contain rich text tags?
    /// Some players were abusing rich text enabled servers by inserting admin colors into their steam name.
    /// </summary>
    public static bool containsRichText(string name)
    {
        if (name.IndexOf("<color", StringComparison.OrdinalIgnoreCase) != -1)
        {
            return true;
        }
        if (name.IndexOf("<b>", StringComparison.OrdinalIgnoreCase) != -1)
        {
            return true;
        }
        if (name.IndexOf("<i>", StringComparison.OrdinalIgnoreCase) != -1)
        {
            return true;
        }
        if (name.IndexOf("<size", StringComparison.OrdinalIgnoreCase) != -1)
        {
            return true;
        }
        if (name.IndexOf("<voffset", StringComparison.OrdinalIgnoreCase) != -1)
        {
            return true;
        }
        if (name.IndexOf("<sprite", StringComparison.OrdinalIgnoreCase) != -1)
        {
            return true;
        }
        return false;
    }
}
