using System.Globalization;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.CoreTypes;

public class KeyValueTableUShortReader : IFormattedTypeReader
{
    public object read(IFormattedFileReader reader)
    {
        ushort.TryParse(reader.readValue(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
        return result;
    }
}
