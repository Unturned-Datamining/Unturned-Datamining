using System;
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Framework.Devkit;

public class NPCRewardVolume : LevelVolume<NPCRewardVolume, NPCRewardVolumeManager>
{
    private class Menu : SleekWrapper
    {
        private ISleekBox assetNameBox;

        private NPCRewardVolume volume;

        public Menu(NPCRewardVolume volume)
        {
            this.volume = volume;
            base.SizeOffset_X = 400f;
            base.SizeOffset_Y = 120f;
            ISleekField sleekField = Glazier.Get().CreateStringField();
            sleekField.SizeOffset_X = 200f;
            sleekField.SizeOffset_Y = 30f;
            sleekField.Text = volume.parsedAssetGuid.ToString("N");
            sleekField.AddLabel("Asset GUID", ESleekSide.RIGHT);
            sleekField.OnTextChanged += OnIdChanged;
            AddChild(sleekField);
            assetNameBox = Glazier.Get().CreateBox();
            assetNameBox.PositionOffset_Y = 40f;
            assetNameBox.SizeOffset_X = 200f;
            assetNameBox.SizeOffset_Y = 30f;
            assetNameBox.AddLabel("Asset", ESleekSide.RIGHT);
            AddChild(assetNameBox);
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.PositionOffset_Y = 80f;
            sleekToggle.SizeOffset_X = 40f;
            sleekToggle.SizeOffset_Y = 40f;
            sleekToggle.Value = volume.shouldAffectVehiclePassengers;
            sleekToggle.AddLabel("Affect Vehicle Passengers", ESleekSide.RIGHT);
            sleekToggle.OnValueChanged += OnAffectVehiclePassengersToggled;
            AddChild(sleekToggle);
            SyncAssetName();
        }

        private void OnIdChanged(ISleekField field, string idString)
        {
            if (!Guid.TryParse(idString, out volume.parsedAssetGuid))
            {
                volume.parsedAssetGuid = Guid.Empty;
            }
            volume._assetGuid = volume.parsedAssetGuid.ToString("N");
            SyncAssetName();
            LevelHierarchy.MarkDirty();
        }

        private void SyncAssetName()
        {
            if (Assets.find(volume.parsedAssetGuid) is NPCRewardsAsset nPCRewardsAsset)
            {
                assetNameBox.Text = nPCRewardsAsset.FriendlyName;
            }
            else
            {
                assetNameBox.Text = "null";
            }
        }

        private void OnAffectVehiclePassengersToggled(ISleekToggle toggle, bool state)
        {
            volume.shouldAffectVehiclePassengers = state;
            LevelHierarchy.MarkDirty();
        }
    }

    /// <summary>
    /// Nelson 2024-06-10: Changed this from guid to string because Unity serialization doesn't support guids
    /// and neither does the inspector. (e.g., couldn't duplicate reward volume without re-assigning guid)
    /// </summary>
    [SerializeField]
    internal string _assetGuid;

    private Guid parsedAssetGuid;

    /// <summary>
    /// If true, vehicles overlapping volume will check conditions and (if met) grant rewards to passengers.
    /// </summary>
    public bool shouldAffectVehiclePassengers;

    public override ISleekElement CreateMenu()
    {
        ISleekElement sleekElement = new Menu(this);
        AppendBaseMenu(sleekElement);
        return sleekElement;
    }

    protected override void readHierarchyItem(IFormattedFileReader reader)
    {
        base.readHierarchyItem(reader);
        parsedAssetGuid = reader.readValue<Guid>("AssetGuid");
        _assetGuid = parsedAssetGuid.ToString("N");
        shouldAffectVehiclePassengers = reader.readValue<bool>("ShouldAffectVehiclePassengers");
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("AssetGuid", parsedAssetGuid);
        writer.writeValue("ShouldAffectVehiclePassengers", shouldAffectVehiclePassengers);
    }

    protected override void Awake()
    {
        forceShouldAddCollider = true;
        base.Awake();
        Guid.TryParse(_assetGuid, out parsedAssetGuid);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!SDG.Unturned.Provider.isServer)
        {
            return;
        }
        bool flag = other.CompareTag("Player");
        bool flag2 = other.CompareTag("Vehicle");
        if ((!flag && (!flag2 || !shouldAffectVehiclePassengers)) || parsedAssetGuid.IsEmpty())
        {
            return;
        }
        if (!(Assets.find(parsedAssetGuid) is NPCRewardsAsset asset))
        {
            UnturnedLog.warn($"NPC reward volume unable to find asset ({parsedAssetGuid:N})");
        }
        else if (flag)
        {
            Player player = DamageTool.getPlayer(other.transform);
            if (player != null)
            {
                EvaluateForPlayer(player, asset);
            }
        }
        else
        {
            if (!flag2)
            {
                return;
            }
            InteractableVehicle vehicle = DamageTool.getVehicle(other.transform);
            if (!(vehicle != null) || vehicle.passengers == null)
            {
                return;
            }
            Passenger[] passengers = vehicle.passengers;
            foreach (Passenger passenger in passengers)
            {
                if (passenger.player != null && passenger.player.player != null)
                {
                    EvaluateForPlayer(passenger.player.player, asset);
                }
            }
        }
    }

    private void EvaluateForPlayer(Player player, NPCRewardsAsset asset)
    {
        if (asset.AreConditionsMet(player))
        {
            asset.ApplyConditions(player);
            asset.GrantRewards(player);
        }
    }
}
