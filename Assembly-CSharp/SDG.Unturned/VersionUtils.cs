using System.Globalization;

namespace SDG.Unturned;

public static class VersionUtils
{
    public static string binaryToHexadecimal(uint binaryVersion)
    {
        return binaryVersion.ToString("X8");
    }

    public static bool hexadecimalToBinary(string hexadecimalVersion, out uint binaryVersion)
    {
        if (string.IsNullOrEmpty(hexadecimalVersion) || hexadecimalVersion.Length != 8)
        {
            binaryVersion = 0u;
            return false;
        }
        return uint.TryParse(hexadecimalVersion, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out binaryVersion);
    }
}
