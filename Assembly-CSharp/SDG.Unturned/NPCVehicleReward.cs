using System;
using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

public class NPCVehicleReward : INPCReward
{
    private CachingBcAssetRef _vehicleAssetRef;

    public CachingBcAssetRef VehicleAssetRef => _vehicleAssetRef;

    public string spawnpoint { get; protected set; }

    /// <summary>
    /// If set, takes priority over VehicleRedirectorAsset's paint color and over VehicleAsset's default paint color.
    /// </summary>
    public Color32? paintColor { get; protected set; }

    [Obsolete]
    public Guid VehicleGuid => _vehicleAssetRef.Guid;

    [Obsolete]
    public ushort id => _vehicleAssetRef.LegacyId;

    /// <summary>
    /// Returned asset is not necessarily a vehicle asset yet: It can also be a VehicleRedirectorAsset which the
    /// vehicle spawner requires to properly set paint color.
    /// </summary>
    public Asset FindAsset()
    {
        return _vehicleAssetRef.Get();
    }

    public VehicleAsset FindVehicleAssetAndHandleRedirects()
    {
        Asset asset = FindAsset();
        if (asset is VehicleRedirectorAsset { TargetVehicle: var targetVehicle })
        {
            asset = targetVehicle.Find();
        }
        return asset as VehicleAsset;
    }

    public override void GrantReward(Player player)
    {
        Spawnpoint spawnpoint = SpawnpointSystemV2.Get().FindFirstSpawnpoint(this.spawnpoint);
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
        VehicleManager.spawnLockedVehicleForPlayerV2(FindAsset(), point, rotation, player, paintColor);
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.read("Reward_Vehicle");
        }
        VehicleAsset vehicleAsset = FindVehicleAssetAndHandleRedirects();
        return Local.FormatText(arg0: (vehicleAsset == null) ? "?" : ("<color=" + Palette.hex(ItemTool.getRarityColorUI(vehicleAsset.rarity)) + ">" + vehicleAsset.vehicleName + "</color>"), text: text);
    }

    public override ISleekElement createUI(Player player)
    {
        string value = formatReward(player);
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        if (FindVehicleAssetAndHandleRedirects() == null)
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

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (!p.data.TryParseBcAssetRef("ID", EAssetType.VEHICLE, out _vehicleAssetRef))
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        if (p.data.TryGetString("Spawnpoint", out var value))
        {
            spawnpoint = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Spawnpoint");
        }
        if (p.data.TryParseColor32RGB("PaintColor", out var value2))
        {
            paintColor = value2;
        }
        else
        {
            paintColor = null;
        }
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        if (!p.data.TryParseBcAssetRef(p.legacyPrefix + "_ID", EAssetType.VEHICLE, out _vehicleAssetRef))
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        if (p.data.TryGetString(p.legacyPrefix + "_Spawnpoint", out var value))
        {
            spawnpoint = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Spawnpoint");
        }
        if (p.data.TryParseColor32RGB(p.legacyPrefix + "_PaintColor", out var value2))
        {
            paintColor = value2;
        }
        else
        {
            paintColor = null;
        }
    }

    public NPCVehicleReward()
    {
    }

    [Obsolete]
    public NPCVehicleReward(Guid newVehicleGuid, ushort newID, string newSpawnpoint, Color32? newPaintColor, string newText)
        : base(newText)
    {
        _vehicleAssetRef = new CachingBcAssetRef(newVehicleGuid, EAssetType.VEHICLE, newID);
        spawnpoint = newSpawnpoint;
        paintColor = newPaintColor;
    }
}
