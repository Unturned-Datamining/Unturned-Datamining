using System.Collections;
using System.Collections.Generic;

namespace SDG.Unturned;

internal sealed class EditableDatDictionary : EditableDatNode<IDatDictionary, DatDictionary, EditableDatDictionary>, IEditableDatDictionary, IDatDictionary, IDatNode, IEnumerable<KeyValuePair<string, IDatNode>>, IEnumerable, IEditableDatNode
{
    private int nodeCreationCounter;

    public int Count => wrappedNode.Count;

    public IDatNode this[string index] => wrappedNode[index];

    public EditableDatDictionary(IDatDictionary node)
    {
        wrappedNode = node;
    }

    public bool ContainsKey(string key)
    {
        return wrappedNode.ContainsKey(key);
    }

    public bool TryGetNode(string key, out IDatNode node)
    {
        return wrappedNode.TryGetNode(key, out node);
    }

    public bool TryGetKeyLineNumber(string key, out int lineNumber)
    {
        return wrappedNode.TryGetKeyLineNumber(key, out lineNumber);
    }

    public IEditableDatDictionary Edit()
    {
        return this;
    }

    IEnumerator<KeyValuePair<string, IDatNode>> IEnumerable<KeyValuePair<string, IDatNode>>.GetEnumerator()
    {
        return wrappedNode.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return wrappedNode.GetEnumerator();
    }

    public IEditableDatValue AddValue(string key)
    {
        EditableDatValue editableDatValue = new EditableDatValue(new DatValue());
        editableDatValue.creationId = ++nodeCreationCounter;
        editableDatValue.parentNode = this;
        GetUnderlyingNode().Add(key, editableDatValue);
        return editableDatValue;
    }

    public IEditableDatList AddList(string key)
    {
        EditableDatList editableDatList = new EditableDatList(new DatList());
        editableDatList.creationId = ++nodeCreationCounter;
        editableDatList.parentNode = this;
        GetUnderlyingNode().Add(key, editableDatList);
        return editableDatList;
    }

    public IEditableDatDictionary AddDictionary(string key)
    {
        EditableDatDictionary editableDatDictionary = new EditableDatDictionary(new DatDictionary());
        editableDatDictionary.creationId = ++nodeCreationCounter;
        editableDatDictionary.parentNode = this;
        GetUnderlyingNode().Add(key, editableDatDictionary);
        return editableDatDictionary;
    }

    public IEditableDatValue ReplaceWithValue(string key)
    {
        IDatNode node;
        bool num = Remove(key, out node);
        IEditableDatValue editableDatValue = AddValue(key);
        if (num && node != null && node.TryGetParsedLineNumber(out var lineNumber))
        {
            editableDatValue.PreferredLineNumber = lineNumber;
        }
        return editableDatValue;
    }

    public IEditableDatList ReplaceWithList(string key)
    {
        IDatNode node;
        bool num = Remove(key, out node);
        IEditableDatList editableDatList = AddList(key);
        if (num && node != null && node.TryGetParsedLineNumber(out var lineNumber))
        {
            editableDatList.PreferredLineNumber = lineNumber;
        }
        return editableDatList;
    }

    public IEditableDatDictionary ReplaceWithDictionary(string key)
    {
        IDatNode node;
        bool num = Remove(key, out node);
        IEditableDatDictionary editableDatDictionary = AddDictionary(key);
        if (num && node != null && node.TryGetParsedLineNumber(out var lineNumber))
        {
            editableDatDictionary.PreferredLineNumber = lineNumber;
        }
        return editableDatDictionary;
    }

    public bool Remove(string key)
    {
        return GetUnderlyingNode().Remove(key);
    }

    public bool Remove(string key, out IDatNode node)
    {
        return GetUnderlyingNode().Remove(key, out node);
    }

    public override string WriterGetInlineComment()
    {
        return null;
    }
}
