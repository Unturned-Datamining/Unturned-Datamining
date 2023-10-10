using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class Spawnpoint : TempNodeBase
{
    private class Menu : SleekWrapper
    {
        private Spawnpoint node;

        public Menu(Spawnpoint node)
        {
            this.node = node;
            base.SizeOffset_X = 400f;
            float num = 0f;
            ISleekField sleekField = Glazier.Get().CreateStringField();
            sleekField.PositionOffset_Y = num;
            sleekField.SizeOffset_X = 200f;
            sleekField.SizeOffset_Y = 30f;
            sleekField.Text = node.id;
            sleekField.AddLabel("ID", ESleekSide.RIGHT);
            sleekField.OnTextChanged += OnIdTyped;
            AddChild(sleekField);
            num += sleekField.SizeOffset_Y + 10f;
            base.SizeOffset_Y = num - 10f;
        }

        private void OnIdTyped(ISleekField field, string state)
        {
            node.id = state;
        }
    }

    public string id;

    public SphereCollider sphere { get; protected set; }

    internal override ISleekElement CreateMenu()
    {
        return new Menu(this);
    }

    internal void UpdateEditorVisibility()
    {
        sphere.enabled = SpawnpointSystemV2.Get().IsVisible;
    }

    protected override void readHierarchyItem(IFormattedFileReader reader)
    {
        base.readHierarchyItem(reader);
        id = reader.readValue<string>("ID");
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("ID", id);
    }

    protected void OnEnable()
    {
        LevelHierarchy.addItem(this);
        SpawnpointSystemV2.Get().AddSpawnpoint(this);
    }

    protected void OnDisable()
    {
        SpawnpointSystemV2.Get().RemoveSpawnpoint(this);
        LevelHierarchy.removeItem(this);
    }

    protected void Awake()
    {
        base.name = "Spawnpoint";
        base.gameObject.layer = 30;
        if (Level.isEditor)
        {
            sphere = base.gameObject.GetOrAddComponent<SphereCollider>();
            sphere.radius = 0.5f;
            UpdateEditorVisibility();
        }
    }
}
