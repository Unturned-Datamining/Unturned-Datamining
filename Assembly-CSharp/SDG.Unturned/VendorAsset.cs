using System;

namespace SDG.Unturned;

public class VendorAsset : Asset
{
    public string vendorName { get; protected set; }

    public override string FriendlyName => vendorName;

    public string vendorDescription { get; protected set; }

    public VendorBuying[] buying { get; protected set; }

    public VendorSellingBase[] selling { get; protected set; }

    public bool enableSorting { get; protected set; }

    public AssetReference<ItemCurrencyAsset> currency { get; protected set; }

    public byte? faceOverride { get; private set; }

    public override EAssetType assetCategory => EAssetType.NPC;

    public VendorAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        if (id < 2000 && !bundle.hasResource && !data.has("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 2000");
        }
        vendorName = localization.format("Name");
        vendorName = ItemTool.filterRarityRichText(vendorName);
        string desc = localization.format("Description");
        desc = ItemTool.filterRarityRichText(desc);
        RichTextUtil.replaceNewlineMarkup(ref desc);
        vendorDescription = desc;
        if (data.has("FaceOverride"))
        {
            faceOverride = data.readByte("FaceOverride", 0);
        }
        else
        {
            faceOverride = null;
        }
        buying = new VendorBuying[data.readByte("Buying", 0)];
        for (byte b = 0; b < buying.Length; b = (byte)(b + 1))
        {
            ushort newID = data.readUInt16("Buying_" + b + "_ID", 0);
            uint newCost = data.readUInt32("Buying_" + b + "_Cost");
            INPCCondition[] array = new INPCCondition[data.readByte("Buying_" + b + "_Conditions", 0)];
            NPCTool.readConditions(data, localization, "Buying_" + b + "_Condition_", array, this);
            INPCReward[] array2 = new INPCReward[data.readByte("Buying_" + b + "_Rewards", 0)];
            NPCTool.readRewards(data, localization, "Buying_" + b + "_Reward_", array2, this);
            buying[b] = new VendorBuying(this, b, newID, newCost, array, array2);
        }
        selling = new VendorSellingBase[data.readByte("Selling", 0)];
        for (byte b2 = 0; b2 < selling.Length; b2 = (byte)(b2 + 1))
        {
            string text = null;
            if (data.has("Selling_" + b2 + "_Type"))
            {
                text = data.readString("Selling_" + b2 + "_Type");
            }
            ushort newID2 = data.readUInt16("Selling_" + b2 + "_ID", 0);
            uint newCost2 = data.readUInt32("Selling_" + b2 + "_Cost");
            INPCCondition[] array3 = new INPCCondition[data.readByte("Selling_" + b2 + "_Conditions", 0)];
            NPCTool.readConditions(data, localization, "Selling_" + b2 + "_Condition_", array3, this);
            INPCReward[] array4 = new INPCReward[data.readByte("Selling_" + b2 + "_Rewards", 0)];
            NPCTool.readRewards(data, localization, "Selling_" + b2 + "_Reward_", array4, this);
            if (text == null || text.Equals("Item", StringComparison.InvariantCultureIgnoreCase))
            {
                selling[b2] = new VendorSellingItem(this, b2, newID2, newCost2, array3, array4);
            }
            else
            {
                if (!text.Equals("Vehicle", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new NotSupportedException("unknown selling type: '" + text + "'");
                }
                string text2 = "Selling_" + b2 + "_Spawnpoint";
                string text3 = data.readString(text2);
                if (string.IsNullOrEmpty(text3))
                {
                    Assets.reportError(this, "missing \"" + text2 + "\" for vehicle");
                }
                selling[b2] = new VendorSellingVehicle(this, b2, newID2, newCost2, text3, array3, array4);
            }
        }
        enableSorting = !data.has("Disable_Sorting");
        currency = data.readAssetReference<ItemCurrencyAsset>("Currency");
    }
}
