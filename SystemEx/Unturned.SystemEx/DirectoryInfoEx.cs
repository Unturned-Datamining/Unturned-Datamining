using System.Collections.Generic;
using System.IO;

namespace Unturned.SystemEx;

public static class DirectoryInfoEx
{
    public static bool ContainsAnything(this DirectoryInfo directory)
    {
        using IEnumerator<FileSystemInfo> enumerator = directory.EnumerateFileSystemInfos().GetEnumerator();
        return enumerator.MoveNext();
    }

    public static bool IsEmpty(this DirectoryInfo directory)
    {
        return !directory.ContainsAnything();
    }

    public static DirectoryInfo Join(DirectoryInfo path1, string path2)
    {
        return new DirectoryInfo(PathEx.Join(path1, path2));
    }

    public static DirectoryInfo Join(DirectoryInfo path1, string path2, string path3)
    {
        return new DirectoryInfo(PathEx.Join(path1, path2, path3));
    }

    public static DirectoryInfo Join(DirectoryInfo path1, string path2, string path3, string path4)
    {
        return new DirectoryInfo(PathEx.Join(path1, path2, path3, path4));
    }
}
