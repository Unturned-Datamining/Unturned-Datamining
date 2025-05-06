using System.Collections;
using System.Collections.Generic;

namespace SDG.Unturned;

internal sealed class DatDictionaryWithMetadata : DatNodeWithMetadata<DatDictionary, EditableDatDictionary>, IDatDictionary, IDatNode, IEnumerable<KeyValuePair<string, IDatNode>>, IEnumerable
{
    public int openingLineNumber;

    public int closingLineNumber;

    public int Count => underlyingNode.Count;

    public IDatNode this[string index]
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

    public DatDictionaryWithMetadata(DatDictionary dictionary)
        : base(dictionary)
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

    public bool ContainsKey(string key)
    {
        return underlyingNode.ContainsKey(key);
    }

    public bool TryGetNode(string key, out IDatNode node)
    {
        return underlyingNode.TryGetNode(key, out node);
    }

    public bool TryGetKeyLineNumber(string key, out int lineNumber)
    {
        return underlyingNode.TryGetKeyLineNumber(key, out lineNumber);
    }

    public IEditableDatDictionary Edit()
    {
        if (editable == null)
        {
            editable = new EditableDatDictionary(this);
        }
        return editable;
    }

    IEnumerator<KeyValuePair<string, IDatNode>> IEnumerable<KeyValuePair<string, IDatNode>>.GetEnumerator()
    {
        return underlyingNode.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return underlyingNode.GetEnumerator();
    }

    public override string WriterGetInlineComment()
    {
        return null;
    }
}
