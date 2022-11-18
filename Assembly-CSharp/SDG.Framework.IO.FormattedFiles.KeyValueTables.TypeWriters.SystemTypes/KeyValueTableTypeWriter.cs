using System;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.SystemTypes;

public class KeyValueTableTypeWriter : IFormattedTypeWriter
{
    public void write(IFormattedFileWriter writer, object value)
    {
        Type type = value as Type;
        writer.writeValue(type.AssemblyQualifiedName);
    }
}
