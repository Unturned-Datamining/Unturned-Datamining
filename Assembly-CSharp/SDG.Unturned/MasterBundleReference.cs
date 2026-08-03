using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Essentially identical to ContentReference, but MasterBundle is more convenient.
/// Perhaps in the future all asset/content systems will be consolidated.
/// </summary>
public struct MasterBundleReference<T> : IFormattedFileReadable, IFormattedFileWritable, IDatParseable where T : Object
{
    public static MasterBundleReference<T> invalid = new MasterBundleReference<T>(null, null);

    /// <summary>
    /// Name of master bundle file.
    /// </summary>
    public string name;

    /// <summary>
    /// Path to Unity asset within asset bundle.
    /// </summary>
    public string path;

    /// <summary>
    /// Are name or path null or empty?
    /// </summary>
    public bool isNull
    {
        get
        {
            if (!string.IsNullOrEmpty(name))
            {
                return string.IsNullOrEmpty(path);
            }
            return true;
        }
    }

    /// <summary>
    /// Are both name and path non-null and non-empty?
    /// </summary>
    public bool isValid => !isNull;

    public MasterBundleReference(string name, string path)
    {
        this.name = name;
        this.path = path;
    }

    public bool TryParse(IDatNode node)
    {
        if (node is IDatValue datValue)
        {
            if (string.IsNullOrEmpty(datValue.Value))
            {
                return false;
            }
            if (datValue.Value.Length < 2)
            {
                return false;
            }
            int num = datValue.Value.IndexOf(':');
            if (num < 0)
            {
                if (Assets.currentMasterBundle != null)
                {
                    name = Assets.currentMasterBundle.assetBundleName;
                }
                path = datValue.Value;
            }
            else
            {
                name = datValue.Value.Substring(0, num);
                path = datValue.Value.Substring(num + 1);
            }
            return true;
        }
        if (node is IDatDictionary dictionary)
        {
            name = dictionary.GetString("MasterBundle");
            path = dictionary.GetString("AssetPath");
            return true;
        }
        return false;
    }

    public void read(IFormattedFileReader reader)
    {
        IFormattedFileReader formattedFileReader = reader.readObject();
        if (formattedFileReader == null)
        {
            if (Assets.currentMasterBundle != null)
            {
                name = Assets.currentMasterBundle.assetBundleName;
            }
            path = reader.readValue();
        }
        else
        {
            name = formattedFileReader.readValue("MasterBundle");
            path = formattedFileReader.readValue("AssetPath");
        }
    }

    public void write(IFormattedFileWriter writer)
    {
        writer.beginObject();
        writer.writeValue("MasterBundle", name);
        writer.writeValue("AssetPath", path);
        writer.endObject();
    }

    public T loadAsset(bool logWarnings = true)
    {
        T val;
        if (isNull)
        {
            val = null;
        }
        else
        {
            MasterBundleConfig masterBundleConfig = Assets.findMasterBundleByName(name);
            if (masterBundleConfig == null || masterBundleConfig.assetBundle == null)
            {
                if (logWarnings)
                {
                    UnturnedLog.warn("Unable to find master bundle '{0}' when loading asset '{1}' as {2}", name, path, typeof(T).Name);
                }
                val = null;
            }
            else
            {
                string text = masterBundleConfig.FormatAssetPathAndCache(path);
                val = masterBundleConfig.assetBundle.LoadAsset<T>(text);
                if (val != null)
                {
                    if (val is GameObject gameObject)
                    {
                        Bundle.FixupGameObjectAudio(gameObject);
                        if (!StaticUnityEventPrevention.Validate(gameObject))
                        {
                            UnturnedLog.warn("Canceling load asset '{0}' from master bundle '{1}' because it failed UnityEvent checks", text, name);
                            val = null;
                        }
                    }
                }
                else if (logWarnings)
                {
                    UnturnedLog.warn("Failed to load asset '{0}' from master bundle '{1}' as {2}", text, name, typeof(T).Name);
                }
            }
        }
        return val;
    }

    /// <summary>
    /// TODO: if adding additional calls, result should ideally wrap AssetBundleRequest so that
    /// bundle.processLoadedObject runs before returning the result. Should be consolidated with
    /// MasterBundleConfig.LoadAssetAsync, too.
    /// </summary>
    public AssetBundleRequest LoadAssetAsync(bool logWarnings = true)
    {
        if (isNull)
        {
            return null;
        }
        MasterBundleConfig masterBundleConfig = Assets.findMasterBundleByName(name);
        if (masterBundleConfig == null || masterBundleConfig.assetBundle == null)
        {
            if (logWarnings)
            {
                UnturnedLog.warn("Unable to find master bundle '{0}' when async loading asset '{1}' as {2}", name, path, typeof(T).Name);
            }
            return null;
        }
        string text = masterBundleConfig.FormatAssetPathAndCache(path);
        return masterBundleConfig.assetBundle.LoadAssetAsync<T>(text);
    }

    public override string ToString()
    {
        return $"{name}:{path}";
    }
}
