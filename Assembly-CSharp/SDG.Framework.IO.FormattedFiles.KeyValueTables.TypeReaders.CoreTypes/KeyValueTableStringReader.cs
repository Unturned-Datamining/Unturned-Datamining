namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.CoreTypes;

public class KeyValueTableStringReader : IFormattedTypeReader
{
    public object read(IFormattedFileReader reader)
    {
        return reader.readValue();
    }
}
