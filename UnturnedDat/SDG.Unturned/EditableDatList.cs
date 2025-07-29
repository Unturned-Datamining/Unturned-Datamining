using System.Collections;
using System.Collections.Generic;

namespace SDG.Unturned;

internal sealed class EditableDatList : EditableDatNode<IDatList, DatList, EditableDatList>, IEditableDatList, IDatList, IDatNode, IEnumerable<IDatNode>, IEnumerable, IEditableDatNode
{
    private int nodeCreationCounter;

    public int Count => wrappedNode.Count;

    public IDatNode this[int index] => wrappedNode[index];

    public EditableDatList(IDatList list)
    {
        wrappedNode = list;
    }

    public bool TryGetNode(int index, out IDatNode node)
    {
        return wrappedNode.TryGetNode(index, out node);
    }

    public DatListValueEnumerable GetValues()
    {
        return wrappedNode.GetValues();
    }

    public int IndexOf(IDatNode node)
    {
        return wrappedNode.IndexOf(node);
    }

    public IEditableDatList Edit()
    {
        return this;
    }

    IEnumerator<IDatNode> IEnumerable<IDatNode>.GetEnumerator()
    {
        return wrappedNode.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return wrappedNode.GetEnumerator();
    }

    public IEditableDatValue AddValue()
    {
        EditableDatValue editableDatValue = new EditableDatValue(new DatValue());
        editableDatValue.creationId = ++nodeCreationCounter;
        editableDatValue.parentNode = this;
        GetUnderlyingNode().Add(editableDatValue);
        return editableDatValue;
    }

    public IEditableDatList AddList()
    {
        EditableDatList editableDatList = new EditableDatList(new DatList());
        editableDatList.creationId = ++nodeCreationCounter;
        editableDatList.parentNode = this;
        GetUnderlyingNode().Add(editableDatList);
        return editableDatList;
    }

    public IEditableDatDictionary AddDictionary()
    {
        EditableDatDictionary editableDatDictionary = new EditableDatDictionary(new DatDictionary());
        editableDatDictionary.creationId = ++nodeCreationCounter;
        editableDatDictionary.parentNode = this;
        GetUnderlyingNode().Add(editableDatDictionary);
        return editableDatDictionary;
    }

    public void RemoveAt(int index)
    {
        GetUnderlyingNode().RemoveAt(index);
    }

    public override string WriterGetInlineComment()
    {
        return null;
    }
}
