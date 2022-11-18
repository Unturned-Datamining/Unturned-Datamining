using System.Globalization;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.CoreTypes;

public class KeyValueTableUIntReader : IFormattedTypeReader
{
    public object read(IFormattedFileReader reader)
    {
        uint.TryParse(reader.readValue(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
        return result;
    }
}
