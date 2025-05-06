using System;

namespace SDG.Unturned;

public class NPCRewardsListAssetReward : INPCReward
{
    public AssetReference<Asset> assetRef { get; protected set; }

    public override void GrantReward(Player player)
    {
        Asset asset = assetRef.Find();
        if (asset == null)
        {
            UnturnedLog.warn($"Rewards list asset reward unable to resolve guid ({assetRef})");
            return;
        }
        if (asset is SpawnAsset spawnAsset)
        {
            asset = SpawnTableTool.Resolve(spawnAsset, OnGetSpawnTableErrorContext);
            if (asset == null)
            {
                return;
            }
        }
        if (asset is NPCRewardsAsset nPCRewardsAsset)
        {
            if (nPCRewardsAsset.AreConditionsMet(player))
            {
                nPCRewardsAsset.ApplyConditions(player);
                nPCRewardsAsset.GrantRewards(player);
            }
        }
        else
        {
            UnturnedLog.warn("Rewards list asset reward unable to grant \"" + asset.FriendlyName + "\" because type is incompatible (" + asset.GetTypeFriendlyName() + ")");
        }
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseGuid("GUID", out var value))
        {
            assetRef = new AssetReference<Asset>(value);
        }
        else
        {
            p.ReportRequiredOptionInvalid("GUID");
        }
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseGuid(p.legacyPrefix + "_GUID", out var value))
        {
            assetRef = new AssetReference<Asset>(value);
        }
        else
        {
            p.ReportRequiredOptionInvalid("GUID");
        }
    }

    public NPCRewardsListAssetReward()
    {
    }

    [Obsolete]
    public NPCRewardsListAssetReward(AssetReference<Asset> newAssetRef, string newText)
        : base(newText)
    {
        assetRef = newAssetRef;
    }

    private string OnGetSpawnTableErrorContext()
    {
        return "NPC rewards list asset reward";
    }
}
