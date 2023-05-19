using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SDG.Unturned;

public sealed class DatWriter : IDisposable
{
    private enum EWriterToken
    {
        Dictionary,
        List,
        Key
    }

    private Stack<EWriterToken> stack = new Stack<EWriterToken>();

    private TextWriter output;

    private int indentationDepth;

    public DatWriter()
    {
    }

    public DatWriter(TextWriter output)
    {
        SetOutput(output);
    }

    public void SetOutput(TextWriter output)
    {
        this.output = output;
        output.NewLine = "\n";
        stack.Clear();
        indentationDepth = 0;
    }

    public void Dispose()
    {
        CloseStack();
    }

    public void CloseStack()
    {
        while (stack.Count > 0)
        {
            switch (stack.Peek())
            {
            case EWriterToken.Dictionary:
                WriteDictionaryEnd();
                break;
            case EWriterToken.List:
                WriteListEnd();
                break;
            case EWriterToken.Key:
                stack.Pop();
                break;
            }
        }
    }

    public void WriteEmptyLine()
    {
        output.WriteLine();
    }

    public void WriteKey(string key)
    {
        if (key == null)
        {
            throw new ArgumentNullException("key");
        }
        if (key.Length < 1)
        {
            throw new ArgumentException("cannot be empty", "key");
        }
        if (stack.Count > 0 && stack.Peek() != 0)
        {
            throw new Exception($"Cannot write key (\"{key}\") into {stack.Peek()}");
        }
        WriteIndentation();
        output.Write(key);
        stack.Push(EWriterToken.Key);
    }

    public void WriteValue(string value, string comment = null)
    {
        if (stack.Count < 1)
        {
            throw new Exception("Writing value into empty slot");
        }
        if (stack.Peek() == EWriterToken.Dictionary)
        {
            throw new Exception($"Cannot write value (\"{value}\") into {stack.Peek()}");
        }
        if (string.IsNullOrEmpty(value))
        {
            if (stack.Peek() == EWriterToken.Key)
            {
                stack.Pop();
            }
            if (!string.IsNullOrEmpty(comment))
            {
                output.Write(" // ");
                output.Write(comment);
            }
            output.WriteLine();
            return;
        }
        if (stack.Peek() == EWriterToken.Key)
        {
            output.Write(' ');
            stack.Pop();
        }
        else
        {
            WriteIndentation();
        }
        bool flag = value[0] == '"' || !string.IsNullOrEmpty(comment);
        if (flag)
        {
            output.Write('"');
        }
        foreach (char c in value)
        {
            switch (c)
            {
            case '\n':
                output.Write("\\n");
                break;
            case '\r':
                output.Write("\\r");
                break;
            case '\t':
                output.Write("\\t");
                break;
            case '\\':
                output.Write("\\\\");
                break;
            case '"':
                output.Write(flag ? "\\\"" : "\"");
                break;
            default:
                output.Write(c);
                break;
            }
        }
        if (flag)
        {
            output.Write('"');
        }
        if (!string.IsNullOrEmpty(comment))
        {
            output.Write(" // ");
            output.Write(comment);
        }
        output.WriteLine();
    }

    public void WriteValue(sbyte value, string comment = null)
    {
        WriteValue(value.ToString(CultureInfo.InvariantCulture), comment);
    }

    public void WriteValue(byte value, string comment = null)
    {
        WriteValue(value.ToString(CultureInfo.InvariantCulture), comment);
    }

    public void WriteValue(short value, string comment = null)
    {
        WriteValue(value.ToString(CultureInfo.InvariantCulture), comment);
    }

    public void WriteValue(ushort value, string comment = null)
    {
        WriteValue(value.ToString(CultureInfo.InvariantCulture), comment);
    }

    public void WriteValue(int value, string comment = null)
    {
        WriteValue(value.ToString(CultureInfo.InvariantCulture), comment);
    }

    public void WriteValue(uint value, string comment = null)
    {
        WriteValue(value.ToString(CultureInfo.InvariantCulture), comment);
    }

    public void WriteValue(long value, string comment = null)
    {
        WriteValue(value.ToString(CultureInfo.InvariantCulture), comment);
    }

    public void WriteValue(ulong value, string comment = null)
    {
        WriteValue(value.ToString(CultureInfo.InvariantCulture), comment);
    }

    public void WriteValue(float value, string comment = null)
    {
        WriteValue(value.ToString(CultureInfo.InvariantCulture), comment);
    }

    public void WriteValue(double value, string comment = null)
    {
        WriteValue(value.ToString(CultureInfo.InvariantCulture), comment);
    }

    public void WriteValueEnumString<T>(T value, string comment = null) where T : struct
    {
        WriteValue(value.ToString(), comment);
    }

    public void WriteValue(bool value, string comment = null)
    {
        WriteValue(value ? "true" : "false", comment);
    }

    public void WriteValue(Guid value, string comment = null)
    {
        WriteValue(value.ToString("N"), comment);
    }

    public void WriteValue(DateTime value, string comment = null)
    {
        if (value.Hour == 0 && value.Minute == 0 && value.Second == 0)
        {
            WriteValue(value.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture), comment);
        }
        else
        {
            WriteValue(value.ToString("yyyy'-'MM'-'dd HH':'mm':'ss", CultureInfo.InvariantCulture), comment);
        }
    }

    public void WriteKeyValue(string key, string value, string comment = null)
    {
        WriteKey(key);
        WriteValue(value, comment);
    }

    public void WriteKeyValue(string key, sbyte value, string comment = null)
    {
        WriteKey(key);
        WriteValue(value, comment);
    }

    public void WriteKeyValue(string key, byte value, string comment = null)
    {
        WriteKey(key);
        WriteValue(value, comment);
    }

    public void WriteKeyValue(string key, short value, string comment = null)
    {
        WriteKey(key);
        WriteValue(value, comment);
    }

    public void WriteKeyValue(string key, ushort value, string comment = null)
    {
        WriteKey(key);
        WriteValue(value, comment);
    }

    public void WriteKeyValue(string key, int value, string comment = null)
    {
        WriteKey(key);
        WriteValue(value, comment);
    }

    public void WriteKeyValue(string key, uint value, string comment = null)
    {
        WriteKey(key);
        WriteValue(value, comment);
    }

    public void WriteKeyValue(string key, long value, string comment = null)
    {
        WriteKey(key);
        WriteValue(value, comment);
    }

    public void WriteKeyValue(string key, ulong value, string comment = null)
    {
        WriteKey(key);
        WriteValue(value, comment);
    }

    public void WriteKeyValue(string key, float value, string comment = null)
    {
        WriteKey(key);
        WriteValue(value, comment);
    }

    public void WriteKeyValue(string key, double value, string comment = null)
    {
        WriteKey(key);
        WriteValue(value, comment);
    }

    public void WriteKeyValueEnumString<T>(string key, T value, string comment = null) where T : struct
    {
        WriteKey(key);
        WriteValueEnumString(value, comment);
    }

    public void WriteKeyValue(string key, bool value, string comment = null)
    {
        WriteKey(key);
        WriteValue(value, comment);
    }

    public void WriteKeyValue(string key, Guid value, string comment = null)
    {
        WriteKey(key);
        WriteValue(value, comment);
    }

    public void WriteKeyValue(string key, DateTime value, string comment = null)
    {
        WriteKey(key);
        WriteValue(value, comment);
    }

    public void WriteDictionaryStart()
    {
        if (stack.Count < 1)
        {
            throw new Exception("Cannot write dictionary into root without a key");
        }
        if (stack.Peek() == EWriterToken.Dictionary)
        {
            throw new Exception("Cannot write dictionary into dictionary without a key");
        }
        if (stack.Peek() == EWriterToken.Key)
        {
            output.WriteLine();
            stack.Pop();
        }
        WriteIndentation();
        output.WriteLine('{');
        stack.Push(EWriterToken.Dictionary);
        indentationDepth++;
    }

    public void WriteDictionaryStart(string key)
    {
        WriteKey(key);
        WriteDictionaryStart();
    }

    public void WriteDictionaryEnd()
    {
        if (stack.Count < 1)
        {
            throw new Exception("Reached end of stack");
        }
        if (stack.Peek() != 0)
        {
            throw new Exception($"Current element ({stack.Peek()}) is not a dictionary");
        }
        indentationDepth--;
        stack.Pop();
        WriteIndentation();
        output.WriteLine('}');
    }

    public void WriteListStart()
    {
        if (stack.Count < 1)
        {
            throw new Exception("Cannot write dictionary into root without a key name");
        }
        if (stack.Peek() == EWriterToken.Dictionary)
        {
            throw new Exception("Cannot write list into dictionary without a key");
        }
        if (stack.Peek() == EWriterToken.Key)
        {
            output.WriteLine();
            stack.Pop();
        }
        WriteIndentation();
        output.WriteLine('[');
        stack.Push(EWriterToken.List);
        indentationDepth++;
    }

    public void WriteListStart(string key)
    {
        WriteKey(key);
        WriteListStart();
    }

    public void WriteListEnd()
    {
        if (stack.Count < 1)
        {
            throw new Exception("Reached end of stack");
        }
        if (stack.Peek() != EWriterToken.List)
        {
            throw new Exception($"Current element ({stack.Peek()}) is not a list");
        }
        indentationDepth--;
        stack.Pop();
        WriteIndentation();
        output.WriteLine(']');
    }

    public void WriteComment(string message)
    {
        WriteIndentation();
        output.Write("// ");
        output.WriteLine(message);
    }

    public void WriteNode(IDatNode node)
    {
        if (node == null)
        {
            throw new ArgumentNullException("node");
        }
        switch (node.NodeType)
        {
        case EDatNodeType.Value:
            WriteValue(((DatValue)node).value);
            break;
        case EDatNodeType.Dictionary:
            WriteDictionary((DatDictionary)node);
            break;
        case EDatNodeType.List:
            WriteList((DatList)node);
            break;
        }
    }

    public void WriteDictionary(DatDictionary dictionary)
    {
        if (dictionary == null)
        {
            throw new ArgumentNullException("dictionary");
        }
        bool flag = stack.Count < 1;
        if (!flag)
        {
            WriteDictionaryStart();
        }
        foreach (KeyValuePair<string, IDatNode> item in dictionary)
        {
            WriteKey(item.Key);
            if (item.Value != null)
            {
                WriteNode(item.Value);
            }
            else
            {
                WriteValue(null);
            }
        }
        if (!flag)
        {
            WriteDictionaryEnd();
        }
    }

    public void WriteList(DatList list)
    {
        if (list == null)
        {
            throw new ArgumentNullException("list");
        }
        WriteListStart();
        foreach (IDatNode item in list)
        {
            if (item != null)
            {
                WriteNode(item);
            }
        }
        WriteListEnd();
    }

    private void WriteIndentation()
    {
        for (int i = 0; i < indentationDepth; i++)
        {
            output.Write('\t');
        }
    }
}
