namespace SDG.Provider.Services.Cloud;

public interface ICloudService : IService
{
    bool read(string path, byte[] data);

    bool write(string path, byte[] data, int size);

    bool getSize(string path, out int size);

    bool exists(string path, out bool exists);

    bool delete(string path);
}
