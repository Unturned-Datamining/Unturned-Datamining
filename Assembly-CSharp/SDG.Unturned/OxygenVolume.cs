using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned;

public class OxygenVolume : LevelVolume<OxygenVolume, OxygenVolumeManager>
{
    private class Menu : SleekWrapper
    {
        private OxygenVolume volume;

        public Menu(OxygenVolume volume)
        {
            this.volume = volume;
            base.sizeOffset_X = 400;
            base.sizeOffset_Y = 40;
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.sizeOffset_X = 40;
            sleekToggle.sizeOffset_Y = 40;
            sleekToggle.state = volume.isBreathable;
            sleekToggle.addLabel("Is Breathable", ESleekSide.RIGHT);
            sleekToggle.onToggled += OnHasOxygenToggled;
            AddChild(sleekToggle);
        }

        private void OnHasOxygenToggled(ISleekToggle toggle, bool state)
        {
            volume.isBreathable = state;
        }
    }

    [SerializeField]
    private bool _isBreathable = true;

    public bool isBreathable
    {
        get
        {
            return _isBreathable;
        }
        set
        {
            if (!base.enabled)
            {
                _isBreathable = value;
                return;
            }
            GetVolumeManager().RemoveVolume(this);
            _isBreathable = value;
            GetVolumeManager().AddVolume(this);
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
        isBreathable = reader.readValue<bool>("Is_Breathable");
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("Is_Breathable", isBreathable);
    }

    protected override void Awake()
    {
        supportsFalloff = true;
        base.Awake();
    }
}
