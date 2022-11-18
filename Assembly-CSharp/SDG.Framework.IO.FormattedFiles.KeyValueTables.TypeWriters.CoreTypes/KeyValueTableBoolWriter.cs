namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.CoreTypes;

public class KeyValueTableBoolWriter : IFormattedTypeWriter
{
    public void write(IFormattedFileWriter writer, object value)
    {
        bool flag = (bool)value;
        writer.writeValue(flag ? "true" : "false");
    }
}
