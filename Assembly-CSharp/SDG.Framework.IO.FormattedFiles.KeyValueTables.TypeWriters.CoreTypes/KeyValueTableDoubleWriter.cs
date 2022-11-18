using System.Globalization;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.CoreTypes;

public class KeyValueTableDoubleWriter : IFormattedTypeWriter
{
    public void write(IFormattedFileWriter writer, object value)
    {
        string value2 = ((double)value).ToString(CultureInfo.InvariantCulture);
        writer.writeValue(value2);
    }
}
