using System.Collections;
using System.Collections.Generic;

namespace SDG.Unturned;

public interface IEditableDatList : IDatList, IDatNode, IEnumerable<IDatNode>, IEnumerable, IEditableDatNode
{
    IEditableDatValue AddValue();

    IEditableDatList AddList();

    IEditableDatDictionary AddDictionary();
}
