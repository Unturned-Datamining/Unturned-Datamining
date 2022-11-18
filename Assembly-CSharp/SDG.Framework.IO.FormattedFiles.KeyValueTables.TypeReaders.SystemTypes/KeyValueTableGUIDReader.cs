using System;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.SystemTypes;

public class KeyValueTableGUIDReader : IFormattedTypeReader
{
    public object read(IFormattedFileReader reader)
    {
        string text = reader.readValue();
        if (string.IsNullOrEmpty(text) || text.Equals("0"))
        {
            return Guid.Empty;
        }
        return new Guid(text);
    }
}
