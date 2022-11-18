using SDG.Framework.Devkit;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned;

public class LocationDevkitNode : TempNodeBase
{
    private class Menu : SleekWrapper
    {
        private LocationDevkitNode node;

        public Menu(LocationDevkitNode node)
        {
            this.node = node;
            base.sizeOffset_X = 400;
            int num = 0;
            ISleekField sleekField = Glazier.Get().CreateStringField();
            sleekField.positionOffset_Y = num;
            sleekField.sizeOffset_X = 200;
            sleekField.sizeOffset_Y = 30;
            sleekField.text = node.locationName;
            sleekField.addLabel("Name", ESleekSide.RIGHT);
            sleekField.onTyped += OnIdTyped;
            AddChild(sleekField);
            num += sleekField.sizeOffset_Y + 10;
            base.sizeOffset_Y = num - 10;
        }

        private void OnIdTyped(ISleekField field, string state)
        {
            node.locationName = state;
        }
    }

    public string locationName;

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
        locationName = reader.readValue<string>("LocationName");
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("LocationName", locationName);
    }

    private void OnEnable()
    {
        LevelHierarchy.addItem(this);
        LocationDevkitNodeSystem.Get().AddNode(this);
    }

    private void OnDisable()
    {
        LocationDevkitNodeSystem.Get().RemoveNode(this);
        LevelHierarchy.removeItem(this);
    }

    private void Awake()
    {
        base.name = "Location";
        base.gameObject.layer = 30;
        if (Level.isEditor)
        {
            boxCollider = base.gameObject.GetOrAddComponent<BoxCollider>();
            boxCollider.size = new Vector3(1.5f, 1.5f, 1.5f);
            UpdateEditorVisibility();
        }
    }
}
