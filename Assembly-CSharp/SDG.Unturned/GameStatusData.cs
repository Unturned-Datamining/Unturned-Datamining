namespace SDG.Unturned;

public class GameStatusData
{
    public byte Major_Version;

    public byte Minor_Version;

    public byte Patch_Version;

    public int[] GrantPackageIDs;

    public string GrantPackageURL;

    public string FormatApplicationVersion()
    {
        return $"3.{Major_Version}.{Minor_Version}.{Patch_Version}";
    }
}
