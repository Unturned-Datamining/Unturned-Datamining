using System.Collections;
using System.Collections.Generic;

namespace SDG.Unturned;

internal sealed class DatListWithMetadata : DatNodeWithMetadata<DatList, EditableDatList>, IDatList, IDatNode, IEnumerable<IDatNode>, IEnumerable
{
    public int openingLineNumber;

    public int closingLineNumber;

    public int Count => underlyingNode.Count;

    public IDatNode this[int index]
    {
        get
        {
            return underlyingNode[index];
        }
        set
        {
            underlyingNode[index] = value;
        }
    }

    public DatListWithMetadata(DatList node)
        : base(node)
    {
    }

    public bool TryGetParsedLineNumber(out int lineNumber)
    {
        lineNumber = openingLineNumber;
        return true;
    }

    public bool TryGetParsedLineNumberRange(out int startingLineNumber, out int endingLineNumber)
    {
        startingLineNumber = openingLineNumber;
        endingLineNumber = closingLineNumber;
        return true;
    }

    public bool TryGetNode(int index, out IDatNode node)
    {
        return underlyingNode.TryGetNode(index, out node);
    }

    public DatListValueEnumerable GetValues()
    {
        return underlyingNode.GetValues();
    }

    public int IndexOf(IDatNode node)
    {
        return underlyingNode.IndexOf(node);
    }

    IEnumerator<IDatNode> IEnumerable<IDatNode>.GetEnumerator()
    {
        return underlyingNode.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return underlyingNode.GetEnumerator();
    }

    public IEditableDatList Edit()
    {
        if (editable == null)
        {
            editable = new EditableDatList(this);
        }
        return editable;
    }

    public override string WriterGetInlineComment()
    {
        return null;
    }
}
