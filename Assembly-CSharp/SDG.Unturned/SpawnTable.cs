using System;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class SpawnTable
{
    internal ushort legacyAssetId;

    internal ushort legacySpawnId;

    internal Guid targetGuid;

    public int weight;

    internal float normalizedWeight;

    public bool isLink;

    public bool isOverride;

    public bool hasNotifiedChild;

    internal bool TryParse(Asset assetContext, DatDictionary datDictionary)
    {
        targetGuid = datDictionary.ParseGuid("Guid");
        legacyAssetId = datDictionary.ParseUInt16("LegacyAssetId", 0);
        legacySpawnId = datDictionary.ParseUInt16("LegacySpawnId", 0);
        isOverride = datDictionary.ParseBool("IsOverride");
        weight = datDictionary.ParseInt32("Weight", isOverride ? 1 : 0);
        if (legacySpawnId == 0 && legacyAssetId == 0 && targetGuid.IsEmpty())
        {
            Assets.reportError(assetContext, "contains an entry with neither a LegacyAssetId, LegacySpawnId, or Guid set!");
            return false;
        }
        if (weight <= 0)
        {
            Assets.reportError(assetContext, "contains an entry with no weight!");
            return false;
        }
        return true;
    }

    internal void Write(DatWriter writer, EAssetType legacyAssetType)
    {
        if (!targetGuid.IsEmpty())
        {
            Asset asset = Assets.find(targetGuid);
            string message = ((asset != null) ? (asset.FriendlyName + " (" + asset.GetTypeNameWithoutSuffix() + ")") : $"Unknown {legacyAssetType}");
            writer.WriteComment(message);
            writer.WriteKeyValue("Guid", targetGuid);
        }
        else if (legacyAssetId > 0)
        {
            Asset asset2 = Assets.find(legacyAssetType, legacyAssetId);
            string message2 = ((asset2 != null) ? (asset2.FriendlyName + " (" + asset2.GetTypeNameWithoutSuffix() + ")") : $"Unknown {legacyAssetType}");
            writer.WriteComment(message2);
            writer.WriteKeyValue("LegacyAssetId", legacyAssetId);
        }
        else if (legacySpawnId > 0)
        {
            string message3 = ((Assets.find(EAssetType.SPAWN, legacySpawnId) is SpawnAsset spawnAsset) ? (spawnAsset.FriendlyName + " (" + spawnAsset.GetTypeNameWithoutSuffix() + ")") : "Unknown");
            writer.WriteComment(message3);
            writer.WriteKeyValue("LegacySpawnId", legacySpawnId);
        }
        if (isOverride)
        {
            writer.WriteKeyValue("IsOverride", isOverride);
        }
        writer.WriteKeyValue("Weight", weight);
    }
}
