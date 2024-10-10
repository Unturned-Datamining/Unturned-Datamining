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
            base.SizeOffset_Y = 110f;
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
    }

    /// <summary>
    /// Nelson 2024-06-10: Changed this from guid to string because Unity serialization doesn't support guids
    /// and neither does the inspector. (e.g., couldn't duplicate reward volume without re-assigning guid)
    /// </summary>
    [SerializeField]
    internal string _assetGuid;

    private Guid parsedAssetGuid;

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
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("AssetGuid", parsedAssetGuid);
    }

    protected override void Awake()
    {
        forceShouldAddCollider = true;
        base.Awake();
        Guid.TryParse(_assetGuid, out parsedAssetGuid);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!SDG.Unturned.Provider.isServer || !other.CompareTag("Player"))
        {
            return;
        }
        Player player = DamageTool.getPlayer(other.transform);
        if (!(player != null) || parsedAssetGuid.IsEmpty())
        {
            return;
        }
        if (Assets.find(parsedAssetGuid) is NPCRewardsAsset nPCRewardsAsset)
        {
            if (nPCRewardsAsset.AreConditionsMet(player))
            {
                nPCRewardsAsset.ApplyConditions(player);
                nPCRewardsAsset.GrantRewards(player);
            }
        }
        else
        {
            UnturnedLog.warn($"NPC reward volume unable to find asset ({parsedAssetGuid:N})");
        }
    }
}
