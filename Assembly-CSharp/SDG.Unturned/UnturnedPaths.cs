using System.IO;
using Unturned.UnityEx;

namespace SDG.Unturned;

public static class UnturnedPaths
{
    /// <summary>
    /// Directory the game files are installed in. For the editor this is the /Builds/Shared directory.
    /// Windows and Linux: contains the executable and the Unturned_Data directory.
    /// MacOS: contains the Unturned.app bundle.
    /// </summary>
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
