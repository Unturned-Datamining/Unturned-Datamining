using System;
using System.Collections.Generic;
using System.IO;

namespace SDG.Unturned;

internal static class MasterBundleValidation
{
    private static byte[] tempWorkingHash = new byte[20];

    private static List<MasterBundleHash> eligibleBundleHashes;

    public static List<string> eligibleBundleNames { get; private set; }

    public static void initialize(List<MasterBundleConfig> allMasterBundles)
    {
        if (eligibleBundleHashes != null)
        {
            throw new NotSupportedException();
        }
        foreach (MasterBundleConfig allMasterBundle in allMasterBundles)
        {
            if (!allMasterBundle.doesHashFileExist)
            {
                UnturnedLog.info("Asset bundle \"" + allMasterBundle.assetBundleNameWithoutExtension + "\" does not have server hashes file");
                continue;
            }
            allMasterBundle.serverHashes = loadHashForBundle(allMasterBundle);
            if (allMasterBundle.serverHashes != null && !allMasterBundle.serverHashes.DoesAnyHashMatch(allMasterBundle.hash))
            {
                allMasterBundle.serverHashes = null;
                UnturnedLog.warn("Master bundle hash file does not match loaded: {0}", allMasterBundle.assetBundleName);
            }
        }
        eligibleBundleNames = new List<string>();
        eligibleBundleHashes = new List<MasterBundleHash>();
        if (!Provider.configData.Server.Validate_MasterBundle_Hashes)
        {
            UnturnedLog.info("Disabling master bundle hash validation");
            return;
        }
        for (int i = 0; i < allMasterBundles.Count; i++)
        {
            if (eligibleBundleNames.Count >= 12)
            {
                break;
            }
            MasterBundleConfig masterBundleConfig = allMasterBundles[i];
            if (masterBundleConfig.serverHashes != null)
            {
                eligibleBundleNames.Add(masterBundleConfig.assetBundleNameWithoutExtension);
                eligibleBundleHashes.Add(masterBundleConfig.serverHashes);
            }
        }
    }

    public static byte[] clientHandleRequest(IEnumerable<string> requestedNames)
    {
        List<byte> list = new List<byte>();
        foreach (string requestedName in requestedNames)
        {
            MasterBundleConfig masterBundleConfig = Assets.findMasterBundleByName(requestedName, matchExtension: false);
            if (masterBundleConfig != null)
            {
                if (masterBundleConfig.hash != null)
                {
                    list.AddRange(masterBundleConfig.hash);
                    continue;
                }
                UnturnedLog.warn("Missing hash for bundle \"{0}\" request", requestedName);
            }
            else
            {
                UnturnedLog.warn("Missing bundle \"{0}\" for hash request", requestedName);
            }
        }
        return list.ToArray();
    }

    public static bool serverHandleResponse(SteamPending player, byte[] clientHashes)
    {
        if (clientHashes.Length % 20 != 0)
        {
            Provider.reject(player.transportConnection, ESteamRejection.WRONG_HASH_MASTER_BUNDLE, "bad data");
            return false;
        }
        int num = clientHashes.Length / 20;
        if (num != eligibleBundleHashes.Count)
        {
            Provider.reject(player.transportConnection, ESteamRejection.WRONG_HASH_MASTER_BUNDLE, $"{num} of {eligibleBundleHashes.Count} installed");
            return false;
        }
        for (int i = 0; i < eligibleBundleHashes.Count; i++)
        {
            int num2 = i * 20;
            if (num2 + 20 > clientHashes.Length)
            {
                Provider.reject(player.transportConnection, ESteamRejection.WRONG_HASH_MASTER_BUNDLE, "out of bounds");
                return false;
            }
            Array.Copy(clientHashes, num2, tempWorkingHash, 0, 20);
            if (!eligibleBundleHashes[i].DoesPlatformHashMatch(tempWorkingHash, player.clientPlatform))
            {
                Provider.reject(player.transportConnection, ESteamRejection.WRONG_HASH_MASTER_BUNDLE, $"\"{eligibleBundleNames[i]}\" version mismatch");
                return false;
            }
        }
        return true;
    }

    private static MasterBundleHash loadHashForBundle(MasterBundleConfig bundle)
    {
        string hashFilePath = bundle.getHashFilePath();
        if (!File.Exists(hashFilePath))
        {
            UnturnedLog.warn("Master bundle hashes file was deleted: {0}", hashFilePath);
            return null;
        }
        byte[] array = File.ReadAllBytes(hashFilePath);
        if (array.Length < 1)
        {
            UnturnedLog.warn("Master bundle hashes file is empty: {0}", hashFilePath);
            return null;
        }
        int num;
        if (array.Length == 60)
        {
            num = 1;
        }
        else
        {
            num = array[0];
            if (num != 2)
            {
                UnturnedLog.warn("Master bundle hash file is an unknown version ({0}): {1}", num, hashFilePath);
                return null;
            }
            if (array.Length != 61)
            {
                UnturnedLog.warn("Master bundle hash file is the wrong size ({0}): {1}", array.Length, hashFilePath);
                return null;
            }
        }
        MasterBundleHash masterBundleHash = new MasterBundleHash();
        masterBundleHash.windowsHash = new byte[20];
        masterBundleHash.macHash = new byte[20];
        masterBundleHash.linuxHash = new byte[20];
        int num2 = 0;
        if (num > 1)
        {
            num2 = 1;
        }
        Array.Copy(array, num2, masterBundleHash.windowsHash, 0, 20);
        num2 += 20;
        Array.Copy(array, num2, masterBundleHash.linuxHash, 0, 20);
        num2 += 20;
        Array.Copy(array, num2, masterBundleHash.macHash, 0, 20);
        return masterBundleHash;
    }
}
