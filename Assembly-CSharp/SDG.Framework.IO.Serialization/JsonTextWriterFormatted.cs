using System.IO;
using Newtonsoft.Json;

namespace SDG.Framework.IO.Serialization;

public class JsonTextWriterFormatted : JsonTextWriter
{
    public override void WriteStartArray()
    {
        base.Formatting = Formatting.None;
        WriteIndent();
        base.WriteStartArray();
        base.Formatting = Formatting.Indented;
    }

    public JsonTextWriterFormatted(TextWriter textWriter)
        : base(textWriter)
    {
        base.IndentChar = '\t';
        base.Indentation = 1;
    }
}
