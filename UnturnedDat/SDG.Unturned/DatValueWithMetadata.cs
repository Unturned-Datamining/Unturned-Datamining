namespace SDG.Unturned;

internal sealed class DatValueWithMetadata : DatNodeWithMetadata<DatValue, EditableDatValue>, IDatValue, IDatNode
{
    public int lineNumber;

    public string inlineComment;

    public string Value
    {
        get
        {
            return underlyingNode.value;
        }
        set
        {
            underlyingNode.value = value;
        }
    }

    public bool TryGetParsedInlineComment(out string inlineComment)
    {
        inlineComment = this.inlineComment;
        return true;
    }

    public bool TryGetParsedLineNumber(out int lineNumber)
    {
        lineNumber = this.lineNumber;
        return true;
    }

    public bool TryGetParsedLineNumberRange(out int startingLineNumber, out int endingLineNumber)
    {
        startingLineNumber = lineNumber;
        endingLineNumber = lineNumber;
        return true;
    }

    public DatValueWithMetadata(DatValue valueNode, int lineNumber, string inlineComment, DatComment? prefixComment)
        : base(valueNode)
    {
        this.lineNumber = lineNumber;
        this.inlineComment = inlineComment;
        base.prefixComment = prefixComment;
    }

    public IEditableDatValue Edit()
    {
        if (editable == null)
        {
            editable = new EditableDatValue(this);
        }
        return editable;
    }

    public override string WriterGetInlineComment()
    {
        if (editable != null && editable.hasAssignedInlineComment)
        {
            return editable.inlineComment;
        }
        return inlineComment;
    }
}
