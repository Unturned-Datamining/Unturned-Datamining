namespace SDG.Unturned;

/// <summary>
/// Hashes for Windows, Linux, and Mac asset bundles.
/// Only loaded on the dedicated server. Null otherwise.
/// </summary>
internal class MasterBundleHash
{
    public byte[] windowsHash;

    public byte[] macHash;

    public byte[] linuxHash;

    public byte[] GetPlatformHash(EClientPlatform clientPlatform)
    {
        return clientPlatform switch
        {
            EClientPlatform.Windows => windowsHash, 
            EClientPlatform.Mac => macHash, 
            EClientPlatform.Linux => linuxHash, 
            _ => null, 
        };
    }

    /// <summary>
    /// Does given hash match any of the platform hashes?
    /// </summary>
    public bool DoesAnyHashMatch(byte[] hash)
    {
        if (windowsHash == null || macHash == null || linuxHash == null)
        {
            return true;
        }
        if (!Hash.verifyHash(hash, windowsHash) && !Hash.verifyHash(hash, macHash))
        {
            return Hash.verifyHash(hash, linuxHash);
        }
        return true;
    }

    public bool DoesPlatformHashMatch(byte[] hash, EClientPlatform clientPlatform)
    {
        byte[] platformHash = GetPlatformHash(clientPlatform);
        if (platformHash == null)
        {
            return true;
        }
        return Hash.verifyHash(hash, platformHash);
    }
}
