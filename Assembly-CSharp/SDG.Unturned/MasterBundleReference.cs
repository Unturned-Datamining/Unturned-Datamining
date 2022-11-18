using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned;

public struct MasterBundleReference<T> : IFormattedFileReadable, IFormattedFileWritable where T : Object
{
    public static MasterBundleReference<T> invalid = new MasterBundleReference<T>(null, null);

    public string name;

    public string path;

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

    public bool isValid => !isNull;

    public MasterBundleReference(string name, string path)
    {
        this.name = name;
        this.path = path;
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
        if (isNull)
        {
            return null;
        }
        MasterBundleConfig masterBundleConfig = Assets.findMasterBundleByName(name);
        if (masterBundleConfig == null || masterBundleConfig.assetBundle == null)
        {
            if (logWarnings)
            {
                UnturnedLog.warn("Unable to find master bundle '{0}' when loading asset '{1}' as {2}", name, path, typeof(T).Name);
            }
            return null;
        }
        string text = masterBundleConfig.formatAssetPath(path);
        T val = masterBundleConfig.assetBundle.LoadAsset<T>(text);
        if ((Object)val == (Object)null && logWarnings)
        {
            UnturnedLog.warn("Failed to load asset '{0}' from master bundle '{1}' as {2}", text, name, typeof(T).Name);
        }
        return val;
    }

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
        string text = masterBundleConfig.formatAssetPath(path);
        return masterBundleConfig.assetBundle.LoadAssetAsync<T>(text);
    }

    public override string ToString()
    {
        return $"{name}:{path}";
    }
}
