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
            base.SizeOffset_X = 400f;
            float num = 0f;
            ISleekField sleekField = Glazier.Get().CreateStringField();
            sleekField.PositionOffset_Y = num;
            sleekField.SizeOffset_X = 200f;
            sleekField.SizeOffset_Y = 30f;
            sleekField.Text = node.locationName;
            sleekField.AddLabel("Name", ESleekSide.RIGHT);
            sleekField.OnTextChanged += OnIdTyped;
            AddChild(sleekField);
            num += sleekField.SizeOffset_Y + 10f;
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.PositionOffset_Y = num;
            sleekToggle.SizeOffset_X = 40f;
            sleekToggle.SizeOffset_Y = 40f;
            sleekToggle.Value = node.isVisibleOnMap;
            sleekToggle.AddLabel("Visible on map", ESleekSide.RIGHT);
            sleekToggle.OnValueChanged += OnVisibleOnMapToggled;
            AddChild(sleekToggle);
            num += sleekToggle.SizeOffset_Y + 10f;
            base.SizeOffset_Y = num - 10f;
        }

        private void OnIdTyped(ISleekField field, string state)
        {
            node.locationName = state;
        }

        private void OnVisibleOnMapToggled(ISleekToggle toggle, bool state)
        {
            node.isVisibleOnMap = state;
        }
    }

    public string locationName;

    /// <summary>
    /// If true, visible in chart and satellite UIs.
    /// </summary>
    public bool isVisibleOnMap = true;

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
        if (reader.containsKey("IsVisibleOnMap"))
        {
            isVisibleOnMap = reader.readValue<bool>("IsVisibleOnMap");
        }
        else
        {
            isVisibleOnMap = true;
        }
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("LocationName", locationName);
        writer.writeValue("IsVisibleOnMap", isVisibleOnMap);
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
