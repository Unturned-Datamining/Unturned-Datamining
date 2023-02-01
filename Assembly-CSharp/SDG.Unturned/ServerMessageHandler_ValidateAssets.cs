using System;
using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned;

internal static class ServerMessageHandler_ValidateAssets
{
    internal static readonly NetLength MAX_ASSETS = new NetLength(7u);

    private static byte[] clientHash = new byte[20];

    internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
    {
        SteamPlayer steamPlayer = Provider.findPlayer(transportConnection);
        if (steamPlayer == null)
        {
            return;
        }
        if (!reader.ReadBits(MAX_ASSETS.bitCount, out var value))
        {
            Provider.kick(steamPlayer.playerID.steamID, "ValidateAssets unable to read itemCountBits");
            return;
        }
        int num = (int)value;
        if (num > MAX_ASSETS.value)
        {
            Provider.kick(steamPlayer.playerID.steamID, "ValidateAssets invalid itemCount");
            return;
        }
        num++;
        if (!reader.ReadBits(num, out var value2))
        {
            Provider.kick(steamPlayer.playerID.steamID, "ValidateAssets unable to read hasHashFlags");
            return;
        }
        for (int i = 0; i < num; i++)
        {
            if (!reader.ReadGuid(out var value3))
            {
                Provider.kick(steamPlayer.playerID.steamID, "ValidateAssets unable to read guid");
                break;
            }
            if (value3 == Guid.Empty)
            {
                Provider.kick(steamPlayer.playerID.steamID, "ValidateAssets empty guid");
                break;
            }
            if (!steamPlayer.validatedGuids.Add(value3))
            {
                Provider.kick(steamPlayer.playerID.steamID, "ValidateAssets duplicate guid");
                break;
            }
            bool flag = (value2 & (uint)(1 << i)) != 0;
            if (flag && !reader.ReadBytes(clientHash))
            {
                Provider.kick(steamPlayer.playerID.steamID, "ValidateAssets unable to read clientHash");
                break;
            }
            if (ClientAssetIntegrity.serverKnownMissingGuids.Contains(value3))
            {
                continue;
            }
            Asset asset = Assets.find(value3);
            if (asset == null)
            {
                UnturnedLog.info(string.Format("Kicking {0} for invalid file integrity request guid: {1}", transportConnection, value3.ToString("N")));
                Assets.SendKickForInvalidGuid.Invoke(ENetReliability.Reliable, transportConnection, value3);
                Provider.dismiss(steamPlayer.playerID.steamID);
                break;
            }
            if (!asset.ShouldVerifyHash)
            {
                continue;
            }
            if (flag)
            {
                byte[] array = asset.hash;
                if (asset.originMasterBundle != null && asset.originMasterBundle.serverHashes != null)
                {
                    byte[] platformHash = asset.originMasterBundle.serverHashes.GetPlatformHash(steamPlayer.clientPlatform);
                    if (platformHash != null)
                    {
                        array = Hash.combine(array, platformHash);
                    }
                }
                if (!Hash.verifyHash(clientHash, array))
                {
                    string text = asset.origin?.name;
                    if (string.IsNullOrEmpty(text))
                    {
                        text = "Unknown";
                    }
                    UnturnedLog.info(string.Format("Kicking {0} for asset hash mismatch: \"{1}\" Type: {2} File: \"{3}\" Id: {4} Client: {5} Server: {6}", transportConnection, asset.FriendlyName, asset.GetTypeNameWithoutSuffix(), asset.name, value3.ToString("N"), Hash.toString(clientHash), Hash.toString(array)));
                    Assets.SendKickForHashMismatch.Invoke(ENetReliability.Reliable, transportConnection, value3, asset.name, asset.FriendlyName, array, asset.originMasterBundle?.assetBundleNameWithoutExtension, text);
                    Provider.dismiss(steamPlayer.playerID.steamID);
                    break;
                }
            }
            else if (asset.hash != null && asset.hash.Length == 20)
            {
                Provider.kick(steamPlayer.playerID.steamID, "missing asset: \"" + asset.FriendlyName + "\" File: \"" + asset.name + "\" Id: " + value3.ToString("N"));
                break;
            }
        }
    }
}
