using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SDG.Unturned;

public sealed class DatDictionary : Dictionary<string, IDatNode>, IDatDictionary, IDatNode, IEnumerable<KeyValuePair<string, IDatNode>>, IEnumerable
{
    public EDatNodeType NodeType => EDatNodeType.Dictionary;

    public bool IsMetadataAvailable => false;

    public DatDictionary()
        : base((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase)
    {
    }

    public bool TryGetNode(string key, out IDatNode node)
    {
        return TryGetValue(key, out node);
    }

    public void DebugDumpToStringBuilder(StringBuilder output, int indentationLevel = 0)
    {
        output.Append('{');
        if (TryGetParsedLineNumberRange(out var startingLineNumber, out var endingLineNumber))
        {
            output.Append(" (lines ");
            output.Append(startingLineNumber);
            output.Append('-');
            output.Append(endingLineNumber);
            output.Append(')');
        }
        output.AppendLine();
        using (Enumerator enumerator = GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                KeyValuePair<string, IDatNode> current = enumerator.Current;
                if (current.Value.TryGetParsedComment(out var comment))
                {
                    comment.DebugDumpToStringBuilder(output, indentationLevel);
                }
                for (int i = 0; i < indentationLevel + 1; i++)
                {
                    output.Append('\t');
                }
                output.Append("\"" + current.Key + "\"");
                output.Append(" = ");
                if (current.Value != null)
                {
                    current.Value.DebugDumpToStringBuilder(output, indentationLevel + 1);
                }
                else
                {
                    output.AppendLine("null");
                }
            }
        }
        for (int j = 0; j < indentationLevel; j++)
        {
            output.Append('\t');
        }
        output.AppendLine("}");
    }

    public bool TryGetParsedComment(out DatComment comment)
    {
        comment = default(DatComment);
        return false;
    }

    public bool TryGetParentNode(out IDatNode parentNode)
    {
        parentNode = null;
        return false;
    }

    public bool TryGetParsedLineNumber(out int lineNumber)
    {
        lineNumber = 0;
        return false;
    }

    public bool TryGetParsedLineNumberRange(out int startingLineNumber, out int endingLineNumber)
    {
        startingLineNumber = 0;
        endingLineNumber = 0;
        return false;
    }

    public bool TryGetKeyLineNumber(string key, out int lineNumber)
    {
        lineNumber = 0;
        return false;
    }

    public IEditableDatDictionary Edit()
    {
        return null;
    }
}
