using System.Globalization;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.CoreTypes;

public class KeyValueTableFloatWriter : IFormattedTypeWriter
{
    public void write(IFormattedFileWriter writer, object value)
    {
        string value2 = ((float)value).ToString(CultureInfo.InvariantCulture);
        writer.writeValue(value2);
    }
}
