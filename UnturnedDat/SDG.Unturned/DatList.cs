using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SDG.Unturned;

public sealed class DatList : List<IDatNode>, IDatNode
{
    public struct ValueEnumerator : IEnumerator<DatValue>, IEnumerator, IDisposable
    {
        private DatList list;

        private int index;

        private DatValue current;

        public DatValue Current => current;

        object IEnumerator.Current => current;

        public ValueEnumerator(DatList list)
        {
            this.list = list;
            index = -1;
            current = null;
        }

        public bool MoveNext()
        {
            while (++index < list.Count)
            {
                current = list[index] as DatValue;
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

    public struct ValueEnumerable : IEnumerable<DatValue>, IEnumerable
    {
        private DatList list;

        public ValueEnumerable(DatList list)
        {
            this.list = list;
        }

        public ValueEnumerator GetEnumerator()
        {
            return new ValueEnumerator(list);
        }

        IEnumerator<DatValue> IEnumerable<DatValue>.GetEnumerator()
        {
            return new ValueEnumerator(list);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ValueEnumerator(list);
        }
    }

    public EDatNodeType NodeType => EDatNodeType.List;

    public bool TryGetValue(int index, out DatValue value)
    {
        IDatNode datNode = ((index >= 0 && index < base.Count) ? base[index] : null);
        value = datNode as DatValue;
        return datNode != null;
    }

    public bool TryGetDictionary(int index, out DatDictionary dictionary)
    {
        IDatNode datNode = ((index >= 0 && index < base.Count) ? base[index] : null);
        dictionary = datNode as DatDictionary;
        return dictionary != null;
    }

    public DatDictionary GetDictionary(int index)
    {
        if (!TryGetDictionary(index, out var dictionary))
        {
            return null;
        }
        return dictionary;
    }

    public bool TryGetList(int index, out DatList list)
    {
        IDatNode datNode = ((index >= 0 && index < base.Count) ? base[index] : null);
        list = datNode as DatList;
        return list != null;
    }

    public DatList GetList(int index)
    {
        if (!TryGetList(index, out var list))
        {
            return null;
        }
        return list;
    }

    public bool TryGetString(int index, out string value)
    {
        if (TryGetValue(index, out var value2))
        {
            value = value2.value;
            return true;
        }
        value = null;
        return false;
    }

    public string GetString(int index, string defaultValue = null)
    {
        if (!TryGetString(index, out var value))
        {
            return defaultValue;
        }
        return value;
    }

    public List<T> ParseListOfStructs<T>() where T : struct, IDatParseable
    {
        List<T> list = new List<T>(base.Count);
        using Enumerator enumerator = GetEnumerator();
        while (enumerator.MoveNext())
        {
            IDatNode current = enumerator.Current;
            if (current != null && current.TryParseStruct<T>(out var value))
            {
                list.Add(value);
            }
        }
        return list;
    }

    public T[] ParseArrayOfStructs<T>(T defaultValue = default(T)) where T : struct, IDatParseable
    {
        T[] array = new T[base.Count];
        for (int i = 0; i < base.Count; i++)
        {
            IDatNode datNode = base[i];
            if (datNode != null && datNode.TryParseStruct<T>(out var value))
            {
                array[i] = value;
            }
            else
            {
                array[i] = defaultValue;
            }
        }
        return array;
    }

    public ValueEnumerable GetValues()
    {
        return new ValueEnumerable(this);
    }

    public void DebugDumpToStringBuilder(StringBuilder output, int indentationLevel = 0)
    {
        output.AppendLine("[");
        for (int i = 0; i < base.Count; i++)
        {
            for (int j = 0; j < indentationLevel + 1; j++)
            {
                output.Append('\t');
            }
            output.Append(i);
            output.Append(" = ");
            IDatNode datNode = base[i];
            if (datNode != null)
            {
                datNode.DebugDumpToStringBuilder(output, indentationLevel + 1);
            }
            else
            {
                output.AppendLine("null");
            }
        }
        for (int k = 0; k < indentationLevel; k++)
        {
            output.Append('\t');
        }
        output.AppendLine("]");
    }

    public string DebugDumpToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        DebugDumpToStringBuilder(stringBuilder);
        return stringBuilder.ToString();
    }
}
