using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

public class NPCEffectReward : INPCReward
{
    /// <summary>
    /// Should the RPC be called in reliable mode? Unreliable effects might not be received.
    /// </summary>
    public bool IsReliable;

    /// <summary>
    /// Applied if greater than zero. Defaults to 128.
    /// </summary>
    public float OverrideRelevantDistance;

    public AssetReference<EffectAsset> AssetRef { get; protected set; }

    public string Spawnpoint { get; protected set; }

    public override void GrantReward(Player player)
    {
        Spawnpoint spawnpoint = SpawnpointSystemV2.Get().FindSpawnpoint(Spawnpoint);
        if (spawnpoint != null)
        {
            Vector3 position = spawnpoint.transform.position;
            Quaternion rotation = spawnpoint.transform.rotation;
            TriggerEffectParameters parameters = new TriggerEffectParameters(AssetRef);
            parameters.shouldReplicate = true;
            parameters.reliable = IsReliable;
            if (OverrideRelevantDistance > 0.01f)
            {
                parameters.relevantDistance = OverrideRelevantDistance;
            }
            parameters.position = position;
            parameters.SetRotation(rotation);
            EffectManager.triggerEffect(parameters);
        }
        else
        {
            UnturnedLog.error("Failed to find NPC effect reward spawnpoint: " + Spawnpoint);
        }
    }

    public NPCEffectReward(AssetReference<EffectAsset> newAssetRef, string newSpawnpoint, bool newIsReliable, float newRelevantDistance, string newText)
        : base(newText)
    {
        AssetRef = newAssetRef;
        Spawnpoint = newSpawnpoint;
        IsReliable = newIsReliable;
        OverrideRelevantDistance = newRelevantDistance;
    }
}
