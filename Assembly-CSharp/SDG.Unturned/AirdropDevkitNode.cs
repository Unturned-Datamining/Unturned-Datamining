using System;
using SDG.Framework.Devkit;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class AirdropDevkitNode : TempNodeBase
{
    private class Menu : SleekWrapper
    {
        private AirdropDevkitNode node;

        public Menu(AirdropDevkitNode node)
        {
            this.node = node;
            base.SizeOffset_X = 400f;
            float num = 0f;
            SleekBcAssetField sleekBcAssetField = new SleekBcAssetField(EAssetType.SPAWN);
            sleekBcAssetField.PositionOffset_Y = num;
            sleekBcAssetField.SizeOffset_X = 200f;
            sleekBcAssetField.SizeOffset_Y = 60f;
            sleekBcAssetField.Value = node.CargoSpawnTableRef;
            sleekBcAssetField.AddLabel("ID", ESleekSide.RIGHT);
            sleekBcAssetField.OnValueChanged += OnIdTyped;
            AddChild(sleekBcAssetField);
            num += sleekBcAssetField.SizeOffset_Y + 10f;
            base.SizeOffset_Y = num - 10f;
        }

        private void OnIdTyped(SleekBcAssetField field)
        {
            node.CargoSpawnTableRef = field.Value;
        }
    }

    [Obsolete("Replaced by CargoSpawnTableRef")]
    public ushort id;

    [SerializeField]
    internal Guid _cargoSpawnTableGuid;

    [SerializeField]
    private BoxCollider boxCollider;

    public CachingBcAssetRef CargoSpawnTableRef
    {
        get
        {
            return new CachingBcAssetRef(_cargoSpawnTableGuid, EAssetType.SPAWN, id);
        }
        set
        {
            id = value.LegacyId;
            _cargoSpawnTableGuid = value.Guid;
        }
    }

    public SpawnAsset GetCargoSpawnTableOrLogWarning()
    {
        CachingBcAssetRef cargoSpawnTableRef = CargoSpawnTableRef;
        SpawnAsset spawnAsset = cargoSpawnTableRef.Get<SpawnAsset>();
        if (spawnAsset == null)
        {
            UnturnedLog.warn($"Unable to find cargo spawn table ({cargoSpawnTableRef}) for airdrop marker at {base.transform.position}");
        }
        return spawnAsset;
    }

    internal override ISleekElement CreateMenu()
    {
        return new Menu(this);
    }

    internal void UpdateEditorVisibility()
    {
        boxCollider.enabled = SpawnpointSystemV2.Get().IsVisible;
    }

    protected override void readHierarchyItem(IFormattedFileReader reader)
    {
        base.readHierarchyItem(reader);
        string text = reader.readValue("SpawnTable_ID");
        if (ushort.TryParse(text, out id))
        {
            _cargoSpawnTableGuid = Guid.Empty;
        }
        else if (Guid.TryParse(text, out _cargoSpawnTableGuid))
        {
            id = 0;
        }
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        if (!_cargoSpawnTableGuid.IsEmpty())
        {
            writer.writeValue("SpawnTable_ID", _cargoSpawnTableGuid);
        }
        else
        {
            writer.writeValue("SpawnTable_ID", id);
        }
    }

    private void OnEnable()
    {
        LevelHierarchy.addItem(this);
        AirdropDevkitNodeSystem.Get().AddNode(this);
    }

    private void OnDisable()
    {
        AirdropDevkitNodeSystem.Get().RemoveNode(this);
        LevelHierarchy.removeItem(this);
    }

    private void Awake()
    {
        base.name = "Airdrop";
        base.gameObject.layer = 30;
        if (Level.isEditor)
        {
            boxCollider = base.gameObject.GetOrAddComponent<BoxCollider>();
            boxCollider.center = new Vector3(0f, 16f, 0f);
            boxCollider.size = new Vector3(1f, 32f, 1f);
            UpdateEditorVisibility();
        }
    }
}
