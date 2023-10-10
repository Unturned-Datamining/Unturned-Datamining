using System;
using UnityEngine;

namespace SDG.Unturned;

public abstract class Asset
{
    public string name;

    public ushort id;

    public Guid GUID;

    internal AssetOrigin origin;

    public string absoluteOriginFilePath;

    public bool requiredShaderUpgrade;

    public bool ignoreNPOT;

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

    public byte[] hash { get; internal set; }

    internal virtual bool ShouldVerifyHash => true;

    public virtual string FriendlyName => name;

    public virtual EAssetType assetCategory => EAssetType.NONE;

    protected bool OriginAllowsVanillaLegacyId
    {
        get
        {
            if (origin != Assets.coreOrigin)
            {
                return origin == Assets.reloadOrigin;
            }
            return true;
        }
    }

    public virtual string getFilePath()
    {
        return absoluteOriginFilePath;
    }

    public AssetReference<T> getReferenceTo<T>() where T : Asset
    {
        return new AssetReference<T>(GUID);
    }

    public string GetOriginName()
    {
        return origin?.name ?? "Unknown";
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

    public virtual void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
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
        if (data != null)
        {
            ignoreNPOT = data.ContainsKey("Ignore_NPOT");
            ignoreTextureReadable = data.ContainsKey("Ignore_TexRW");
        }
    }

    internal virtual void OnCreatedAtRuntime()
    {
    }

    public override string ToString()
    {
        return id + " - " + name;
    }

    protected T LoadRedirectableAsset<T>(Bundle fromBundle, string defaultName, DatDictionary data, string key) where T : UnityEngine.Object
    {
        if (data.TryGetString(key, out var value))
        {
            int num = value.IndexOf(':');
            MasterBundleConfig masterBundleConfig;
            string text;
            if (num < 0)
            {
                masterBundleConfig = ((fromBundle is MasterBundle masterBundle) ? masterBundle.cfg : Assets.currentMasterBundle);
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
