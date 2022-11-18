using System;
using System.IO;
using SDG.Framework.Devkit;
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.IO.FormattedFiles.KeyValueTables;
using UnityEngine;

namespace SDG.Unturned;

public abstract class Asset : IDirtyable, IFormattedFileReadable, IFormattedFileWritable
{
    protected bool _isDirty;

    public string name;

    public ushort id;

    public Guid GUID;

    internal AssetOrigin origin;

    public string absoluteOriginFilePath;

    public bool requiredShaderUpgrade;

    public bool ignoreNPOT;

    public bool isDirty
    {
        get
        {
            return _isDirty;
        }
        set
        {
            if (isDirty != value)
            {
                _isDirty = value;
                if (isDirty)
                {
                    DirtyManager.markDirty(this);
                }
                else
                {
                    DirtyManager.markClean(this);
                }
            }
        }
    }

    [Obsolete("Replaced by AssetOrigin class")]
    public EAssetOrigin assetOrigin
    {
        get
        {
            if (origin == null)
            {
                return EAssetOrigin.MISC;
            }
            if (origin == Assets.coreOrigin || origin == Assets.legacyOfficialOrigin)
            {
                return EAssetOrigin.OFFICIAL;
            }
            if (origin.workshopFileId != 0L)
            {
                return EAssetOrigin.WORKSHOP;
            }
            return EAssetOrigin.MISC;
        }
        set
        {
        }
    }

    public MasterBundleConfig originMasterBundle { get; protected set; }

    public bool ignoreTextureReadable { get; protected set; }

    public byte[] hash { get; protected set; }

    internal virtual bool ShouldVerifyHash => true;

    public virtual string FriendlyName => name;

    public virtual EAssetType assetCategory => EAssetType.NONE;

    public virtual string getFilePath()
    {
        return absoluteOriginFilePath;
    }

    public void save()
    {
        string filePath = getFilePath();
        string directoryName = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
        using StreamWriter writer = new StreamWriter(filePath);
        IFormattedFileWriter writer2 = new KeyValueTableWriter(writer);
        write(writer2);
    }

    public virtual void read(IFormattedFileReader reader)
    {
        if (reader != null)
        {
            reader = reader.readObject();
            readAsset(reader);
        }
    }

    protected virtual void readAsset(IFormattedFileReader reader)
    {
        id = reader.readValue<ushort>("ID");
    }

    public virtual void write(IFormattedFileWriter writer)
    {
        writer.beginObject("Metadata");
        writer.writeValue("GUID", GUID);
        writer.writeValue("Type", GetType());
        writer.endObject();
        writer.beginObject("Asset");
        writeAsset(writer);
        writer.endObject();
    }

    protected virtual void writeAsset(IFormattedFileWriter writer)
    {
        writer.writeValue("ID", id);
    }

    public AssetReference<T> getReferenceTo<T>() where T : Asset
    {
        return new AssetReference<T>(GUID);
    }

    public virtual void clearHash()
    {
        hash = new byte[20];
    }

    public void appendHash(byte[] otherHash)
    {
        hash = Hash.combineSHA1Hashes(hash, otherHash);
    }

    public string GetTypeNameWithoutSuffix()
    {
        string text = GetType().Name;
        if (text.EndsWith("Asset"))
        {
            return text.Substring(0, text.Length - 5);
        }
        return text;
    }

    public string getTypeNameAndIdDisplayString()
    {
        return $"({GetTypeNameWithoutSuffix()}) {name} [{id}]";
    }

    public Asset()
    {
        name = GetType().Name;
    }

    public Asset(Bundle bundle, Local localization, byte[] hash)
    {
        if (bundle != null)
        {
            name = bundle.name;
        }
        else
        {
            name = "Asset_" + id;
        }
        if (bundle is MasterBundle masterBundle)
        {
            originMasterBundle = masterBundle.cfg;
        }
        id = 0;
        this.hash = hash;
    }

    public Asset(Bundle bundle, Data data, Local localization, ushort id)
    {
        if (bundle != null)
        {
            name = bundle.name;
        }
        else
        {
            name = "Asset_" + id;
        }
        if (bundle is MasterBundle masterBundle)
        {
            originMasterBundle = masterBundle.cfg;
        }
        this.id = id;
        if (data != null)
        {
            hash = data.hash;
            ignoreNPOT = data.has("Ignore_NPOT");
            ignoreTextureReadable = data.has("Ignore_TexRW");
        }
        else
        {
            hash = new byte[20];
        }
    }

    public override string ToString()
    {
        return id + " - " + name;
    }

    protected T LoadRedirectableAsset<T>(Bundle fromBundle, string defaultName, Data data, string key) where T : UnityEngine.Object
    {
        if (data.TryReadString(key, out var value))
        {
            int num = value.IndexOf(':');
            MasterBundleConfig masterBundleConfig;
            string text;
            if (num < 0)
            {
                masterBundleConfig = Assets.currentMasterBundle;
                text = value;
                if (masterBundleConfig == null || masterBundleConfig.assetBundle == null)
                {
                    Assets.reportError(this, "unable to load \"{0}\" without masterbundle", value);
                    return null;
                }
            }
            else
            {
                string text2 = value.Substring(0, num);
                masterBundleConfig = Assets.findMasterBundleByName(text2);
                text = value.Substring(num + 1);
                if (masterBundleConfig == null || masterBundleConfig.assetBundle == null)
                {
                    Assets.reportError(this, "unable to find masterbundle \"" + text2 + "\" when loading asset \"" + text + "\"");
                    return null;
                }
            }
            string text3 = masterBundleConfig.formatAssetPath(text);
            T val = masterBundleConfig.assetBundle.LoadAsset<T>(text3);
            if ((UnityEngine.Object)val == (UnityEngine.Object)null)
            {
                Assets.reportError(this, "failed to load asset \"" + text3 + "\" from \"" + masterBundleConfig.assetBundleName + "\" as " + typeof(T).Name);
            }
            return val;
        }
        return fromBundle.load<T>(defaultName);
    }

    protected T loadRequiredAsset<T>(Bundle fromBundle, string name) where T : UnityEngine.Object
    {
        T val = fromBundle.load<T>(name);
        if ((UnityEngine.Object)val == (UnityEngine.Object)null)
        {
            Assets.reportError(this, "missing '{0}' {1}", name, typeof(T).Name);
        }
        else if (typeof(T) == typeof(GameObject))
        {
            AssetValidation.searchGameObjectForErrors(this, val as GameObject);
        }
        return val;
    }

    protected void validateAnimation(Animation animComponent, string name)
    {
        if (animComponent.GetClip(name) == null)
        {
            Assets.reportError(this, "{0} missing animation clip '{1}'", animComponent.gameObject, name);
        }
    }
}
