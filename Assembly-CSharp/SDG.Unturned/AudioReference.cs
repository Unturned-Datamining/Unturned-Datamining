using UnityEngine;

namespace SDG.Unturned;

public struct AudioReference
{
    private string assetBundleName;

    private string path;

    public bool IsNullOrEmpty
    {
        get
        {
            if (!string.IsNullOrEmpty(assetBundleName))
            {
                return string.IsNullOrEmpty(path);
            }
            return true;
        }
    }

    public AudioReference(string assetBundleName, string path)
    {
        this.assetBundleName = assetBundleName;
        this.path = path;
    }

    public AudioClip LoadAudioClip(out float volumeMultiplier, out float pitchMultiplier)
    {
        volumeMultiplier = 1f;
        pitchMultiplier = 1f;
        if (IsNullOrEmpty)
        {
            return null;
        }
        MasterBundleConfig masterBundleConfig = Assets.findMasterBundleByName(assetBundleName);
        if (masterBundleConfig == null || masterBundleConfig.assetBundle == null)
        {
            UnturnedLog.warn("Unable to find master bundle '{0}' when loading audio reference '{1}'", assetBundleName, path);
            return null;
        }
        string text = masterBundleConfig.formatAssetPath(path);
        if (text.EndsWith(".asset"))
        {
            OneShotAudioDefinition oneShotAudioDefinition = masterBundleConfig.assetBundle.LoadAsset<OneShotAudioDefinition>(text);
            if (oneShotAudioDefinition == null)
            {
                UnturnedLog.warn("Failed to load audio def '{0}' from master bundle '{1}'", text, assetBundleName);
                return null;
            }
            volumeMultiplier = oneShotAudioDefinition.volumeMultiplier;
            pitchMultiplier = Random.Range(oneShotAudioDefinition.minPitch, oneShotAudioDefinition.maxPitch);
            return oneShotAudioDefinition.GetRandomClip();
        }
        AudioClip audioClip = masterBundleConfig.assetBundle.LoadAsset<AudioClip>(text);
        if (audioClip == null)
        {
            UnturnedLog.warn("Failed to load audio clip '{0}' from master bundle '{1}'", text, assetBundleName);
        }
        return audioClip;
    }
}
