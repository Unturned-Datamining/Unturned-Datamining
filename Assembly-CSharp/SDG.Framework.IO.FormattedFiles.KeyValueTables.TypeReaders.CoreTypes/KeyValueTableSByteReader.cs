using System.Globalization;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.CoreTypes;

public class KeyValueTableSByteReader : IFormattedTypeReader
{
    public object read(IFormattedFileReader reader)
    {
        sbyte.TryParse(reader.readValue(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
        return result;
    }
}
