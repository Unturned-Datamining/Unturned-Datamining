using System.Text;

namespace SDG.Unturned;

internal abstract class EditableDatNode<TInterface, TNode, TEditable> : EditableDatNodeBase, IMetadataPreservingDatWriterCompatible where TInterface : IDatNode where TNode : TInterface where TEditable : EditableDatNodeBase
{
    public TInterface wrappedNode;

    public int PreferredLineNumber { get; set; }

    public EDatNodeType NodeType => wrappedNode.NodeType;

    public bool IsMetadataAvailable => wrappedNode.IsMetadataAvailable;

    public TNode GetUnderlyingNode()
    {
        if (wrappedNode is DatNodeWithMetadata<TNode, TEditable> datNodeWithMetadata)
        {
            return datNodeWithMetadata.underlyingNode;
        }
        return (TNode)(object)wrappedNode;
    }

    public void DebugDumpToStringBuilder(StringBuilder output, int indentationLevel = 0)
    {
        wrappedNode.DebugDumpToStringBuilder(output, indentationLevel);
    }

    public bool TryGetParentNode(out IDatNode parentNode)
    {
        if (wrappedNode.IsMetadataAvailable)
        {
            return wrappedNode.TryGetParentNode(out parentNode);
        }
        parentNode = base.parentNode;
        return true;
    }

    public bool TryGetParsedComment(out DatComment comment)
    {
        return wrappedNode.TryGetParsedComment(out comment);
    }

    public bool TryGetParsedLineNumber(out int lineNumber)
    {
        return wrappedNode.TryGetParsedLineNumber(out lineNumber);
    }

    public bool TryGetParsedLineNumberRange(out int startingLineNumber, out int endingLineNumber)
    {
        return wrappedNode.TryGetParsedLineNumberRange(out startingLineNumber, out endingLineNumber);
    }

    public DatComment? WriterGetPrefixComment()
    {
        if (hasAssignedComment)
        {
            return new DatComment(comment);
        }
        if (!wrappedNode.TryGetParsedComment(out var value))
        {
            return null;
        }
        return value;
    }

    public abstract string WriterGetInlineComment();

    public int WriterGetEarliestLineNumber()
    {
        if (PreferredLineNumber > 0)
        {
            return PreferredLineNumber;
        }
        wrappedNode.TryGetParsedLineNumber(out var lineNumber);
        if (NodeType != 0 && TryGetParentNode(out var datNode) && datNode != null && datNode.NodeType == EDatNodeType.Dictionary)
        {
            return lineNumber - 1;
        }
        return lineNumber;
    }

    public int WriterGetLatestLineNumber()
    {
        wrappedNode.TryGetParsedLineNumberRange(out var _, out var endingLineNumber);
        return endingLineNumber;
    }

    public void WriterGetSortingParameters(out int lineNumber, out int sortOrder)
    {
        lineNumber = WriterGetEarliestLineNumber();
        sortOrder = creationId;
    }

    public void WriterGetMargins(out int topMargin, out int bottomMargin)
    {
        topMargin = base.TopMargin;
        bottomMargin = base.BottomMargin;
    }
}
