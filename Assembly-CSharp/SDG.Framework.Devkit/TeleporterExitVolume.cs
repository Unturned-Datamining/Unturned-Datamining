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
            base.sizeOffset_X = 400;
            base.sizeOffset_Y = 30;
            ISleekField sleekField = Glazier.Get().CreateStringField();
            sleekField.sizeOffset_X = 200;
            sleekField.sizeOffset_Y = 30;
            sleekField.text = volume.id;
            sleekField.addLabel("ID", ESleekSide.RIGHT);
            sleekField.onTyped += OnIdChanged;
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
