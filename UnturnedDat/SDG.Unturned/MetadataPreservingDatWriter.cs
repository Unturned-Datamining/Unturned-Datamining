using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public sealed class MetadataPreservingDatWriter
{
    private Stack<List<KeyValuePair<string, IDatNode>>> kvpPool = new Stack<List<KeyValuePair<string, IDatNode>>>();

    private Stack<List<IDatNode>> listPool = new Stack<List<IDatNode>>();

    private DatWriter output;

    public void WriteRootDictionary(IDatDictionary rootDictionary, DatWriter writer)
    {
        if (rootDictionary == null)
        {
            throw new ArgumentNullException("rootDictionary");
        }
        if (writer == null)
        {
            throw new ArgumentNullException("writer");
        }
        if (!(rootDictionary is IMetadataPreservingDatWriterCompatible))
        {
            throw new ArgumentException("not compatible", "rootDictionary");
        }
        output = writer;
        WriteDictionary(rootDictionary);
    }

    private void WriteDictionary(IDatDictionary dictionary)
    {
        if (kvpPool.TryPop(out var result))
        {
            result.Clear();
            if (dictionary.Count > result.Capacity)
            {
                result.Capacity = dictionary.Count;
            }
        }
        else
        {
            result = new List<KeyValuePair<string, IDatNode>>(dictionary.Count);
        }
        result.AddRange(dictionary);
        result.Sort(DictionaryLineNumberComparer);
        int previousElementLatestLineNumber = 0;
        int previousElementMargin = 0;
        foreach (KeyValuePair<string, IDatNode> item in result)
        {
            IMetadataPreservingDatWriterCompatible metadataPreservingDatWriterCompatible = (IMetadataPreservingDatWriterCompatible)item.Value;
            WriteCommon(metadataPreservingDatWriterCompatible, ref previousElementLatestLineNumber, ref previousElementMargin);
            switch (metadataPreservingDatWriterCompatible.NodeType)
            {
            case EDatNodeType.Value:
            {
                IDatValue datValue = (IDatValue)metadataPreservingDatWriterCompatible;
                output.WriteKeyValue(item.Key, datValue.Value, metadataPreservingDatWriterCompatible.WriterGetInlineComment());
                break;
            }
            case EDatNodeType.Dictionary:
            {
                IDatDictionary dictionary2 = (IDatDictionary)metadataPreservingDatWriterCompatible;
                output.WriteDictionaryStart(item.Key);
                WriteDictionary(dictionary2);
                output.WriteDictionaryEnd();
                break;
            }
            case EDatNodeType.List:
            {
                IDatList list = (IDatList)metadataPreservingDatWriterCompatible;
                output.WriteListStart(item.Key);
                WriteList(list);
                output.WriteListEnd();
                break;
            }
            }
        }
        kvpPool.Push(result);
    }

    private void WriteList(IDatList list)
    {
        if (listPool.TryPop(out var result))
        {
            result.Clear();
            if (list.Count > result.Capacity)
            {
                result.Capacity = list.Count;
            }
        }
        else
        {
            result = new List<IDatNode>(list.Count);
        }
        result.AddRange(list);
        result.Sort(ListLineNumberComparer);
        int previousElementLatestLineNumber = 0;
        int previousElementMargin = 0;
        foreach (IMetadataPreservingDatWriterCompatible item in result)
        {
            WriteCommon(item, ref previousElementLatestLineNumber, ref previousElementMargin);
            switch (item.NodeType)
            {
            case EDatNodeType.Value:
            {
                IDatValue datValue = (IDatValue)item;
                output.WriteValue(datValue.Value, item.WriterGetInlineComment());
                break;
            }
            case EDatNodeType.Dictionary:
            {
                IDatDictionary dictionary = (IDatDictionary)item;
                output.WriteDictionaryStart();
                WriteDictionary(dictionary);
                output.WriteDictionaryEnd();
                break;
            }
            case EDatNodeType.List:
            {
                IDatList list2 = (IDatList)item;
                output.WriteListStart();
                WriteList(list2);
                output.WriteListEnd();
                break;
            }
            }
        }
        listPool.Push(result);
    }

    private void WriteCommon(IMetadataPreservingDatWriterCompatible node, ref int previousElementLatestLineNumber, ref int previousElementMargin)
    {
        int num = node.WriterGetEarliestLineNumber();
        int num2 = node.WriterGetLatestLineNumber();
        node.WriterGetMargins(out var upperMargin, out var lowerMargin);
        int a = 0;
        if (num > 0 && previousElementLatestLineNumber > 0)
        {
            a = num - previousElementLatestLineNumber - 1;
        }
        int b = Mathf.Max(previousElementMargin, upperMargin);
        a = Mathf.Max(a, b);
        previousElementLatestLineNumber = num2;
        previousElementMargin = lowerMargin;
        while (a > 0)
        {
            output.WriteEmptyLine();
            a--;
        }
        DatComment? datComment = node.WriterGetPrefixComment();
        if (datComment.HasValue && !datComment.Value.AreMessageLinesNullOrEmpty)
        {
            string[] messageLines = datComment.Value.MessageLines;
            foreach (string message in messageLines)
            {
                output.WriteComment(message);
            }
        }
    }

    private int DictionaryLineNumberComparer(KeyValuePair<string, IDatNode> lhs, KeyValuePair<string, IDatNode> rhs)
    {
        return ListLineNumberComparer(lhs.Value, rhs.Value);
    }

    private int ListLineNumberComparer(IDatNode baseLhs, IDatNode baseRhs)
    {
        IMetadataPreservingDatWriterCompatible obj = (IMetadataPreservingDatWriterCompatible)baseLhs;
        IMetadataPreservingDatWriterCompatible metadataPreservingDatWriterCompatible = (IMetadataPreservingDatWriterCompatible)baseRhs;
        obj.WriterGetSortingParameters(out var lineNumber, out var sortOrder);
        metadataPreservingDatWriterCompatible.WriterGetSortingParameters(out var lineNumber2, out var sortOrder2);
        if (lineNumber > 0 && lineNumber2 > 0 && lineNumber != lineNumber2)
        {
            return lineNumber.CompareTo(lineNumber2);
        }
        return sortOrder.CompareTo(sortOrder2);
    }
}
