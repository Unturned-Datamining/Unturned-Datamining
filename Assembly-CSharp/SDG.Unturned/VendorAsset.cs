using System;
using UnityEngine;

namespace SDG.Unturned;

public class VendorAsset : Asset
{
    public string vendorName { get; protected set; }

    public override string FriendlyName => RichTextUtil.replaceColorTags(vendorName);

    public string vendorDescription { get; protected set; }

    public VendorBuying[] buying { get; protected set; }

    public VendorSellingBase[] selling { get; protected set; }

    /// <summary>
    /// Should the buying and selling lists be alphabetically sorted?
    /// </summary>
    public bool enableSorting { get; protected set; }

    public AssetReference<ItemCurrencyAsset> currency { get; protected set; }

    public byte? faceOverride { get; private set; }

    public override EAssetType assetCategory => EAssetType.NPC;

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        if (id < 2000 && !base.OriginAllowsVanillaLegacyId && !p.data.ContainsKey("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 2000");
        }
        vendorName = p.localization.format("Name");
        vendorName = ItemTool.filterRarityRichText(vendorName);
        string desc = p.localization.format("Description");
        desc = ItemTool.filterRarityRichText(desc);
        RichTextUtil.replaceNewlineMarkup(ref desc);
        vendorDescription = desc;
        if (p.data.ContainsKey("FaceOverride"))
        {
            faceOverride = p.data.ParseUInt8("FaceOverride", 0);
        }
        else
        {
            faceOverride = null;
        }
        buying = new VendorBuying[p.data.ParseUInt8("Buying", 0)];
        for (byte b = 0; b < buying.Length; b++)
        {
            string text = p.localization.FormatOrNull($"Buying_{b}_Description");
            if (!string.IsNullOrEmpty(text))
            {
                text = ItemTool.filterRarityRichText(text);
            }
            p.data.ParseGuidOrLegacyId("Buying_" + b + "_ID", out var guid, out var legacyId);
            uint newCost = p.data.ParseUInt32("Buying_" + b + "_Cost");
            NPCConditionsList newConditionsList = default(NPCConditionsList);
            newConditionsList.Parse(p.data, p.localization, this, "Buying_" + b + "_Conditions", "Buying_" + b + "_Condition_");
            NPCRewardsList newRewardsList = default(NPCRewardsList);
            newRewardsList.Parse(p.data, p.localization, this, "Buying_" + b + "_Rewards", "Buying_" + b + "_Reward_");
            buying[b] = new VendorBuying(this, b, guid, legacyId, newCost, newConditionsList, newRewardsList, text);
        }
        selling = new VendorSellingBase[p.data.ParseUInt8("Selling", 0)];
        for (byte b2 = 0; b2 < selling.Length; b2++)
        {
            string text2 = null;
            if (p.data.ContainsKey("Selling_" + b2 + "_Type"))
            {
                text2 = p.data.GetString("Selling_" + b2 + "_Type");
            }
            string text3 = p.localization.FormatOrNull($"Selling_{b2}_Description");
            if (!string.IsNullOrEmpty(text3))
            {
                text3 = ItemTool.filterRarityRichText(text3);
            }
            p.data.ParseGuidOrLegacyId("Selling_" + b2 + "_ID", out var guid2, out var legacyId2);
            uint newCost2 = p.data.ParseUInt32("Selling_" + b2 + "_Cost");
            NPCConditionsList newConditionsList2 = default(NPCConditionsList);
            newConditionsList2.Parse(p.data, p.localization, this, "Selling_" + b2 + "_Conditions", "Selling_" + b2 + "_Condition_");
            NPCRewardsList newRewardsList2 = default(NPCRewardsList);
            newRewardsList2.Parse(p.data, p.localization, this, "Selling_" + b2 + "_Rewards", "Selling_" + b2 + "_Reward_");
            if (text2 == null || text2.Equals("Item", StringComparison.InvariantCultureIgnoreCase))
            {
                int newSight = p.data.ParseInt32("Selling_" + b2 + "_Sight", -1);
                int newTactical = p.data.ParseInt32("Selling_" + b2 + "_Tactical", -1);
                int newGrip = p.data.ParseInt32("Selling_" + b2 + "_Grip", -1);
                int newBarrel = p.data.ParseInt32("Selling_" + b2 + "_Barrel", -1);
                int newMagazine = p.data.ParseInt32("Selling_" + b2 + "_Magazine", -1);
                int newAmmo = p.data.ParseInt32("Selling_" + b2 + "_Ammo", -1);
                selling[b2] = new VendorSellingItem(this, b2, guid2, legacyId2, newCost2, newConditionsList2, newRewardsList2, text3, newSight, newTactical, newGrip, newBarrel, newMagazine, newAmmo);
            }
            else
            {
                if (!text2.Equals("Vehicle", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new NotSupportedException("unknown selling type: '" + text2 + "'");
                }
                string text4 = "Selling_" + b2 + "_Spawnpoint";
                string @string = p.data.GetString(text4);
                if (string.IsNullOrEmpty(@string))
                {
                    Assets.ReportError(this, "missing \"" + text4 + "\" for vehicle");
                }
                Color32? newPaintColor = null;
                if (p.data.TryParseColor32RGB("Selling_" + b2 + "_PaintColor", out var value))
                {
                    newPaintColor = value;
                }
                selling[b2] = new VendorSellingVehicle(this, b2, guid2, legacyId2, newCost2, @string, newPaintColor, newConditionsList2, newRewardsList2, text3);
            }
        }
        enableSorting = !p.data.ContainsKey("Disable_Sorting");
        currency = p.data.readAssetReference<ItemCurrencyAsset>("Currency");
    }
}
