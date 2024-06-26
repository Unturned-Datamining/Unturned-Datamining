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
            float num = 0f;
            SleekButtonState sleekButtonState = new SleekButtonState(new GUIContent("Default Radiation"), new GUIContent("Full Suit Radiation"));
            sleekButtonState.PositionOffset_Y = num;
            sleekButtonState.SizeOffset_X = 200f;
            sleekButtonState.SizeOffset_Y = 30f;
            sleekButtonState.state = (int)volume.DeadzoneType;
            sleekButtonState.AddLabel("Deadzone Type", ESleekSide.RIGHT);
            sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedState));
            AddChild(sleekButtonState);
            num += sleekButtonState.SizeOffset_Y + 10f;
            ISleekFloat32Field sleekFloat32Field = Glazier.Get().CreateFloat32Field();
            sleekFloat32Field.PositionOffset_Y = num;
            sleekFloat32Field.SizeOffset_X = 200f;
            sleekFloat32Field.SizeOffset_Y = 30f;
            sleekFloat32Field.Value = volume.UnprotectedDamagePerSecond;
            sleekFloat32Field.AddLabel("Damage per Second (Unprotected)", ESleekSide.RIGHT);
            sleekFloat32Field.OnValueChanged += OnUnprotectedDamageChanged;
            AddChild(sleekFloat32Field);
            num += sleekFloat32Field.SizeOffset_Y + 10f;
            ISleekFloat32Field sleekFloat32Field2 = Glazier.Get().CreateFloat32Field();
            sleekFloat32Field2.PositionOffset_Y = num;
            sleekFloat32Field2.SizeOffset_X = 200f;
            sleekFloat32Field2.SizeOffset_Y = 30f;
            sleekFloat32Field2.Value = volume.UnprotectedDamagePerSecond;
            sleekFloat32Field2.AddLabel("Damage per Second (Protected)", ESleekSide.RIGHT);
            sleekFloat32Field2.OnValueChanged += OnProtectedDamageChanged;
            AddChild(sleekFloat32Field2);
            num += sleekFloat32Field2.SizeOffset_Y + 10f;
            ISleekFloat32Field sleekFloat32Field3 = Glazier.Get().CreateFloat32Field();
            sleekFloat32Field3.PositionOffset_Y = num;
            sleekFloat32Field3.SizeOffset_X = 200f;
            sleekFloat32Field3.SizeOffset_Y = 30f;
            sleekFloat32Field3.Value = volume.UnprotectedRadiationPerSecond;
            sleekFloat32Field3.AddLabel("Radiation per Second", ESleekSide.RIGHT);
            sleekFloat32Field3.OnValueChanged += OnUnprotectedRadiationChanged;
            AddChild(sleekFloat32Field3);
            num += sleekFloat32Field3.SizeOffset_Y + 10f;
            ISleekFloat32Field sleekFloat32Field4 = Glazier.Get().CreateFloat32Field();
            sleekFloat32Field4.PositionOffset_Y = num;
            sleekFloat32Field4.SizeOffset_X = 200f;
            sleekFloat32Field4.SizeOffset_Y = 30f;
            sleekFloat32Field4.Value = volume.MaskFilterDamagePerSecond;
            sleekFloat32Field4.AddLabel("Mask Filter Degradation per Second", ESleekSide.RIGHT);
            sleekFloat32Field4.OnValueChanged += OnMaskFilterDamageChanged;
            AddChild(sleekFloat32Field4);
            num += sleekFloat32Field4.SizeOffset_Y + 10f;
            base.SizeOffset_Y = num - 10f;
        }

        private void OnSwappedState(SleekButtonState button, int state)
        {
            volume.DeadzoneType = (EDeadzoneType)state;
        }

        private void OnUnprotectedDamageChanged(ISleekFloat32Field field, float value)
        {
            volume.UnprotectedDamagePerSecond = value;
        }

        private void OnProtectedDamageChanged(ISleekFloat32Field field, float value)
        {
            volume.ProtectedDamagePerSecond = value;
        }

        private void OnUnprotectedRadiationChanged(ISleekFloat32Field field, float value)
        {
            volume.UnprotectedRadiationPerSecond = value;
        }

        private void OnMaskFilterDamageChanged(ISleekFloat32Field field, float value)
        {
            volume.MaskFilterDamagePerSecond = value;
        }
    }

    [SerializeField]
    private EDeadzoneType _deadzoneType;

    [SerializeField]
    private float _unprotectedDamagePerSecond;

    [SerializeField]
    private float _protectedDamagePerSecond;

    [SerializeField]
    private float _unprotectedRadiationPerSecond = 6.25f;

    [SerializeField]
    private float _maskFilterDamagePerSecond = 0.4f;

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

    public float UnprotectedDamagePerSecond
    {
        get
        {
            return _unprotectedDamagePerSecond;
        }
        set
        {
            _unprotectedDamagePerSecond = value;
        }
    }

    public float ProtectedDamagePerSecond
    {
        get
        {
            return _protectedDamagePerSecond;
        }
        set
        {
            _protectedDamagePerSecond = value;
        }
    }

    public float UnprotectedRadiationPerSecond
    {
        get
        {
            return _unprotectedRadiationPerSecond;
        }
        set
        {
            _unprotectedRadiationPerSecond = value;
        }
    }

    public float MaskFilterDamagePerSecond
    {
        get
        {
            return _maskFilterDamagePerSecond;
        }
        set
        {
            _maskFilterDamagePerSecond = value;
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
        _unprotectedDamagePerSecond = reader.readValue<float>("UnprotectedDamagePerSecond");
        _protectedDamagePerSecond = reader.readValue<float>("ProtectedDamagePerSecond");
        if (reader.containsKey("UnprotectedRadiationPerSecond"))
        {
            _unprotectedRadiationPerSecond = reader.readValue<float>("UnprotectedRadiationPerSecond");
        }
        else
        {
            _unprotectedRadiationPerSecond = 6.25f;
        }
        if (reader.containsKey("MaskFilterDamagePerSecond"))
        {
            _maskFilterDamagePerSecond = reader.readValue<float>("MaskFilterDamagePerSecond");
        }
        else
        {
            _maskFilterDamagePerSecond = 0.4f;
        }
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("Deadzone_Type", _deadzoneType);
        writer.writeValue("UnprotectedDamagePerSecond", _unprotectedDamagePerSecond);
        writer.writeValue("ProtectedDamagePerSecond", _protectedDamagePerSecond);
        writer.writeValue("UnprotectedRadiationPerSecond", _unprotectedRadiationPerSecond);
        writer.writeValue("MaskFilterDamagePerSecond", _maskFilterDamagePerSecond);
    }
}
