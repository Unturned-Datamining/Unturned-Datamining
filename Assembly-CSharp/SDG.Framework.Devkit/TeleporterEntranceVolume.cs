using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class TeleporterEntranceVolume : LevelVolume<TeleporterEntranceVolume, TeleporterEntranceVolumeManager>
{
    private class Menu : SleekWrapper
    {
        private TeleporterEntranceVolume volume;

        public Menu(TeleporterEntranceVolume volume)
        {
            this.volume = volume;
            base.SizeOffset_X = 400f;
            base.SizeOffset_Y = 30f;
            ISleekField sleekField = Glazier.Get().CreateStringField();
            sleekField.SizeOffset_X = 200f;
            sleekField.SizeOffset_Y = 30f;
            sleekField.Text = volume.pairId;
            sleekField.AddLabel("Pair ID", ESleekSide.RIGHT);
            sleekField.OnTextChanged += OnIdChanged;
            AddChild(sleekField);
        }

        private void OnIdChanged(ISleekField field, string id)
        {
            volume.pairId = id;
        }
    }

    [SerializeField]
    public string pairId;

    public override ISleekElement CreateMenu()
    {
        ISleekElement sleekElement = new Menu(this);
        AppendBaseMenu(sleekElement);
        return sleekElement;
    }

    protected override void readHierarchyItem(IFormattedFileReader reader)
    {
        base.readHierarchyItem(reader);
        pairId = reader.readValue<string>("Pair_Id");
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("Pair_Id", pairId);
    }

    public void OnTriggerEnter(Collider other)
    {
        TeleporterExitVolume teleporterExitVolume = VolumeManager<TeleporterExitVolume, TeleporterExitVolumeManager>.Get().FindExitVolume(pairId);
        if (teleporterExitVolume != null)
        {
            PlayerMovement component = other.gameObject.GetComponent<PlayerMovement>();
            if (component != null && component.CanEnterTeleporter)
            {
                component.EnterTeleporterVolume(this, teleporterExitVolume);
            }
        }
    }

    protected override void Awake()
    {
        forceShouldAddCollider = true;
        base.Awake();
    }
}
