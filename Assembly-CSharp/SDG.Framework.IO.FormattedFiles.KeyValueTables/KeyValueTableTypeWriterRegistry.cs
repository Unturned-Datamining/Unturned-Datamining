using System;
using System.Collections.Generic;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables;

public class KeyValueTableTypeWriterRegistry
{
    private static Dictionary<Type, IFormattedTypeWriter> writers = new Dictionary<Type, IFormattedTypeWriter>();

    public static void write<T>(IFormattedFileWriter output, T value)
    {
        if (writers.TryGetValue(typeof(T), out var value2))
        {
            value2.write(output, value);
        }
        else
        {
            output.writeValue(value.ToString());
        }
    }

    public static void write(IFormattedFileWriter output, object value)
    {
        if (writers.TryGetValue(value.GetType(), out var value2))
        {
            value2.write(output, value);
        }
        else
        {
            output.writeValue(value.ToString());
        }
    }

    public static void add<T>(IFormattedTypeWriter writer)
    {
        add(typeof(T), writer);
    }

    public static void add(Type type, IFormattedTypeWriter writer)
    {
        writers.Add(type, writer);
    }

    public static void remove<T>()
    {
        remove(typeof(T));
    }

    public static void remove(Type type)
    {
        writers.Remove(type);
    }
}
