using System.Globalization;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.CoreTypes;

public class KeyValueTableShortReader : IFormattedTypeReader
{
    public object read(IFormattedFileReader reader)
    {
        short.TryParse(reader.readValue(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
        return result;
    }
}
