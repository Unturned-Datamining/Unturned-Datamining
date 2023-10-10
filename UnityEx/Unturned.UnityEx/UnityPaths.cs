using System.IO;
using UnityEngine;

namespace Unturned.UnityEx;

public static class UnityPaths
{
    public static readonly DirectoryInfo ProjectDirectory;

    public static readonly DirectoryInfo AssetsDirectory;

    public static readonly DirectoryInfo TempDirectory;

    public static readonly DirectoryInfo LibraryDirectory;

    public static readonly DirectoryInfo GameDirectory;

    public static readonly DirectoryInfo GameDataDirectory;

    static UnityPaths()
    {
        GameDataDirectory = new DirectoryInfo(Application.dataPath);
        GameDirectory = GameDataDirectory.Parent;
    }
}
