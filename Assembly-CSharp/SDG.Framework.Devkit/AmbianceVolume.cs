using System;
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Framework.Devkit;

public class AmbianceVolume : LevelVolume<AmbianceVolume, AmbianceVolumeManager>, IAmbianceNode
{
    private class Menu : SleekWrapper
    {
        private AmbianceVolume volume;

        public Menu(AmbianceVolume volume)
        {
            this.volume = volume;
            base.SizeOffset_X = 400f;
            ISleekField sleekField = Glazier.Get().CreateStringField();
            sleekField.SizeOffset_X = 200f;
            sleekField.SizeOffset_Y = 30f;
            if (volume._effectGuid.IsEmpty())
            {
                sleekField.Text = volume._id.ToString();
            }
            else
            {
                sleekField.Text = volume._effectGuid.ToString("N");
            }
            sleekField.AddLabel("Effect ID", ESleekSide.RIGHT);
            sleekField.OnTextChanged += OnIdChanged;
            AddChild(sleekField);
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.PositionOffset_Y = 40f;
            sleekToggle.SizeOffset_X = 40f;
            sleekToggle.SizeOffset_Y = 40f;
            sleekToggle.Value = volume.noWater;
            sleekToggle.AddLabel("No Water", ESleekSide.RIGHT);
            sleekToggle.OnValueChanged += OnNoWaterToggled;
            AddChild(sleekToggle);
            ISleekToggle sleekToggle2 = Glazier.Get().CreateToggle();
            sleekToggle2.PositionOffset_Y = 80f;
            sleekToggle2.SizeOffset_X = 40f;
            sleekToggle2.SizeOffset_Y = 40f;
            sleekToggle2.Value = volume.noLighting;
            sleekToggle2.AddLabel("No Lighting", ESleekSide.RIGHT);
            sleekToggle2.OnValueChanged += OnNoLightingToggled;
            AddChild(sleekToggle2);
            ISleekUInt32Field sleekUInt32Field = Glazier.Get().CreateUInt32Field();
            sleekUInt32Field.PositionOffset_Y = 120f;
            sleekUInt32Field.SizeOffset_X = 200f;
            sleekUInt32Field.SizeOffset_Y = 30f;
            sleekUInt32Field.Value = volume.weatherMask;
            sleekUInt32Field.AddLabel("Weather Mask", ESleekSide.RIGHT);
            sleekUInt32Field.OnValueChanged += OnWeatherMaskChanged;
            AddChild(sleekUInt32Field);
            ISleekToggle sleekToggle3 = Glazier.Get().CreateToggle();
            sleekToggle3.PositionOffset_Y = 150f;
            sleekToggle3.SizeOffset_X = 40f;
            sleekToggle3.SizeOffset_Y = 40f;
            sleekToggle3.Value = volume.overrideFog;
            sleekToggle3.AddLabel("Override Fog", ESleekSide.RIGHT);
            sleekToggle3.OnValueChanged += OnOverrideFogToggled;
            AddChild(sleekToggle3);
            SleekColorPicker sleekColorPicker = new SleekColorPicker
            {
                PositionOffset_Y = 190f,
                state = volume.fogColor
            };
            sleekColorPicker.onColorPicked = (ColorPicked)Delegate.Combine(sleekColorPicker.onColorPicked, new ColorPicked(OnFogColorPicked));
            AddChild(sleekColorPicker);
            ISleekFloat32Field sleekFloat32Field = Glazier.Get().CreateFloat32Field();
            sleekFloat32Field.PositionOffset_Y = 200f + sleekColorPicker.SizeOffset_Y;
            sleekFloat32Field.SizeOffset_X = 200f;
            sleekFloat32Field.SizeOffset_Y = 30f;
            sleekFloat32Field.Value = volume.fogIntensity;
            sleekFloat32Field.AddLabel("Fog Intensity", ESleekSide.RIGHT);
            sleekFloat32Field.OnValueChanged += OnFogIntensityChanged;
            AddChild(sleekFloat32Field);
            ISleekToggle sleekToggle4 = Glazier.Get().CreateToggle();
            sleekToggle4.PositionOffset_Y = sleekFloat32Field.PositionOffset_Y + 40f;
            sleekToggle4.SizeOffset_X = 40f;
            sleekToggle4.SizeOffset_Y = 40f;
            sleekToggle4.Value = volume.overrideAtmosphericFog;
            sleekToggle4.AddLabel("Override Atmospheric Fog", ESleekSide.RIGHT);
            sleekToggle4.OnValueChanged += OnOverrideAtmosphericFogToggled;
            AddChild(sleekToggle4);
            base.SizeOffset_Y = sleekToggle4.PositionOffset_Y + 40f;
        }

        private void OnIdChanged(ISleekField field, string effectIdString)
        {
            if (ushort.TryParse(effectIdString, out volume._id))
            {
                volume._effectGuid = Guid.Empty;
            }
            else if (Guid.TryParse(effectIdString, out volume._effectGuid))
            {
                volume._id = 0;
            }
            else
            {
                volume._effectGuid = Guid.Empty;
                volume._id = 0;
            }
            LevelHierarchy.MarkDirty();
        }

        private void OnNoWaterToggled(ISleekToggle toggle, bool noWater)
        {
            volume.noWater = noWater;
            LevelHierarchy.MarkDirty();
        }

        private void OnNoLightingToggled(ISleekToggle toggle, bool noLighting)
        {
            volume.noLighting = noLighting;
            LevelHierarchy.MarkDirty();
        }

        private void OnWeatherMaskChanged(ISleekUInt32Field field, uint mask)
        {
            volume.weatherMask = mask;
            LevelHierarchy.MarkDirty();
        }

        private void OnOverrideFogToggled(ISleekToggle toggle, bool overrideFog)
        {
            volume.overrideFog = overrideFog;
            LevelHierarchy.MarkDirty();
        }

        private void OnFogColorPicked(SleekColorPicker picker, Color color)
        {
            volume.fogColor = color;
            LevelHierarchy.MarkDirty();
        }

        private void OnFogIntensityChanged(ISleekFloat32Field field, float value)
        {
            volume.fogIntensity = value;
            LevelHierarchy.MarkDirty();
        }

        private void OnOverrideAtmosphericFogToggled(ISleekToggle toggle, bool overrideAtmosphericFog)
        {
            volume.overrideAtmosphericFog = overrideAtmosphericFog;
            LevelHierarchy.MarkDirty();
        }
    }

    [SerializeField]
    internal Guid _effectGuid;

    /// <summary>
    /// Kept because lots of modders have been using this script in Unity,
    /// so removing legacy effect id would break their content.
    /// </summary>
    [SerializeField]
    protected ushort _id;

    [SerializeField]
    protected bool _noWater;

    [SerializeField]
    protected bool _noLighting;

    /// <summary>
    /// If per-weather mask AND is non zero the weather will blend in.
    /// </summary>
    [SerializeField]
    public uint weatherMask = uint.MaxValue;

    [SerializeField]
    protected bool _overrideFog;

    [SerializeField]
    protected Color _fogColor = Color.white;

    [SerializeField]
    protected float _fogIntensity;

    [SerializeField]
    public bool overrideAtmosphericFog;

    public Guid EffectGuid => _effectGuid;

    public ushort id
    {
        [Obsolete]
        get
        {
            return _id;
        }
        set
        {
            _id = value;
        }
    }

    public bool noWater
    {
        get
        {
            return _noWater;
        }
        set
        {
            _noWater = value;
        }
    }

    public bool noLighting
    {
        get
        {
            return _noLighting;
        }
        set
        {
            _noLighting = value;
        }
    }

    public bool overrideFog
    {
        get
        {
            return _overrideFog;
        }
        set
        {
            _overrideFog = value;
        }
    }

    public Color fogColor
    {
        get
        {
            return _fogColor;
        }
        set
        {
            _fogColor = value;
        }
    }

    public float fogIntensity
    {
        get
        {
            return _fogIntensity;
        }
        set
        {
            _fogIntensity = value;
        }
    }

    public override ISleekElement CreateMenu()
    {
        ISleekElement sleekElement = new Menu(this);
        AppendBaseMenu(sleekElement);
        return sleekElement;
    }

    /// <summary>
    /// Used by lighting to get the currently active effect.
    /// </summary>
    public EffectAsset GetEffectAsset()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(_effectGuid, _id);
    }

    protected override void readHierarchyItem(IFormattedFileReader reader)
    {
        base.readHierarchyItem(reader);
        string text = reader.readValue("Ambiance_ID");
        if (ushort.TryParse(text, out _id))
        {
            _effectGuid = Guid.Empty;
        }
        else if (Guid.TryParse(text, out _effectGuid))
        {
            _id = 0;
        }
        noWater = reader.readValue<bool>("No_Water");
        noLighting = reader.readValue<bool>("No_Lighting");
        if (reader.containsKey("Weather_Mask"))
        {
            weatherMask = reader.readValue<uint>("Weather_Mask");
        }
        else
        {
            weatherMask = uint.MaxValue;
            if (reader.containsKey("Can_Rain") && !reader.readValue<bool>("Can_Rain"))
            {
                weatherMask &= 4294967294u;
            }
            if (reader.containsKey("Can_Snow") && !reader.readValue<bool>("Can_Snow"))
            {
                weatherMask &= 4294967293u;
            }
        }
        overrideFog = reader.readValue<bool>("Override_Fog");
        fogColor = reader.readValue<Color>("Fog_Color");
        if (reader.containsKey("Fog_Intensity"))
        {
            fogIntensity = reader.readValue<float>("Fog_Intensity");
        }
        else
        {
            float value = reader.readValue<float>("Fog_Height");
            fogIntensity = Mathf.InverseLerp(-1024f, 1024f, value);
        }
        overrideAtmosphericFog = reader.readValue<bool>("Override_Atmospheric_Fog");
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        if (!_effectGuid.IsEmpty())
        {
            writer.writeValue("Ambiance_ID", _effectGuid);
        }
        else
        {
            writer.writeValue("Ambiance_ID", _id);
        }
        writer.writeValue("No_Water", noWater);
        writer.writeValue("No_Lighting", noLighting);
        writer.writeValue("Weather_Mask", weatherMask);
        writer.writeValue("Override_Fog", overrideFog);
        writer.writeValue("Fog_Color", fogColor);
        writer.writeValue("Fog_Intensity", fogIntensity);
        writer.writeValue("Override_Atmospheric_Fog", overrideAtmosphericFog);
    }
}
