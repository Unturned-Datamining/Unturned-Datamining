using UnityEngine;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeReaders.UnityTypes;

public class KeyValueTableQuaternionReader : IFormattedTypeReader
{
    public object read(IFormattedFileReader reader)
    {
        reader = reader.readObject();
        if (reader == null)
        {
            return null;
        }
        return new Quaternion(reader.readValue<float>("X"), reader.readValue<float>("Y"), reader.readValue<float>("Z"), reader.readValue<float>("W"));
    }
}
