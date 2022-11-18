using System;
using UnityEngine;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Effect Spawner")]
public class EffectSpawner : MonoBehaviour
{
    public string DefaultEffectAssetGuid;

    public bool AuthorityOnly;

    public bool Reliable;

    public Transform OverrideTransform;

    public void SpawnDefaultEffect()
    {
        SpawnEffect(DefaultEffectAssetGuid);
    }

    public void SpawnEffect(string guid)
    {
        if (!AuthorityOnly || Provider.isServer)
        {
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
            Transform transform = ((OverrideTransform == null) ? base.transform : OverrideTransform);
            parameters.position = transform.position;
            parameters.direction = transform.forward;
            EffectManager.triggerEffect(parameters);
        }
    }
}
