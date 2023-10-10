using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

public class NPCVehicleReward : INPCReward
{
    public ushort id { get; protected set; }

    public string spawnpoint { get; protected set; }

    public override void GrantReward(Player player)
    {
        Spawnpoint spawnpoint = SpawnpointSystemV2.Get().FindSpawnpoint(this.spawnpoint);
        Vector3 point;
        Quaternion rotation;
        if (spawnpoint != null)
        {
            point = spawnpoint.transform.position;
            rotation = spawnpoint.transform.rotation;
        }
        else
        {
            UnturnedLog.error("Failed to find NPC vehicle reward spawnpoint: " + this.spawnpoint);
            point = VehicleTool.GetPositionForVehicle(player);
            rotation = player.transform.rotation;
        }
        VehicleManager.spawnLockedVehicleForPlayerV2(id, point, rotation, player);
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.read("Reward_Vehicle");
        }
        return string.Format(arg0: (!(Assets.find(EAssetType.VEHICLE, id) is VehicleAsset vehicleAsset)) ? "?" : ("<color=" + Palette.hex(ItemTool.getRarityColorUI(vehicleAsset.rarity)) + ">" + vehicleAsset.vehicleName + "</color>"), format: text);
    }

    public override ISleekElement createUI(Player player)
    {
        string value = formatReward(player);
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        if (!(Assets.find(EAssetType.VEHICLE, id) is VehicleAsset))
        {
            return null;
        }
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        sleekBox.SizeOffset_Y = 30f;
        sleekBox.SizeScale_X = 1f;
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.PositionOffset_X = 5f;
        sleekLabel.SizeOffset_X = -10f;
        sleekLabel.SizeScale_X = 1f;
        sleekLabel.SizeScale_Y = 1f;
        sleekLabel.TextAlignment = TextAnchor.MiddleLeft;
        sleekLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        sleekLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        sleekLabel.AllowRichText = true;
        sleekLabel.Text = value;
        sleekBox.AddChild(sleekLabel);
        return sleekBox;
    }

    public NPCVehicleReward(ushort newID, string newSpawnpoint, string newText)
        : base(newText)
    {
        id = newID;
        spawnpoint = newSpawnpoint;
    }
}
