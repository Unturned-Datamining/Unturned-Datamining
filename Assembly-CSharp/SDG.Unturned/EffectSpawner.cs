using System;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Allows Unity events to spawn effects.
/// </summary>
[AddComponentMenu("Unturned/Effect Spawner")]
public class EffectSpawner : MonoBehaviour
{
    /// <summary>
    /// GUID of effect asset to spawn when SpawnDefaultEffect is invoked.
    /// </summary>
    public string DefaultEffectAssetGuid;

    /// <summary>
    /// If true the server will spawn the effect and replicate it to clients,
    /// otherwise clients will predict their own local copy.
    /// </summary>
    public bool AuthorityOnly;

    /// <summary>
    /// Should the RPC be called in reliable mode? Unreliable effects might not be received.
    /// </summary>
    public bool Reliable;

    /// <summary>
    /// Transform to spawn the effect at.
    /// If unset this game object's transform will be used instead.
    /// </summary>
    public Transform OverrideTransform;

    /// <summary>
    /// Applied if greater than zero. Defaults to 128.
    /// </summary>
    public float OverrideRelevantDistance;

    public void SpawnDefaultEffect()
    {
        SpawnEffect(DefaultEffectAssetGuid);
    }

    public void SpawnEffect(string guid)
    {
        if (AuthorityOnly && !Provider.isServer)
        {
            return;
        }
        if (!Guid.TryParse(guid, out var result))
        {
            UnturnedLog.warn("{0} unable to parse effect asset guid \"{1}\"", base.transform.GetSceneHierarchyPath(), guid);
            return;
        }
        if (!(Assets.find(result) is EffectAsset asset))
        {
            UnturnedLog.warn("{0} unable to find effect asset with guid \"{1}\"", base.transform.GetSceneHierarchyPath(), guid);
            return;
        }
        TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
        parameters.shouldReplicate = AuthorityOnly;
        parameters.reliable = Reliable;
        if (OverrideRelevantDistance > 0.01f)
        {
            parameters.relevantDistance = OverrideRelevantDistance;
        }
        Transform transform = ((OverrideTransform == null) ? base.transform : OverrideTransform);
        parameters.position = transform.position;
        parameters.SetRotation(transform.rotation);
        EffectManager.triggerEffect(parameters);
    }
}
