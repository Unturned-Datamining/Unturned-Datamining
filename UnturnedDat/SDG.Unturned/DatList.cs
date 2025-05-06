using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SDG.Unturned;

public sealed class DatList : List<IDatNode>, IDatList, IDatNode, IEnumerable<IDatNode>, IEnumerable
{
    public EDatNodeType NodeType => EDatNodeType.List;

    public bool IsMetadataAvailable => false;

    public bool TryGetNode(int index, out IDatNode node)
    {
        node = ((index >= 0 && index < base.Count) ? base[index] : null);
        return node != null;
    }

    public DatListValueEnumerable GetValues()
    {
        return new DatListValueEnumerable(this);
    }

    public void DebugDumpToStringBuilder(StringBuilder output, int indentationLevel = 0)
    {
        output.Append('[');
        if (TryGetParsedLineNumberRange(out var startingLineNumber, out var endingLineNumber))
        {
            output.Append(" (lines ");
            output.Append(startingLineNumber);
            output.Append('-');
            output.Append(endingLineNumber);
            output.Append(')');
        }
        output.AppendLine();
        for (int i = 0; i < base.Count; i++)
        {
            IDatNode datNode = base[i];
            if (datNode.TryGetParsedComment(out var comment))
            {
                comment.DebugDumpToStringBuilder(output, indentationLevel);
            }
            for (int j = 0; j < indentationLevel + 1; j++)
            {
                output.Append('\t');
            }
            output.Append(i);
            output.Append(" = ");
            if (datNode != null)
            {
                datNode.DebugDumpToStringBuilder(output, indentationLevel + 1);
            }
            else
            {
                output.AppendLine("null");
            }
        }
        for (int k = 0; k < indentationLevel; k++)
        {
            output.Append('\t');
        }
        output.AppendLine("]");
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

    public IEditableDatList Edit()
    {
        return null;
    }
}
