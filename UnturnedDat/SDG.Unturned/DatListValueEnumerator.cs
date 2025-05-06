using System;
using System.Collections;
using System.Collections.Generic;

namespace SDG.Unturned;

public struct DatListValueEnumerator : IEnumerator<IDatValue>, IEnumerator, IDisposable
{
    private IDatList list;

    private int index;

    private IDatValue current;

    public IDatValue Current => current;

    object IEnumerator.Current => current;

    public DatListValueEnumerator(IDatList list)
    {
        this.list = list;
        index = -1;
        current = null;
    }

    public bool MoveNext()
    {
        while (++index < list.Count)
        {
            current = list[index] as IDatValue;
            if (current != null)
            {
                return true;
            }
        }
        return false;
    }

    public void Reset()
    {
        index = -1;
        current = null;
    }

    public void Dispose()
    {
    }
}
