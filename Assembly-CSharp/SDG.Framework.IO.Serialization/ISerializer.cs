using System.IO;

namespace SDG.Framework.IO.Serialization;

public interface ISerializer
{
    void serialize<T>(T instance, byte[] data, int offset, out int size, bool isFormatted);

    void serialize<T>(T instance, MemoryStream memoryStream, bool isFormatted);

    void serialize<T>(T instance, string path, bool isFormatted);
}
