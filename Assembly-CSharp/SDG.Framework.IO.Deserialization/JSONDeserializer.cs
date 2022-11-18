using System.IO;
using Newtonsoft.Json;

namespace SDG.Framework.IO.Deserialization;

public class JSONDeserializer : IDeserializer
{
    public T deserialize<T>(byte[] data, int offset)
    {
        MemoryStream memoryStream = new MemoryStream(data, offset, data.Length - offset);
        T result = deserialize<T>(memoryStream);
        memoryStream.Close();
        memoryStream.Dispose();
        return result;
    }

    public T deserialize<T>(MemoryStream memoryStream)
    {
        StreamReader streamReader = new StreamReader(memoryStream);
        JsonReader jsonReader = new JsonTextReader(streamReader);
        JsonSerializer jsonSerializer = new JsonSerializer();
        try
        {
            return jsonSerializer.Deserialize<T>(jsonReader);
        }
        finally
        {
            jsonReader.Close();
            streamReader.Close();
            streamReader.Dispose();
        }
    }

    public T deserialize<T>(string path)
    {
        StreamReader streamReader = new StreamReader(path);
        JsonReader jsonReader = new JsonTextReader(streamReader);
        JsonSerializer jsonSerializer = new JsonSerializer();
        try
        {
            return jsonSerializer.Deserialize<T>(jsonReader);
        }
        finally
        {
            jsonReader.Close();
            streamReader.Close();
            streamReader.Dispose();
        }
    }
}
