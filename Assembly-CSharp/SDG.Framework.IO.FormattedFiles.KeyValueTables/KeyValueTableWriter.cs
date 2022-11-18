using System.IO;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables;

public class KeyValueTableWriter : IFormattedFileWriter
{
    protected StreamWriter writer;

    protected int indentationCount;

    protected bool hasWritten;

    protected bool wroteKey;

    public virtual void writeKey(string key)
    {
        if (hasWritten)
        {
            writer.WriteLine();
        }
        writeIndents();
        writer.Write('"');
        writer.Write(key);
        writer.Write('"');
        hasWritten = true;
        wroteKey = true;
    }

    public virtual void writeValue(string key, string value)
    {
        writeKey(key);
        writeValue(value);
    }

    public virtual void writeValue(string value)
    {
        if (wroteKey)
        {
            writer.Write(' ');
        }
        else
        {
            if (hasWritten)
            {
                writer.WriteLine();
            }
            writeIndents();
        }
        writer.Write('"');
        writer.Write(value);
        writer.Write('"');
        wroteKey = false;
    }

    public virtual void writeValue(string key, object value)
    {
        writeKey(key);
        writeValue(value);
    }

    public virtual void writeValue(object value)
    {
        if (value is IFormattedFileWritable)
        {
            (value as IFormattedFileWritable).write(this);
        }
        else
        {
            KeyValueTableTypeWriterRegistry.write(this, value);
        }
    }

    public virtual void writeValue<T>(string key, T value)
    {
        writeKey(key);
        writeValue(value);
    }

    public virtual void writeValue<T>(T value)
    {
        if (value is IFormattedFileWritable)
        {
            (value as IFormattedFileWritable).write(this);
        }
        else
        {
            KeyValueTableTypeWriterRegistry.write(this, value);
        }
    }

    public virtual void beginObject(string key)
    {
        writeKey(key);
        beginObject();
    }

    public virtual void beginObject()
    {
        if (hasWritten)
        {
            writer.WriteLine();
        }
        writeIndents();
        writer.Write('{');
        indentationCount++;
        hasWritten = true;
        wroteKey = false;
    }

    public virtual void endObject()
    {
        if (hasWritten)
        {
            writer.WriteLine();
        }
        indentationCount--;
        writeIndents();
        writer.Write('}');
    }

    public virtual void beginArray(string key)
    {
        writeKey(key);
        beginArray();
    }

    public virtual void beginArray()
    {
        if (hasWritten)
        {
            writer.WriteLine();
        }
        writeIndents();
        writer.Write('[');
        indentationCount++;
        hasWritten = true;
        wroteKey = false;
    }

    public virtual void endArray()
    {
        if (hasWritten)
        {
            writer.WriteLine();
        }
        indentationCount--;
        writeIndents();
        writer.Write(']');
    }

    protected virtual void writeIndents()
    {
        for (int i = 0; i < indentationCount; i++)
        {
            writer.Write('\t');
        }
    }

    public KeyValueTableWriter(StreamWriter writer)
    {
        this.writer = writer;
        indentationCount = 0;
        hasWritten = false;
        wroteKey = false;
    }
}
