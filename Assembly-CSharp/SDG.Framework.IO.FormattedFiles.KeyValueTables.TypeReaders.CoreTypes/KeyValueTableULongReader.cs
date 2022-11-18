using System.Globalization;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.CoreTypes;

public class KeyValueTableULongReader : IFormattedTypeReader
{
    public object read(IFormattedFileReader reader)
    {
        ulong.TryParse(reader.readValue(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
        return result;
    }
}
