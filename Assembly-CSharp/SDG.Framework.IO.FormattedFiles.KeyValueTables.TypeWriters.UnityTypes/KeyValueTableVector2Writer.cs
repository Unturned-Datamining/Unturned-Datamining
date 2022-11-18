using UnityEngine;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.UnityTypes;

public class KeyValueTableVector2Writer : IFormattedTypeWriter
{
    public void write(IFormattedFileWriter writer, object value)
    {
        writer.beginObject();
        Vector2 vector = (Vector2)value;
        writer.writeValue("X", vector.x);
        writer.writeValue("Y", vector.y);
        writer.endObject();
    }
}
