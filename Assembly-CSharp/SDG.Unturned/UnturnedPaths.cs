using System.IO;
using Unturned.UnityEx;

namespace SDG.Unturned;

public static class UnturnedPaths
{
    public static readonly DirectoryInfo RootDirectory;

    static UnturnedPaths()
    {
        if (UnityPaths.ProjectDirectory != null)
        {
            RootDirectory = UnityPaths.ProjectDirectory.CreateSubdirectory("Builds").CreateSubdirectory("Shared");
        }
        else
        {
            RootDirectory = UnityPaths.GameDirectory;
        }
    }
}
