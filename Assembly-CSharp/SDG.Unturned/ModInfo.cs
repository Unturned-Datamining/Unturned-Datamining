namespace SDG.Unturned;

public class ModInfo
{
    /// <summary>
    /// Name shown in the main menu.
    /// </summary>
    public string Name;

    /// <summary>
    /// Author name shown in the main menu.
    /// </summary>
    public string Creators;

    public byte Major_Version;

    public byte Minor_Version;

    public byte Patch_Version;

    public string FormatModVersion()
    {
        return $"{Major_Version}.{Minor_Version}.{Patch_Version}";
    }

    public uint GetPackedVersion()
    {
        return (uint)((Major_Version << 16) | (Minor_Version << 8) | Patch_Version);
    }

    public string FormatServerListName()
    {
        string text = "Mod";
        string name = Name;
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (char.IsLetterOrDigit(c))
            {
                text += c;
            }
        }
        return text;
    }
}
