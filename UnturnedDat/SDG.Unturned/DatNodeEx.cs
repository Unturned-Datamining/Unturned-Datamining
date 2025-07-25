using System.Collections.Generic;
using System.Text;

namespace SDG.Unturned;

public static class DatNodeEx
{
    public static string DebugDumpToString(this IDatNode node)
    {
        StringBuilder stringBuilder = new StringBuilder();
        node.DebugDumpToStringBuilder(stringBuilder);
        return stringBuilder.ToString();
    }

    public static bool TryParseStruct<T>(this IDatNode node, out T value) where T : struct, IDatParseable
    {
        value = default(T);
        return value.TryParse(node);
    }

    public static T ParseStruct<T>(this IDatNode node, T defaultValue = default(T)) where T : struct, IDatParseable
    {
        if (!node.TryParseStruct<T>(out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public static bool TryGetNodePath(this IDatNode node, out string path)
    {
        if (!node.TryGetParentNode(out var parentNode))
        {
            path = null;
            return false;
        }
        if (parentNode == null)
        {
            path = string.Empty;
            return true;
        }
        if (!parentNode.TryGetNodePath(out var path2))
        {
            path = null;
            return false;
        }
        if (parentNode is IDatDictionary datDictionary)
        {
            string text = null;
            foreach (KeyValuePair<string, IDatNode> item in datDictionary)
            {
                if (item.Value == node)
                {
                    text = item.Key;
                    break;
                }
            }
            if (text == null)
            {
                path = null;
                return false;
            }
            path = path2 + "/" + text;
            return true;
        }
        if (parentNode is IDatList datList)
        {
            int num = datList.IndexOf(node);
            if (num < 0)
            {
                path = null;
                return false;
            }
            path = $"{path2}/{num}";
            return true;
        }
        path = null;
        return false;
    }

    public static string GetPath(this IDatNode node)
    {
        if (!node.TryGetNodePath(out var path))
        {
            return null;
        }
        return path;
    }

    public static int GetParsedLineNumber(this IDatNode node)
    {
        if (!node.TryGetParsedLineNumber(out var lineNumber))
        {
            return -1;
        }
        return lineNumber;
    }
}
