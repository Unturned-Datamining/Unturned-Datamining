using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

public class NPCAirdropReward : INPCReward
{
    private CachingBcAssetRef _cargoSpawnTableRef;

    public CachingBcAssetRef CargoSpawnTableRef
    {
        get
        {
            return _cargoSpawnTableRef;
        }
        protected set
        {
            _cargoSpawnTableRef = value;
        }
    }

    public string spawnpoint { get; protected set; }

    public bool ShouldUseRandomAirdropNode { get; set; }

    public override void GrantReward(Player player)
    {
        AirdropDevkitNode airdropDevkitNode = null;
        if (ShouldUseRandomAirdropNode)
        {
            airdropDevkitNode = LevelManager.GetRandomAirdropNode();
            if (airdropDevkitNode == null)
            {
                UnturnedLog.info("NPC airdrop reward unable to get a random airdrop node");
                return;
            }
        }
        SpawnAsset spawnAsset = _cargoSpawnTableRef.Get<SpawnAsset>();
        if (spawnAsset == null)
        {
            if (airdropDevkitNode == null)
            {
                CachingBcAssetRef cargoSpawnTableRef = _cargoSpawnTableRef;
                UnturnedLog.error("Failed to find NPC airdrop reward cargo spawn asset: " + cargoSpawnTableRef.ToString());
                return;
            }
            spawnAsset = airdropDevkitNode.GetCargoSpawnTableOrLogWarning();
            if (spawnAsset == null)
            {
                return;
            }
        }
        Vector3 position;
        if (airdropDevkitNode != null)
        {
            position = airdropDevkitNode.transform.position;
        }
        else
        {
            Spawnpoint spawnpoint = SpawnpointSystemV2.Get().FindFirstSpawnpoint(this.spawnpoint);
            if (spawnpoint != null)
            {
                position = spawnpoint.transform.position;
            }
            else
            {
                UnturnedLog.error("Failed to find NPC airdrop reward spawnpoint: " + this.spawnpoint);
                position = player.transform.position;
            }
        }
        LevelManager.SpawnAirdrop(position, spawnAsset);
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        ShouldUseRandomAirdropNode = p.data.ParseBool("Use_Random_Airdrop_Node");
        if (!p.data.TryParseBcAssetRef("Cargo", EAssetType.SPAWN, out _cargoSpawnTableRef) && !ShouldUseRandomAirdropNode)
        {
            p.ReportRequiredOptionInvalid("Cargo");
        }
        if (!ShouldUseRandomAirdropNode)
        {
            if (p.data.TryGetString("Spawnpoint", out var value))
            {
                spawnpoint = value;
            }
            else
            {
                p.ReportRequiredOptionInvalid("Spawnpoint");
            }
        }
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        ShouldUseRandomAirdropNode = p.data.ParseBool(p.legacyPrefix + "_Use_Random_Airdrop_Node");
        if (!p.data.TryParseBcAssetRef(p.legacyPrefix + "_Cargo", EAssetType.SPAWN, out _cargoSpawnTableRef) && !ShouldUseRandomAirdropNode)
        {
            p.ReportRequiredOptionInvalid("Cargo");
        }
        if (!ShouldUseRandomAirdropNode)
        {
            if (p.data.TryGetString(p.legacyPrefix + "_Spawnpoint", out var value))
            {
                spawnpoint = value;
            }
            else
            {
                p.ReportRequiredOptionInvalid("Spawnpoint");
            }
        }
    }
}
