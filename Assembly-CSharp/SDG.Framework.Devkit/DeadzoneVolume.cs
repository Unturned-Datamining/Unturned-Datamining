using System;
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class DeadzoneVolume : LevelVolume<DeadzoneVolume, DeadzoneVolumeManager>, IDeadzoneNode
{
    private class Menu : SleekWrapper
    {
        private DeadzoneVolume volume;

        public Menu(DeadzoneVolume volume)
        {
            this.volume = volume;
            base.SizeOffset_X = 400f;
            base.SizeOffset_Y = 30f;
            SleekButtonState sleekButtonState = new SleekButtonState(new GUIContent("Default Radiation"), new GUIContent("Full Suit Radiation"));
            sleekButtonState.SizeOffset_X = 200f;
            sleekButtonState.SizeOffset_Y = 30f;
            sleekButtonState.state = (int)volume.DeadzoneType;
            sleekButtonState.AddLabel("Deadzone Type", ESleekSide.RIGHT);
            sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedState));
            AddChild(sleekButtonState);
        }

        private void OnSwappedState(SleekButtonState button, int state)
        {
            volume.DeadzoneType = (EDeadzoneType)state;
        }
    }

    [SerializeField]
    private EDeadzoneType _deadzoneType;

    public EDeadzoneType DeadzoneType
    {
        get
        {
            return _deadzoneType;
        }
        set
        {
            _deadzoneType = value;
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
        if (reader.containsKey("Deadzone_Type"))
        {
            _deadzoneType = reader.readValue<EDeadzoneType>("Deadzone_Type");
        }
        else
        {
            _deadzoneType = EDeadzoneType.DefaultRadiation;
        }
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("Deadzone_Type", _deadzoneType);
    }
}
