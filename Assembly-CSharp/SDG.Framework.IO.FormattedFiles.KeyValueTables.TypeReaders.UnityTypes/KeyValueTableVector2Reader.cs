using UnityEngine;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.UnityTypes;

public class KeyValueTableVector2Reader : IFormattedTypeReader
{
    public object read(IFormattedFileReader reader)
    {
        reader = reader.readObject();
        if (reader == null)
        {
            return null;
        }
        return new Vector2(reader.readValue<float>("X"), reader.readValue<float>("Y"));
    }
}
