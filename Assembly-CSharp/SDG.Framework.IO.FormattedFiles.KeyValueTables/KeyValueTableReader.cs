using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables;

public class KeyValueTableReader : IFormattedFileReader
{
    protected static StringBuilder builder = new StringBuilder();

    protected string key;

    protected int index;

    protected string dictionaryKey;

    protected bool dictionaryInQuotes;

    protected bool dictionaryIgnoreNextChar;

    protected bool listInQuotes;

    protected bool listIgnoreNextChar;

    public Dictionary<string, object> table { get; protected set; }

    public virtual IEnumerable<string> getKeys()
    {
        return table.Keys;
    }

    public virtual bool containsKey(string key)
    {
        return table.ContainsKey(key);
    }

    public virtual void readKey(string key)
    {
        this.key = key;
        index = -1;
    }

    public virtual int readArrayLength(string key)
    {
        readKey(key);
        return readArrayLength();
    }

    public virtual int readArrayLength()
    {
        if (table.TryGetValue(key, out var value))
        {
            return (value as List<object>).Count;
        }
        return 0;
    }

    public virtual void readArrayIndex(string key, int index)
    {
        readKey(key);
        readArrayIndex(index);
    }

    public virtual void readArrayIndex(int index)
    {
        this.index = index;
    }

    public virtual string readValue(string key)
    {
        readKey(key);
        return readValue();
    }

    public virtual string readValue(int index)
    {
        readArrayIndex(index);
        return readValue();
    }

    public virtual string readValue(string key, int index)
    {
        readKey(key);
        readArrayIndex(index);
        return readValue();
    }

    public virtual string readValue()
    {
        if (index == -1)
        {
            if (!table.TryGetValue(key, out var value))
            {
                return null;
            }
            return (string)value;
        }
        if (table.TryGetValue(key, out var value2))
        {
            return (string)(value2 as List<object>)[index];
        }
        return null;
    }

    public virtual object readValue(Type type, string key)
    {
        readKey(key);
        return readValue(type);
    }

    public virtual object readValue(Type type, int index)
    {
        readArrayIndex(index);
        return readValue(type);
    }

    public virtual object readValue(Type type, string key, int index)
    {
        readKey(key);
        readArrayIndex(index);
        return readValue(type);
    }

    public virtual object readValue(Type type)
    {
        if (typeof(IFormattedFileReadable).IsAssignableFrom(type))
        {
            IFormattedFileReadable obj = Activator.CreateInstance(type) as IFormattedFileReadable;
            obj.read(this);
            return obj;
        }
        return KeyValueTableTypeReaderRegistry.read(this, type);
    }

    public virtual T readValue<T>(string key)
    {
        readKey(key);
        return readValue<T>();
    }

    public virtual T readValue<T>(int index)
    {
        readArrayIndex(index);
        return readValue<T>();
    }

    public virtual T readValue<T>(string key, int index)
    {
        readKey(key);
        readArrayIndex(index);
        return readValue<T>();
    }

    public virtual T readValue<T>()
    {
        if (typeof(IFormattedFileReadable).IsAssignableFrom(typeof(T)))
        {
            IFormattedFileReadable obj = Activator.CreateInstance<T>() as IFormattedFileReadable;
            obj.read(this);
            return (T)obj;
        }
        return KeyValueTableTypeReaderRegistry.read<T>(this);
    }

    public virtual IFormattedFileReader readObject(string key)
    {
        readKey(key);
        return readObject();
    }

    public virtual IFormattedFileReader readObject(int index)
    {
        readArrayIndex(index);
        return readObject();
    }

    public virtual IFormattedFileReader readObject(string key, int index)
    {
        readKey(key);
        readArrayIndex(index);
        return readObject();
    }

    public virtual IFormattedFileReader readObject()
    {
        if (index == -1)
        {
            if (table.TryGetValue(key, out var value))
            {
                return value as IFormattedFileReader;
            }
            return null;
        }
        if (table.TryGetValue(key, out var value2))
        {
            return (value2 as List<object>)[index] as IFormattedFileReader;
        }
        return null;
    }

    protected virtual bool canContinueReadDictionary(StreamReader input, Dictionary<string, object> scope)
    {
        return true;
    }

    public virtual void readDictionary(StreamReader input, Dictionary<string, object> scope)
    {
        dictionaryKey = null;
        dictionaryInQuotes = false;
        dictionaryIgnoreNextChar = false;
        while (!input.EndOfStream)
        {
            char c = (char)input.Read();
            if (dictionaryIgnoreNextChar)
            {
                builder.Append(c);
                dictionaryIgnoreNextChar = false;
                continue;
            }
            switch (c)
            {
            case '\\':
                dictionaryIgnoreNextChar = true;
                continue;
            case '"':
                if (dictionaryInQuotes)
                {
                    dictionaryInQuotes = false;
                    if (string.IsNullOrEmpty(dictionaryKey))
                    {
                        dictionaryKey = builder.ToString();
                        continue;
                    }
                    string value = builder.ToString();
                    if (!scope.ContainsKey(dictionaryKey))
                    {
                        scope.Add(dictionaryKey, value);
                    }
                    if (!canContinueReadDictionary(input, scope))
                    {
                        return;
                    }
                    dictionaryKey = null;
                }
                else
                {
                    dictionaryInQuotes = true;
                    builder.Length = 0;
                }
                continue;
            }
            if (dictionaryInQuotes)
            {
                builder.Append(c);
                continue;
            }
            switch (c)
            {
            case '{':
            {
                if (scope.TryGetValue(dictionaryKey, out var value3))
                {
                    KeyValueTableReader keyValueTableReader = (KeyValueTableReader)value3;
                    keyValueTableReader.readDictionary(input, keyValueTableReader.table);
                }
                else
                {
                    KeyValueTableReader keyValueTableReader2 = new KeyValueTableReader(input);
                    value3 = keyValueTableReader2;
                    scope.Add(dictionaryKey, keyValueTableReader2);
                }
                if (!canContinueReadDictionary(input, scope))
                {
                    return;
                }
                dictionaryKey = null;
                break;
            }
            case '}':
                return;
            case '[':
            {
                if (!scope.TryGetValue(dictionaryKey, out var value2))
                {
                    value2 = new List<object>();
                    scope.Add(dictionaryKey, value2);
                }
                readList(input, (List<object>)value2);
                if (!canContinueReadDictionary(input, scope))
                {
                    return;
                }
                dictionaryKey = null;
                break;
            }
            }
        }
    }

    public virtual void readList(StreamReader input, List<object> scope)
    {
        listInQuotes = false;
        listIgnoreNextChar = false;
        while (!input.EndOfStream)
        {
            char c = (char)input.Read();
            if (listIgnoreNextChar)
            {
                builder.Append(c);
                listIgnoreNextChar = false;
                continue;
            }
            switch (c)
            {
            case '\\':
                listIgnoreNextChar = true;
                continue;
            case '"':
                if (listInQuotes)
                {
                    listInQuotes = false;
                    string item = builder.ToString();
                    scope.Add(item);
                }
                else
                {
                    listInQuotes = true;
                    builder.Length = 0;
                }
                continue;
            }
            if (listInQuotes)
            {
                builder.Append(c);
                continue;
            }
            switch (c)
            {
            case '{':
            {
                KeyValueTableReader item2 = new KeyValueTableReader(input);
                scope.Add(item2);
                break;
            }
            case ']':
                return;
            }
        }
    }

    public KeyValueTableReader()
    {
        table = new Dictionary<string, object>();
    }

    public KeyValueTableReader(StreamReader input)
    {
        table = new Dictionary<string, object>();
        readDictionary(input, table);
    }
}
