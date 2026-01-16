using System;
using System.Text;
using UnityEngine;

namespace SDG.Unturned;

public abstract class Asset : IAssetErrorContext
{
    public string name;

    public ushort id;

    public Guid GUID;

    internal AssetOrigin origin;

    /// <summary>
    /// If true, an asset with the same ID or GUID has been added to the current asset mapping, replacing this one.
    /// </summary>
    internal bool hasBeenReplaced;

    /// <summary>
    /// Null or empty if created at runtime, otherwise set by <see cref="T:SDG.Unturned.Assets" /> when loading.
    /// </summary>
    public string absoluteOriginFilePath;

    /// <summary>
    /// Were this asset's shaders set to Standard and/or consolidated?
    /// Needed for vehicle rotors special case.
    /// </summary>
    public bool requiredShaderUpgrade;

    /// <summary>
    /// Should texture non-power-of-two warnings be ignored?
    /// Unfortunately some already-included third-party assets have NPOT textures.
    /// </summary>
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

    /// <summary>
    /// If true, errors related to this asset were reported during loading.
    /// </summary>
    public bool HasErrors { get; internal set; }

    /// <summary>
    /// Contents of file this asset was loaded from. Only kept if data re-saving is enabled. (So that this memory
    /// is collected after populating the asset.)
    /// </summary>
    public IDatDictionary OriginParsedData { get; set; }

    /// <summary>
    /// Translation data associated with this asset. Only kept if per-asset property
    /// "Keep_Localization_Loaded" is true.
    /// (Otherwise, memory is collected after populating the asset.)
    /// Nelson 2025-11-07: hacking this in so that NPC hints replicated from the server don't
    /// use the server's language.
    /// </summary>
    public Local Localization { get; set; }

    /// <summary>
    /// Master bundle this asset loaded from.
    /// </summary>
    public MasterBundleConfig originMasterBundle { get; protected set; }

    /// <summary>
    /// Should read/write texture warnings be ignored?
    /// </summary>
    public bool ignoreTextureReadable { get; protected set; }

    /// <summary>
    /// Hash of the original input file.
    /// </summary>
    public byte[] hash { get; internal set; }

    internal virtual bool ShouldVerifyHash => true;

    public virtual string FriendlyName => name;

    public virtual EAssetType assetCategory => EAssetType.NONE;

    public string AssetErrorPrefix => $"{GetOriginName()} {FriendlyName} ({GetTypeFriendlyName()}) [{GUID:N}]";

    /// <summary>
    /// e.g. Canned Beans (Consumeable Item)
    /// </summary>
    public string FriendlyNameWithFriendlyType => FriendlyName + " (" + GetTypeFriendlyName() + ")";

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

    /// <summary>
    /// Maybe temporary? Used when something in-game changes the asset so that it shouldn't be useable on the server anymore.
    /// </summary>
    public virtual void clearHash()
    {
        hash = new byte[20];
    }

    public void appendHash(byte[] otherHash)
    {
        hash = Hash.combineSHA1Hashes(hash, otherHash);
    }

    public void ReportAssetError(string message)
    {
        Assets.ReportError(this, message);
    }

    /// <summary>
    /// Most asset classes end in "Asset", so in debug strings if asset is clear from context we can remove the unnecessary suffix.
    /// </summary>
    public string GetTypeNameWithoutSuffix()
    {
        string text = GetType().Name;
        if (text.EndsWith("Asset"))
        {
            return text.Substring(0, text.Length - 5);
        }
        return text;
    }

    /// <summary>
    /// Remove "Asset" suffix and convert to title case.
    /// </summary>
    public virtual string GetTypeFriendlyName()
    {
        string typeNameWithoutSuffix = GetTypeNameWithoutSuffix();
        StringBuilder stringBuilder = new StringBuilder(32);
        for (int i = 0; i < typeNameWithoutSuffix.Length; i++)
        {
            char c = typeNameWithoutSuffix[i];
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(typeNameWithoutSuffix[i - 1]))
            {
                stringBuilder.Append(' ');
            }
            stringBuilder.Append(c);
        }
        return stringBuilder.ToString();
    }

    public string getTypeNameAndIdDisplayString()
    {
        return $"({GetTypeFriendlyName()}) {name} [{id}]";
    }

    public Asset()
    {
        name = GetType().Name;
    }

    public virtual void PopulateAsset(in PopulateAssetParameters p)
    {
        if (p.bundle != null)
        {
            name = p.bundle.name;
        }
        else
        {
            name = "Asset_" + id;
        }
        if (p.bundle is MasterBundle masterBundle)
        {
            originMasterBundle = masterBundle.cfg;
        }
        if (p.data != null)
        {
            ignoreNPOT = p.data.ContainsKey("Ignore_NPOT");
            ignoreTextureReadable = p.data.ContainsKey("Ignore_TexRW");
        }
    }

    internal virtual void PreResaveAsset(IDatDictionary data)
    {
    }

    internal virtual void BuildCargoData(CargoBuilder builder)
    {
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Asset");
        orAddDeclaration.Append("GUID", GUID);
        if (id > 0)
        {
            orAddDeclaration.Append("ID", id);
        }
        orAddDeclaration.Append("Filename", name);
        if (originMasterBundle != null)
        {
            orAddDeclaration.Append("MasterBundle", originMasterBundle.assetBundleNameWithoutExtension);
        }
        orAddDeclaration.Append("Origin", GetOriginName());
        orAddDeclaration.Append("Type", GetTypeFriendlyName());
    }

    /// <summary>
    /// Perform any initialization required when PopulateAsset won't be called.
    /// </summary>
    internal virtual void OnCreatedAtRuntime()
    {
    }

    public override string ToString()
    {
        return id + " - " + name;
    }

    /// <summary>
    /// Planning ahead to potentially convert the game to use Unity's newer Addressables feature.
    /// </summary>
    protected T LoadRedirectableAsset<T>(Bundle fromBundle, string defaultName, IDatDictionary data, string key) where T : UnityEngine.Object
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
                    Assets.ReportError(this, "unable to load \"{0}\" without masterbundle", value);
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
                    Assets.ReportError(this, "unable to find masterbundle \"" + text2 + "\" when loading asset \"" + text + "\"");
                    return null;
                }
            }
            string text3 = masterBundleConfig.formatAssetPath(text);
            T val = masterBundleConfig.assetBundle.LoadAsset<T>(text3);
            if (val == null)
            {
                Assets.ReportError(this, "failed to load asset \"" + text3 + "\" from \"" + masterBundleConfig.assetBundleName + "\" as " + typeof(T).Name);
            }
            return val;
        }
        return fromBundle.load<T>(defaultName);
    }

    internal T loadRequiredAsset<T>(Bundle fromBundle, string name) where T : UnityEngine.Object
    {
        T val = fromBundle.load<T>(name);
        if (val == null)
        {
            Assets.ReportError(this, "missing \"" + name + "\" " + typeof(T).Name + " (expected at " + fromBundle.WhereLoadLookedToString(name) + ")");
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
            Assets.ReportError(this, "{0} missing animation clip '{1}'", animComponent.gameObject, name);
        }
    }
}
