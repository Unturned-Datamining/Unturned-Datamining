using System.Collections;
using System.Collections.Generic;

namespace SDG.Unturned;

public struct DatListValueEnumerable : IEnumerable<IDatValue>, IEnumerable
{
    private IDatList list;

    public DatListValueEnumerable(IDatList list)
    {
        this.list = list;
    }

    public DatListValueEnumerator GetEnumerator()
    {
        return new DatListValueEnumerator(list);
    }

    IEnumerator<IDatValue> IEnumerable<IDatValue>.GetEnumerator()
    {
        return new DatListValueEnumerator(list);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new DatListValueEnumerator(list);
    }
}
