using SDG.Framework.Devkit;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

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
            ISleekUInt16Field sleekUInt16Field = Glazier.Get().CreateUInt16Field();
            sleekUInt16Field.PositionOffset_Y = num;
            sleekUInt16Field.SizeOffset_X = 200f;
            sleekUInt16Field.SizeOffset_Y = 30f;
            sleekUInt16Field.Value = node.id;
            sleekUInt16Field.AddLabel("ID", ESleekSide.RIGHT);
            sleekUInt16Field.OnValueChanged += OnIdTyped;
            AddChild(sleekUInt16Field);
            num += sleekUInt16Field.SizeOffset_Y + 10f;
            base.SizeOffset_Y = num - 10f;
        }

        private void OnIdTyped(ISleekUInt16Field field, ushort state)
        {
            node.id = state;
        }
    }

    public ushort id;

    [SerializeField]
    private BoxCollider boxCollider;

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
        id = reader.readValue<ushort>("SpawnTable_ID");
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("SpawnTable_ID", id);
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
