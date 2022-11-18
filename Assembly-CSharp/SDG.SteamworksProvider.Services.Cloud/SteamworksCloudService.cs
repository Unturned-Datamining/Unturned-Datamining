using System;
using SDG.Provider.Services;
using SDG.Provider.Services.Cloud;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Cloud;

public class SteamworksCloudService : Service, ICloudService, IService
{
    public bool read(string path, byte[] data)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (data == null)
        {
            throw new ArgumentNullException("data");
        }
        int fileSize = SteamRemoteStorage.GetFileSize(path);
        if (data.Length < fileSize)
        {
            return false;
        }
        if (SteamRemoteStorage.FileRead(path, data, fileSize) != fileSize)
        {
            return false;
        }
        return true;
    }

    public bool write(string path, byte[] data, int size)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (data == null)
        {
            throw new ArgumentNullException("data");
        }
        return SteamRemoteStorage.FileWrite(path, data, size);
    }

    public bool getSize(string path, out int size)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        size = SteamRemoteStorage.GetFileSize(path);
        return true;
    }

    public bool exists(string path, out bool exists)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        exists = SteamRemoteStorage.FileExists(path);
        return true;
    }

    public bool delete(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        return SteamRemoteStorage.FileDelete(path);
    }
}
