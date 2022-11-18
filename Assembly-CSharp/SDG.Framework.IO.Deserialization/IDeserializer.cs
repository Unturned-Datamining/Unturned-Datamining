using System.IO;

namespace SDG.Framework.IO.Deserialization;

public interface IDeserializer
{
    T deserialize<T>(byte[] data, int offset);

    T deserialize<T>(MemoryStream memoryStream);

    T deserialize<T>(string path);
}
