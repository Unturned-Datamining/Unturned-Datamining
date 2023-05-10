using System.IO;

namespace Unturned.SystemEx;

public static class PathEx
{
    public static string Join(DirectoryInfo path1, string path2)
    {
        return Path.Combine(path1.FullName, path2);
    }

    public static string Join(DirectoryInfo path1, string path2, string path3)
    {
        return Path.Combine(path1.FullName, path2, path3);
    }

    public static string Join(DirectoryInfo path1, string path2, string path3, string path4)
    {
        return Path.Combine(path1.FullName, path2, path3, path4);
    }

    public static string Join(DirectoryInfo path1, string path2, string path3, string path4, string path5)
    {
        return Path.Combine(path1.FullName, path2, path3, path4, path5);
    }

    public static string Join(DirectoryInfo path1, string path2, string path3, string path4, string path5, string path6)
    {
        return Path.Combine(path1.FullName, path2, path3, path4, path5, path6);
    }
}
