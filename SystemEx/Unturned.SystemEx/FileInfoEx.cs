using System.IO;

namespace Unturned.SystemEx;

public static class FileInfoEx
{
    public static FileInfo Join(DirectoryInfo path1, string path2)
    {
        return new FileInfo(PathEx.Join(path1, path2));
    }

    public static FileInfo Join(DirectoryInfo path1, string path2, string path3)
    {
        return new FileInfo(PathEx.Join(path1, path2, path3));
    }

    public static FileInfo Join(DirectoryInfo path1, string path2, string path3, string path4)
    {
        return new FileInfo(PathEx.Join(path1, path2, path3, path4));
    }
}
