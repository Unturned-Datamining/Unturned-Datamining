using UnityEngine;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.UnityTypes;

public class KeyValueTableVector4Writer : IFormattedTypeWriter
{
    public void write(IFormattedFileWriter writer, object value)
    {
        writer.beginObject();
        Vector4 vector = (Vector4)value;
        writer.writeValue("X", vector.x);
        writer.writeValue("Y", vector.y);
        writer.writeValue("Z", vector.z);
        writer.writeValue("W", vector.w);
        writer.endObject();
    }
}
