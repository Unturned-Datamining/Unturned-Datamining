using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class TeleporterExitVolume : LevelVolume<TeleporterExitVolume, TeleporterExitVolumeManager>
{
    private class Menu : SleekWrapper
    {
        private TeleporterExitVolume volume;

        public Menu(TeleporterExitVolume volume)
        {
            this.volume = volume;
            base.SizeOffset_X = 400f;
            base.SizeOffset_Y = 30f;
            ISleekField sleekField = Glazier.Get().CreateStringField();
            sleekField.SizeOffset_X = 200f;
            sleekField.SizeOffset_Y = 30f;
            sleekField.Text = volume.id;
            sleekField.AddLabel("ID", ESleekSide.RIGHT);
            sleekField.OnTextChanged += OnIdChanged;
            AddChild(sleekField);
        }

        private void OnIdChanged(ISleekField field, string id)
        {
            volume.id = id;
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
        id = reader.readValue<string>("Id");
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("Id", id);
    }
}
