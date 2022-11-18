using UnityEngine;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.UnityTypes;

public class KeyValueTableVector3Reader : IFormattedTypeReader
{
    public object read(IFormattedFileReader reader)
    {
        reader = reader.readObject();
        if (reader == null)
        {
            return null;
        }
        return new Vector3(reader.readValue<float>("X"), reader.readValue<float>("Y"), reader.readValue<float>("Z"));
    }
}
