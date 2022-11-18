using UnityEngine;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.UnityTypes;

public class KeyValueTableColorReader : IFormattedTypeReader
{
    public object read(IFormattedFileReader reader)
    {
        reader = reader.readObject();
        if (reader == null)
        {
            return null;
        }
        return (Color)new Color32(reader.readValue<byte>("R"), reader.readValue<byte>("G"), reader.readValue<byte>("B"), reader.readValue<byte>("A"));
    }
}
