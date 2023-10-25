using System;
using System.Collections.Generic;
using System.IO;

namespace SDG.Unturned;

/// <summary>
/// Compares client asset bundle hash with server known hashes.
/// </summary>
internal static class MasterBundleValidation
{
    /// <summary>
    /// Called by asset startup to cache which bundles are eligible for hashing.
    /// </summary>
    public static void initialize(List<MasterBundleConfig> allMasterBundles)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            throw new NotSupportedException("MasterBundleValidation should only be used on dedicated server!");
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
    }

    private static MasterBundleHash loadHashForBundle(MasterBundleConfig bundle)
    {
        if (bundle.sourceConfig != null)
        {
            bundle = bundle.sourceConfig;
        }
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
