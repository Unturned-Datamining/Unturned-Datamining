using System;
using System.Collections.Generic;
using SDG.Unturned;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables;

public class KeyValueTableTypeReaderRegistry
{
    private static Dictionary<Type, IFormattedTypeReader> readers = new Dictionary<Type, IFormattedTypeReader>();

    public static T read<T>(IFormattedFileReader input)
    {
        if (readers.TryGetValue(typeof(T), out var value))
        {
            object obj = value.read(input);
            if (obj == null)
            {
                return default(T);
            }
            return (T)obj;
        }
        if (typeof(T).IsEnum)
        {
            string value2 = input.readValue();
            if (string.IsNullOrEmpty(value2))
            {
                return default(T);
            }
            return (T)Enum.Parse(typeof(T), value2, ignoreCase: true);
        }
        UnturnedLog.error("Failed to find reader for: " + typeof(T));
        return default(T);
    }

    public static object read(IFormattedFileReader input, Type type)
    {
        if (readers.TryGetValue(type, out var value))
        {
            object obj = value.read(input);
            if (obj == null)
            {
                return type.getDefaultValue();
            }
            return obj;
        }
        if (type.IsEnum)
        {
            string value2 = input.readValue();
            if (string.IsNullOrEmpty(value2))
            {
                return type.getDefaultValue();
            }
            return Enum.Parse(type, value2, ignoreCase: true);
        }
        UnturnedLog.error("Failed to find reader for: " + type);
        return type.getDefaultValue();
    }

    public static void add<T>(IFormattedTypeReader reader)
    {
        add(typeof(T), reader);
    }

    public static void add(Type type, IFormattedTypeReader reader)
    {
        readers.Add(type, reader);
    }

    public static void remove<T>()
    {
        remove(typeof(T));
    }

    public static void remove(Type type)
    {
        readers.Remove(type);
    }
}
