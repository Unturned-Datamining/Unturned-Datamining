using System.Text;

namespace SDG.Unturned;

internal abstract class DatNodeWithMetadata<TNode, TEditable> : DatNodeWithMetadataBase, IMetadataPreservingDatWriterCompatible where TNode : IDatNode where TEditable : EditableDatNodeBase
{
    public TNode underlyingNode;

    public TEditable editable;

    public DatComment? prefixComment;

    public EDatNodeType NodeType => underlyingNode.NodeType;

    public bool IsMetadataAvailable => true;

    public DatNodeWithMetadata(TNode underlyingNode)
    {
        this.underlyingNode = underlyingNode;
    }

    public void DebugDumpToStringBuilder(StringBuilder output, int indentationLevel = 0)
    {
        underlyingNode.DebugDumpToStringBuilder(output, indentationLevel);
    }

    public bool TryGetParentNode(out IDatNode parentNode)
    {
        parentNode = base.parentNode;
        return true;
    }

    public bool TryGetParsedComment(out DatComment comment)
    {
        if (prefixComment.HasValue)
        {
            comment = prefixComment.Value;
            return true;
        }
        comment = default(DatComment);
        return false;
    }

    public DatComment? WriterGetPrefixComment()
    {
        if (editable != null && editable.hasAssignedComment)
        {
            return new DatComment(editable.Comment);
        }
        return prefixComment;
    }

    public abstract string WriterGetInlineComment();

    public int WriterGetEarliestLineNumber()
    {
        if (prefixComment.HasValue && prefixComment.Value.StartingLineNumber > 0)
        {
            return prefixComment.Value.StartingLineNumber;
        }
        ((IDatNode)this).TryGetParsedLineNumber(out var lineNumber);
        if (NodeType != 0 && parentNode != null && parentNode.NodeType == EDatNodeType.Dictionary)
        {
            return lineNumber - 1;
        }
        return lineNumber;
    }

    public int WriterGetLatestLineNumber()
    {
        ((IDatNode)this).TryGetParsedLineNumberRange(out var _, out var endingLineNumber);
        return endingLineNumber;
    }

    public void WriterGetSortingParameters(out int lineNumber, out int sortOrder)
    {
        lineNumber = WriterGetEarliestLineNumber();
        sortOrder = 0;
    }

    public void WriterGetMargins(out int topMargin, out int bottomMargin)
    {
        if (editable != null)
        {
            topMargin = editable.TopMargin;
            bottomMargin = editable.BottomMargin;
        }
        else
        {
            topMargin = 0;
            bottomMargin = 0;
        }
    }
}
