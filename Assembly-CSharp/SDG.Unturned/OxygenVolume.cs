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
            base.SizeOffset_X = 400f;
            base.SizeOffset_Y = 40f;
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.SizeOffset_X = 40f;
            sleekToggle.SizeOffset_Y = 40f;
            sleekToggle.Value = volume.isBreathable;
            sleekToggle.AddLabel("Is Breathable", ESleekSide.RIGHT);
            sleekToggle.OnValueChanged += OnHasOxygenToggled;
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
