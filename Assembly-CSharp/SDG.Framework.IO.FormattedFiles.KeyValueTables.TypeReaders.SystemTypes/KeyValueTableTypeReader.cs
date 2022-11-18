using System;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.SystemTypes;

public class KeyValueTableTypeReader : IFormattedTypeReader
{
    public object read(IFormattedFileReader reader)
    {
        string text = reader.readValue();
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        text = KeyValueTableTypeRedirectorRegistry.chase(text);
        return Type.GetType(text);
    }
}
