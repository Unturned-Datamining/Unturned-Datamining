using SDG.Framework.Devkit;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned;

public class NPCOverlapVolume : LevelVolume<NPCOverlapVolume, NPCOverlapVolumeManager>
{
    private class Menu : SleekWrapper
    {
        private NPCOverlapVolume volume;

        public Menu(NPCOverlapVolume volume)
        {
            this.volume = volume;
            base.SizeOffset_X = 400f;
            base.SizeOffset_Y = 30f;
            ISleekField sleekField = Glazier.Get().CreateStringField();
            sleekField.SizeOffset_X = 200f;
            sleekField.SizeOffset_Y = 30f;
            sleekField.Text = volume.id;
            sleekField.AddLabel("ID", ESleekSide.RIGHT);
            sleekField.OnTextChanged += OnNameChanged;
            AddChild(sleekField);
        }

        private void OnNameChanged(ISleekField field, string id)
        {
            volume.id = id;
            LevelHierarchy.MarkDirty();
        }
    }

    [SerializeField]
    private string _id;

    public string id
    {
        get
        {
            return _id;
        }
        set
        {
            if (!string.Equals(_id, value))
            {
                GetVolumeManager().RemoveVolumeFromIdDictionary(this);
                _id = value;
                GetVolumeManager().AddVolumeToIdDictionary(this);
            }
        }
    }

    public override ISleekElement CreateMenu()
    {
        ISleekElement sleekElement = new Menu(this);
        AppendBaseMenu(sleekElement);
        return sleekElement;
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
}
