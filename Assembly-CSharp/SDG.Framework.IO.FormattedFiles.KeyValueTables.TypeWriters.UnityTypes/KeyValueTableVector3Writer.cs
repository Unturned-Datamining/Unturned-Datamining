using UnityEngine;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.UnityTypes;

public class KeyValueTableVector3Writer : IFormattedTypeWriter
{
    public void write(IFormattedFileWriter writer, object value)
    {
        writer.beginObject();
        Vector3 vector = (Vector3)value;
        writer.writeValue("X", vector.x);
        writer.writeValue("Y", vector.y);
        writer.writeValue("Z", vector.z);
        writer.endObject();
    }
}
