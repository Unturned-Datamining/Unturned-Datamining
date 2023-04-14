using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace SDG.Unturned;

public class MasterBundleConfig
{
    internal ulong workshopFileId;

    internal bool doesHashFileExist;

    internal MasterBundleHash serverHashes;

    public string directoryPath { get; protected set; }

    public string assetBundleName { get; protected set; }

    public string assetBundleNameWithoutExtension { get; protected set; }

    public string assetPrefix { get; protected set; }

    public int version { get; protected set; }

    public AssetBundle assetBundle { get; protected set; }

    public byte[] hash { get; protected set; }

    public MasterBundleConfig(string absoluteDirectory, ulong workshopFileId)
    {
        directoryPath = absoluteDirectory;
        this.workshopFileId = workshopFileId;
        DatDictionary datDictionary = ReadWrite.ReadDataWithoutHash(MasterBundleHelper.getConfigPath(absoluteDirectory));
        assetBundleName = datDictionary.GetString("Asset_Bundle_Name");
        if (string.IsNullOrEmpty(assetBundleName))
        {
            throw new Exception("Unspecified Asset_Bundle_Name! This should be the file name and extension of the master asset bundle exported from Unity.");
        }
        assetBundleNameWithoutExtension = Path.GetFileNameWithoutExtension(assetBundleName);
        assetPrefix = datDictionary.GetString("Asset_Prefix");
        if (string.IsNullOrEmpty(assetPrefix))
        {
            throw new Exception("Unspecified Asset_Prefix! This should be the portion of the Unity asset path prior to the /Bundles/ path, e.g. Assets/Bundles/");
        }
        if (datDictionary.ContainsKey("Master_Bundle_Version"))
        {
            version = datDictionary.ParseInt32("Master_Bundle_Version");
        }
        else
        {
            version = datDictionary.ParseInt32("Asset_Bundle_Version", 2);
        }
        if (version < 2)
        {
            throw new Exception("Lowest master bundle version is 2 (default), associated with 2017.4 LTS.");
        }
        if (version > 4)
        {
            throw new Exception("Highest master bundle version is 4, associated with 2020 LTS.");
        }
        string assetBundlePath = getAssetBundlePath();
        if (!File.Exists(assetBundlePath))
        {
            throw new Exception("Unable to find specified Asset_Bundle_Name next to the config file! Expected path: " + assetBundlePath);
        }
        doesHashFileExist = File.Exists(getHashFilePath());
    }

    public string getAssetBundlePath()
    {
        string linuxAssetBundleName = MasterBundleHelper.getLinuxAssetBundleName(assetBundleName);
        string text = Path.Combine(directoryPath, linuxAssetBundleName);
        if (File.Exists(text))
        {
            return text;
        }
        return Path.Combine(directoryPath, assetBundleName);
    }

    public string getHashFilePath()
    {
        return MasterBundleHelper.getHashFileName(Path.Combine(directoryPath, assetBundleName));
    }

    public string formatAssetPath(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPrefix))
        {
            return assetPath;
        }
        if (assetPrefix.EndsWith("/") || assetPath.StartsWith("/"))
        {
            return assetPrefix + assetPath;
        }
        return assetPrefix + "/" + assetPath;
    }

    public bool load()
    {
        using (FileStream underlyingStream = new FileStream(getAssetBundlePath(), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            using SHA1Stream sHA1Stream = new SHA1Stream(underlyingStream);
            assetBundle = AssetBundle.LoadFromStream(sHA1Stream);
            hash = sHA1Stream.Hash;
        }
        CheckOwnerCustomDataAndMaybeUnload();
        return assetBundle != null;
    }

    public IEnumerator loadAsync()
    {
        using (FileStream fs = new FileStream(getAssetBundlePath(), FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
        {
            using SHA1Stream hashStream = new SHA1Stream(fs);
            AssetBundleCreateRequest pendingAssetBundle = AssetBundle.LoadFromStreamAsync(hashStream);
            while (!pendingAssetBundle.isDone)
            {
                LoadingUI.notifyMasterBundleProgress("Master_Bundle_Progress", assetBundleNameWithoutExtension, pendingAssetBundle.progress);
                yield return null;
            }
            assetBundle = pendingAssetBundle.assetBundle;
            hash = hashStream.Hash;
        }
        CheckOwnerCustomDataAndMaybeUnload();
    }

    public void unload()
    {
        if (assetBundle != null)
        {
            assetBundle.Unload(unloadAllLoadedObjects: false);
            assetBundle = null;
        }
    }

    private void CheckOwnerCustomDataAndMaybeUnload()
    {
        if (assetBundle == null)
        {
            return;
        }
        string text = formatAssetPath("AssetBundleCustomData.asset");
        AssetBundleCustomData assetBundleCustomData = assetBundle.LoadAsset<AssetBundleCustomData>(text);
        if (assetBundleCustomData == null)
        {
            UnturnedLog.info("Tried loading \"" + assetBundleName + "\" optional custom data from \"" + text + "\"");
            return;
        }
        UnturnedLog.info("Loaded \"" + assetBundleName + "\" custom data from \"" + text + "\"");
        bool flag = assetBundleCustomData.ownerWorkshopFileIds != null && assetBundleCustomData.ownerWorkshopFileIds.Count > 0;
        if (workshopFileId == 0 || !(assetBundleCustomData.ownerWorkshopFileId != 0 || flag) || workshopFileId == assetBundleCustomData.ownerWorkshopFileId || (flag && assetBundleCustomData.ownerWorkshopFileIds.Contains(workshopFileId)))
        {
            return;
        }
        string text2;
        if (flag)
        {
            text2 = string.Join(", ", assetBundleCustomData.ownerWorkshopFileIds);
            if (assetBundleCustomData.ownerWorkshopFileId != 0)
            {
                text2 += ", ";
                text2 += assetBundleCustomData.ownerWorkshopFileId;
            }
        }
        else
        {
            text2 = assetBundleCustomData.ownerWorkshopFileId.ToString();
        }
        UnturnedLog.warn($"Unloading \"{assetBundle}\" because source workshop file ID ({workshopFileId}) does not match owner workshop file ID(s) ({text2})");
        unload();
    }

    public AssetBundleRequest LoadAssetAsync<T>(string name) where T : UnityEngine.Object
    {
        string name2 = formatAssetPath(name);
        return assetBundle.LoadAssetAsync<T>(name2);
    }

    public override string ToString()
    {
        return $"{assetBundleNameWithoutExtension} in {directoryPath}";
    }
}
