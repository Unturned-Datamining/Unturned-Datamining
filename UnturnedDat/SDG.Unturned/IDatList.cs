using System.Collections;
using System.Collections.Generic;

namespace SDG.Unturned;

public interface IDatList : IDatNode, IEnumerable<IDatNode>, IEnumerable
{
    int Count { get; }

    IDatNode this[int index] { get; }

    bool TryGetNode(int index, out IDatNode node);

    DatListValueEnumerable GetValues();

    int IndexOf(IDatNode node);

    IEditableDatList Edit();
}
