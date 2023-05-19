using System;
using System.Globalization;
using UnityEngine;

namespace SDG.Unturned;

public static class UnturnedDatEx
{
    public static void ParseGuidOrLegacyId(this DatDictionary dictionary, string key, out Guid guid, out ushort legacyId)
    {
        if (dictionary.TryGetString(key, out var value) && !string.IsNullOrEmpty(value) && (value.Length != 1 || value[0] != '0'))
        {
            if (ushort.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out legacyId))
            {
                guid = Guid.Empty;
                return;
            }
            if (Guid.TryParse(value, out guid))
            {
                legacyId = 0;
                return;
            }
        }
        guid = Guid.Empty;
        legacyId = 0;
    }

    public static ushort ParseGuidOrLegacyId(this DatDictionary dictionary, string key, out Guid guid)
    {
        dictionary.ParseGuidOrLegacyId(key, out guid, out var legacyId);
        return legacyId;
    }

    public static AssetReference<T> readAssetReference<T>(this DatDictionary dictionary, string key) where T : Asset
    {
        if (dictionary.ContainsKey(key))
        {
            return new AssetReference<T>(dictionary.ParseGuid(key));
        }
        return AssetReference<T>.invalid;
    }

    public static AssetReference<T> readAssetReference<T>(this DatDictionary dictionary, string key, in AssetReference<T> defaultValue) where T : Asset
    {
        if (dictionary.ContainsKey(key))
        {
            return new AssetReference<T>(dictionary.ParseGuid(key));
        }
        return defaultValue;
    }

    private static void ParseMasterBundleReference(string key, string value, out string name, out string path, MasterBundleConfig defaultMasterBundle)
    {
        int num = value.IndexOf(':');
        if (num < 0)
        {
            if (defaultMasterBundle != null)
            {
                name = defaultMasterBundle.assetBundleName;
            }
            else
            {
                name = string.Empty;
                Assets.reportError("MasterBundleRef \"" + key + "\" is not associated with a master bundle nor does it specify one");
            }
            path = value;
            return;
        }
        name = value.Substring(0, num);
        path = value.Substring(num + 1);
        if (string.IsNullOrEmpty(name))
        {
            Assets.reportError("MasterBundleRef \"" + key + "\" specified asset bundle name is empty");
        }
        if (string.IsNullOrEmpty(path))
        {
            Assets.reportError("MasterBundleRef \"" + key + "\" specified asset path is empty");
        }
    }

    public static MasterBundleReference<T> readMasterBundleReference<T>(this DatDictionary dictionary, string key, MasterBundleConfig defaultMasterBundle = null) where T : UnityEngine.Object
    {
        if (dictionary.TryGetString(key, out var value))
        {
            ParseMasterBundleReference(key, value, out var name, out var path, defaultMasterBundle ?? Assets.currentMasterBundle);
            return new MasterBundleReference<T>(name, path);
        }
        return MasterBundleReference<T>.invalid;
    }

    public static MasterBundleReference<T> readMasterBundleReference<T>(this DatDictionary dictionary, string key, Bundle defaultBundle = null) where T : UnityEngine.Object
    {
        return dictionary.readMasterBundleReference<T>(key, (defaultBundle as MasterBundle)?.cfg);
    }

    public static AudioReference ReadAudioReference(this DatDictionary dictionary, string key, MasterBundleConfig defaultMasterBundle = null)
    {
        if (dictionary.TryGetString(key, out var value))
        {
            ParseMasterBundleReference(key, value, out var name, out var path, defaultMasterBundle ?? Assets.currentMasterBundle);
            return new AudioReference(name, path);
        }
        return default(AudioReference);
    }

    public static AudioReference ReadAudioReference(this DatDictionary dictionary, string key, Bundle defaultBundle = null)
    {
        return dictionary.ReadAudioReference(key, (defaultBundle as MasterBundle)?.cfg);
    }
}
