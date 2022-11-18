using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class MasterBundle : Bundle
{
    private static Dictionary<Type, string[]> typeExtensions = new Dictionary<Type, string[]>
    {
        {
            typeof(Material),
            new string[1] { ".mat" }
        },
        {
            typeof(Texture2D),
            new string[2] { ".png", ".jpg" }
        },
        {
            typeof(GameObject),
            new string[1] { ".prefab" }
        },
        {
            typeof(AudioClip),
            new string[3] { ".wav", ".ogg", ".mp3" }
        }
    };

    public MasterBundleConfig cfg { get; protected set; }

    public string relativePath { get; protected set; }

    protected override bool willBeUnloadedDuringUse => false;

    public override void loadDeferred<T>(string name, out IDeferredAsset<T> asset, LoadedAssetDeferredCallback<T> callback)
    {
        if (Assets.shouldDeferLoadingAssets)
        {
            DeferredMasterAsset<T> deferredMasterAsset = default(DeferredMasterAsset<T>);
            deferredMasterAsset.masterBundle = this;
            deferredMasterAsset.name = name;
            deferredMasterAsset.callback = callback;
            asset = deferredMasterAsset;
        }
        else
        {
            base.loadDeferred(name, out asset, callback);
        }
    }

    public override T load<T>(string name)
    {
        if (cfg.assetBundle == null)
        {
            UnturnedLog.warn("Failed to load '{0}' from master bundle '{1}' because asset bundle was null", name, cfg.assetBundleName);
            return null;
        }
        string text = cfg.formatAssetPath(relativePath + "/" + name);
        if (!typeExtensions.TryGetValue(typeof(T), out var value))
        {
            UnturnedLog.warn("Unknown extension for type: " + typeof(T));
            return null;
        }
        string[] array = value;
        foreach (string text2 in array)
        {
            T val = cfg.assetBundle.LoadAsset<T>(text + text2);
            if ((UnityEngine.Object)val != (UnityEngine.Object)null)
            {
                processLoadedObject(val);
                return val;
            }
        }
        return null;
    }

    public MasterBundle(MasterBundleConfig cfg, string relativePath, string name)
        : base(name)
    {
        this.cfg = cfg;
        this.relativePath = relativePath;
    }
}
