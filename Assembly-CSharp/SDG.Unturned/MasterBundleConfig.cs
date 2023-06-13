using System;
using System.IO;
using UnityEngine;

namespace SDG.Unturned;

public class MasterBundleConfig
{
    internal AssetOrigin origin;

    internal bool doesHashFileExist;

    internal MasterBundleHash serverHashes;

    internal AssetBundleCreateRequest assetBundleCreateRequest;

    private double loadStartTime;

    internal MasterBundleConfig sourceConfig;

    public string directoryPath { get; protected set; }

    public string assetBundleName { get; protected set; }

    public string assetBundleNameWithoutExtension { get; protected set; }

    public string assetPrefix { get; protected set; }

    public int version { get; protected set; }

    public AssetBundle assetBundle { get; protected set; }

    public byte[] hash { get; protected set; }

    public MasterBundleConfig(string absoluteDirectory, DatDictionary data, AssetOrigin origin)
    {
        directoryPath = absoluteDirectory;
        this.origin = origin;
        assetBundleName = data.GetString("Asset_Bundle_Name");
        if (string.IsNullOrEmpty(assetBundleName))
        {
            throw new Exception("Unspecified Asset_Bundle_Name! This should be the file name and extension of the master asset bundle exported from Unity.");
        }
        assetBundleNameWithoutExtension = Path.GetFileNameWithoutExtension(assetBundleName);
        assetPrefix = data.GetString("Asset_Prefix");
        if (string.IsNullOrEmpty(assetPrefix))
        {
            throw new Exception("Unspecified Asset_Prefix! This should be the portion of the Unity asset path prior to the /Bundles/ path, e.g. Assets/Bundles/");
        }
        if (data.ContainsKey("Master_Bundle_Version"))
        {
            version = data.ParseInt32("Master_Bundle_Version");
        }
        else
        {
            version = data.ParseInt32("Asset_Bundle_Version", 2);
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

    internal void CopyAssetBundleFromDuplicateConfig(MasterBundleConfig otherConfig)
    {
        sourceConfig = otherConfig;
        version = otherConfig.version;
        assetBundle = otherConfig.assetBundle;
        hash = otherConfig.hash;
        doesHashFileExist = otherConfig.doesHashFileExist;
        serverHashes = otherConfig.serverHashes;
        assetBundleCreateRequest = null;
        CheckOwnerCustomDataAndMaybeUnload();
    }

    public void StartLoad(byte[] inputData, byte[] inputHash)
    {
        UnturnedLog.info("Loading asset bundle \"" + assetBundleName + "\" from \"" + directoryPath + "\"...");
        assetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(inputData);
        hash = inputHash;
        loadStartTime = Time.realtimeSinceStartupAsDouble;
    }

    public void FinishLoad()
    {
        assetBundle = assetBundleCreateRequest.assetBundle;
        CheckOwnerCustomDataAndMaybeUnload();
        if (assetBundle != null)
        {
            double num = Time.realtimeSinceStartupAsDouble - loadStartTime;
            UnturnedLog.info($"Loading asset bundle \"{assetBundleName}\" from \"{directoryPath}\" took {num}s");
            return;
        }
        UnturnedLog.warn("Failed to load asset bundle \"" + assetBundleName + "\" from \"" + directoryPath + "\"");
    }

    public void unload()
    {
        if (sourceConfig != null)
        {
            assetBundle = null;
        }
        else if (assetBundle != null)
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
            return;
        }
        UnturnedLog.info("Loaded \"" + assetBundleName + "\" custom data from \"" + text + "\"");
        bool flag = assetBundleCustomData.ownerWorkshopFileIds != null && assetBundleCustomData.ownerWorkshopFileIds.Count > 0;
        if (origin.workshopFileId == 0 || !(assetBundleCustomData.ownerWorkshopFileId != 0 || flag) || origin.workshopFileId == assetBundleCustomData.ownerWorkshopFileId || (flag && assetBundleCustomData.ownerWorkshopFileIds.Contains(origin.workshopFileId)))
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
        UnturnedLog.warn($"Unloading \"{assetBundle}\" because source workshop file ID ({origin.workshopFileId}) does not match owner workshop file ID(s) ({text2})");
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
