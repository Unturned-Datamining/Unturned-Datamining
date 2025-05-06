using System.Text;

namespace SDG.Unturned;

public sealed class DatValue : IDatValue, IDatNode
{
    public string value;

    public static readonly char[] INVALID_TYPE_CHARS = new char[3] { '\\', ':', '/' };

    public EDatNodeType NodeType => EDatNodeType.Value;

    public string Value
    {
        get
        {
            return value;
        }
        set
        {
            this.value = value;
        }
    }

    public bool IsMetadataAvailable => false;

    public DatValue()
    {
        value = null;
    }

    public DatValue(string value)
    {
        this.value = value;
    }

    public void DebugDumpToStringBuilder(StringBuilder output, int indentationLevel = 0)
    {
        if (Value != null)
        {
            output.Append('"');
            output.Append(Value);
            output.Append('"');
        }
        else
        {
            output.Append("value(null)");
        }
        TryGetParsedLineNumber(out var lineNumber);
        if (TryGetParsedInlineComment(out var comment) && !string.IsNullOrEmpty(comment))
        {
            output.Append(" // ");
            output.Append(comment);
        }
        if (lineNumber > 0)
        {
            output.Append(" (line ");
            output.Append(lineNumber);
            output.Append(')');
        }
        output.AppendLine();
    }

    public bool TryGetParsedComment(out DatComment comment)
    {
        comment = default(DatComment);
        return false;
    }

    public bool TryGetParsedInlineComment(out string comment)
    {
        comment = null;
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

    public IEditableDatValue Edit()
    {
        return null;
    }
}
