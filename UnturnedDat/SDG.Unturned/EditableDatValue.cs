namespace SDG.Unturned;

internal sealed class EditableDatValue : EditableDatNode<IDatValue, DatValue, EditableDatValue>, IEditableDatValue, IDatValue, IDatNode, IEditableDatNode
{
    public string inlineComment;

    public bool hasAssignedInlineComment;

    public string InlineComment
    {
        get
        {
            return inlineComment;
        }
        set
        {
            inlineComment = value;
            hasAssignedInlineComment = true;
        }
    }

    public string Value
    {
        get
        {
            return wrappedNode.Value;
        }
        set
        {
            wrappedNode.Value = value;
        }
    }

    public EditableDatValue(IDatValue node)
    {
        wrappedNode = node;
    }

    public bool TryGetParsedInlineComment(out string comment)
    {
        return wrappedNode.TryGetParsedInlineComment(out comment);
    }

    public IEditableDatValue Edit()
    {
        return this;
    }

    public override string WriterGetInlineComment()
    {
        if (hasAssignedInlineComment)
        {
            return inlineComment;
        }
        wrappedNode.TryGetParsedInlineComment(out var result);
        return result;
    }
}
