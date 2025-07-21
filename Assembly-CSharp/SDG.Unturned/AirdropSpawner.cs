using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Allows Unity events to call in airdrops.
/// </summary>
[AddComponentMenu("Unturned/Airdrop Spawner")]
public class AirdropSpawner : MonoBehaviour
{
    [Tooltip("Optional ID or GUID of spawn table asset to override cargo with when SpawnDefault is invoked.")]
    public string DefaultCargoSpawnTable;

    [Tooltip("If set, find spawnpoint node by name and call in airdrop there.")]
    public string SpawnpointName;

    [Tooltip("If true, select a random valid airdrop node and call in airdrop there.")]
    public bool UseRandomAirdropNode;

    public void SpawnDefault()
    {
        Spawn(DefaultCargoSpawnTable);
    }

    public void Spawn(string cargoSpawnTableId)
    {
        if (!Provider.isServer || EffectManager.isInstantiatingEffectForPreload)
        {
            return;
        }
        AirdropDevkitNode airdropDevkitNode = null;
        if (UseRandomAirdropNode)
        {
            airdropDevkitNode = LevelManager.GetRandomAirdropNode();
            if (airdropDevkitNode == null)
            {
                UnturnedLog.info("{0} unable to get a random airdrop node", base.transform.GetSceneHierarchyPath());
                return;
            }
        }
        SpawnAsset spawnAsset;
        if (!string.IsNullOrEmpty(cargoSpawnTableId))
        {
            if (!CachingBcAssetRef.TryParse(cargoSpawnTableId, EAssetType.SPAWN, out var result))
            {
                UnturnedLog.warn("{0} unable to parse cargo spawn table ID \"{1}\"", base.transform.GetSceneHierarchyPath(), cargoSpawnTableId);
                return;
            }
            spawnAsset = result.Get<SpawnAsset>();
            if (spawnAsset == null)
            {
                UnturnedLog.warn("{0} unable to find cargo spawn table \"{1}\"", base.transform.GetSceneHierarchyPath(), cargoSpawnTableId);
                return;
            }
        }
        else
        {
            if (airdropDevkitNode == null)
            {
                UnturnedLog.warn("{0} cargo spawn table required because UseAirdropNodes is false", base.transform.GetSceneHierarchyPath());
                return;
            }
            spawnAsset = airdropDevkitNode.GetCargoSpawnTableOrLogWarning();
            if (spawnAsset == null)
            {
                return;
            }
        }
        Vector3 position = base.transform.position;
        if (airdropDevkitNode != null)
        {
            position = airdropDevkitNode.transform.position;
        }
        else if (!string.IsNullOrEmpty(SpawnpointName))
        {
            Spawnpoint spawnpoint = SpawnpointSystemV2.Get().FindSpawnpoint(SpawnpointName);
            if (spawnpoint != null)
            {
                position = spawnpoint.transform.position;
            }
            else
            {
                UnturnedLog.warn("{0} unable to find spawnpoint \"{1}\"", base.transform.GetSceneHierarchyPath(), SpawnpointName);
            }
        }
        LevelManager.SpawnAirdrop(position, spawnAsset);
    }
}
