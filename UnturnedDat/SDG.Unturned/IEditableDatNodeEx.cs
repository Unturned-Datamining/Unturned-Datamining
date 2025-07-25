using System.Collections.Generic;
using System.Text;

namespace SDG.Unturned;

public static class IEditableDatNodeEx
{
    public static TNode SetComment<TNode>(this TNode node, string comment) where TNode : IEditableDatNode
    {
        node.Comment = comment;
        return node;
    }

    public static TNode SetMargins<TNode>(this TNode node, int margins) where TNode : IEditableDatNode
    {
        node.TopMargin = margins;
        node.BottomMargin = margins;
        return node;
    }

    public static TNode SetMargins<TNode>(this TNode node, int topMargin, int bottomMargin) where TNode : IEditableDatNode
    {
        node.TopMargin = topMargin;
        node.BottomMargin = bottomMargin;
        return node;
    }

    public static TNode SetTopMargin<TNode>(this TNode node, int topMargin) where TNode : IEditableDatNode
    {
        node.TopMargin = topMargin;
        return node;
    }

    public static TNode SetBottomMargin<TNode>(this TNode node, int bottomMargin) where TNode : IEditableDatNode
    {
        node.BottomMargin = bottomMargin;
        return node;
    }

    public static TNode SetSortingPreference<TNode>(this TNode node, IEditableDatNode.ESortingPreference sortingPreference) where TNode : IEditableDatNode
    {
        node.SortingPreference = sortingPreference;
        return node;
    }

    public static TNode MergeGeneratedComment<TNode, TEnumerable>(this TNode node, string prefix, TEnumerable generatedLines, StringBuilder stringBuilder, List<string> parsedLines) where TNode : IEditableDatNode where TEnumerable : IEnumerable<string>
    {
        string value = prefix.TrimEnd();
        parsedLines.Clear();
        int num = 0;
        if (node.TryGetParsedComment(out var comment))
        {
            bool flag = false;
            string[] messageLines = comment.MessageLines;
            foreach (string text in messageLines)
            {
                if (text != null && text.StartsWith(value))
                {
                    if (!flag)
                    {
                        flag = true;
                        num = parsedLines.Count;
                    }
                }
                else
                {
                    parsedLines.Add(text);
                }
            }
        }
        stringBuilder.Clear();
        bool flag2 = true;
        for (int j = 0; j < num; j++)
        {
            if (!flag2)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append(parsedLines[j]);
            flag2 = false;
        }
        foreach (string item in generatedLines)
        {
            if (!flag2)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append(prefix);
            stringBuilder.Append(item);
            flag2 = false;
        }
        for (int k = num; k < parsedLines.Count; k++)
        {
            if (!flag2)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append(parsedLines[k]);
            flag2 = false;
        }
        return node.SetComment(stringBuilder.ToString());
    }

    public static TNode MergeGeneratedCommentAlloc<TNode, TEnumerable>(this TNode node, string prefix, TEnumerable generatedLines) where TNode : IEditableDatNode where TEnumerable : IEnumerable<string>
    {
        StringBuilder stringBuilder = new StringBuilder();
        List<string> parsedLines = new List<string>();
        return node.MergeGeneratedComment(prefix, generatedLines, stringBuilder, parsedLines);
    }
}
