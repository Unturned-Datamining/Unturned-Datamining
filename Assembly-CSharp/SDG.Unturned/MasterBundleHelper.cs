using System.IO;

namespace SDG.Unturned;

public static class MasterBundleHelper
{
    public static string getConfigPath(string absoluteDirectory)
    {
        return absoluteDirectory + "/MasterBundle.dat";
    }

    public static bool containsMasterBundle(string absoluteDirectory)
    {
        return File.Exists(getConfigPath(absoluteDirectory));
    }

    public static string insertAssetBundleNameSuffix(string name, string suffix)
    {
        int num = name.IndexOf('.');
        if (num < 0)
        {
            return name + suffix;
        }
        return name.Insert(num, suffix);
    }

    public static string getLinuxAssetBundleName(string name)
    {
        return insertAssetBundleNameSuffix(name, "_linux");
    }

    public static string getMacAssetBundleName(string name)
    {
        return insertAssetBundleNameSuffix(name, "_mac");
    }

    public static string getHashFileName(string name)
    {
        return name + ".hash";
    }
}
