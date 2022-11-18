using System;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.SystemTypes;

public class KeyValueTableGUIDWriter : IFormattedTypeWriter
{
    public void write(IFormattedFileWriter writer, object value)
    {
        writer.writeValue(((Guid)value).ToString("N"));
    }
}
