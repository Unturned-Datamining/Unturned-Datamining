using System;
using SDG.Unturned;

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
        if (text.IndexOfAny(DatValue.INVALID_TYPE_CHARS) >= 0)
        {
            return null;
        }
        return Type.GetType(text);
    }
}
