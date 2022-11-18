using UnityEngine;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables.TypeWriters.UnityTypes;

public class KeyValueTableColorWriter : IFormattedTypeWriter
{
    public void write(IFormattedFileWriter writer, object value)
    {
        writer.beginObject();
        Color32 color = (Color)value;
        writer.writeValue("R", color.r);
        writer.writeValue("G", color.g);
        writer.writeValue("B", color.b);
        writer.writeValue("A", color.a);
        writer.endObject();
    }
}
