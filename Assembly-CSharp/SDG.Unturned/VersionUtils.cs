using System.Globalization;

namespace SDG.Unturned;

public static class VersionUtils
{
    /// <summary>
    /// Convert 32-bit version into 8-char string.
    /// String is advertised on server list for clients to filter their local map version.
    /// </summary>
    public static string binaryToHexadecimal(uint binaryVersion)
    {
        return binaryVersion.ToString("X8");
    }

    /// <summary>
    /// Parse 32-bit version from 8-char string.
    /// String is advertised on server list for clients to filter their local map version.
    /// </summary>
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
