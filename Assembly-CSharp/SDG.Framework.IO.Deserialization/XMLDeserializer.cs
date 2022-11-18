using System.IO;
using System.Xml.Serialization;

namespace SDG.Framework.IO.Deserialization;

public class XMLDeserializer : IDeserializer
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
        return (T)new XmlSerializer(typeof(T)).Deserialize(memoryStream);
    }

    public T deserialize<T>(string path)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
        StreamReader streamReader = new StreamReader(path);
        try
        {
            return (T)xmlSerializer.Deserialize(streamReader);
        }
        finally
        {
            streamReader.Close();
            streamReader.Dispose();
        }
    }
}
