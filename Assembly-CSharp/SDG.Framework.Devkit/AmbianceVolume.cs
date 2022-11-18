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
            base.sizeOffset_X = 400;
            ISleekField sleekField = Glazier.Get().CreateStringField();
            sleekField.sizeOffset_X = 200;
            sleekField.sizeOffset_Y = 30;
            if (volume._effectGuid.IsEmpty())
            {
                sleekField.text = volume._id.ToString();
            }
            else
            {
                sleekField.text = volume._effectGuid.ToString("N");
            }
            sleekField.addLabel("Effect ID", ESleekSide.RIGHT);
            sleekField.onTyped += OnIdChanged;
            AddChild(sleekField);
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.positionOffset_Y = 40;
            sleekToggle.sizeOffset_X = 40;
            sleekToggle.sizeOffset_Y = 40;
            sleekToggle.state = volume.noWater;
            sleekToggle.addLabel("No Water", ESleekSide.RIGHT);
            sleekToggle.onToggled += OnNoWaterToggled;
            AddChild(sleekToggle);
            ISleekToggle sleekToggle2 = Glazier.Get().CreateToggle();
            sleekToggle2.positionOffset_Y = 80;
            sleekToggle2.sizeOffset_X = 40;
            sleekToggle2.sizeOffset_Y = 40;
            sleekToggle2.state = volume.noLighting;
            sleekToggle2.addLabel("No Lighting", ESleekSide.RIGHT);
            sleekToggle2.onToggled += OnNoLightingToggled;
            AddChild(sleekToggle2);
            ISleekUInt32Field sleekUInt32Field = Glazier.Get().CreateUInt32Field();
            sleekUInt32Field.positionOffset_Y = 120;
            sleekUInt32Field.sizeOffset_X = 200;
            sleekUInt32Field.sizeOffset_Y = 30;
            sleekUInt32Field.state = volume.weatherMask;
            sleekUInt32Field.addLabel("Weather Mask", ESleekSide.RIGHT);
            sleekUInt32Field.onTypedUInt32 += OnWeatherMaskChanged;
            AddChild(sleekUInt32Field);
            ISleekToggle sleekToggle3 = Glazier.Get().CreateToggle();
            sleekToggle3.positionOffset_Y = 150;
            sleekToggle3.sizeOffset_X = 40;
            sleekToggle3.sizeOffset_Y = 40;
            sleekToggle3.state = volume.overrideFog;
            sleekToggle3.addLabel("Override Fog", ESleekSide.RIGHT);
            sleekToggle3.onToggled += OnOverrideFogToggled;
            AddChild(sleekToggle3);
            SleekColorPicker sleekColorPicker = new SleekColorPicker
            {
                positionOffset_Y = 190,
                state = volume.fogColor
            };
            sleekColorPicker.onColorPicked = (ColorPicked)Delegate.Combine(sleekColorPicker.onColorPicked, new ColorPicked(OnFogColorPicked));
            AddChild(sleekColorPicker);
            ISleekFloat32Field sleekFloat32Field = Glazier.Get().CreateFloat32Field();
            sleekFloat32Field.positionOffset_Y = 200 + sleekColorPicker.sizeOffset_Y;
            sleekFloat32Field.sizeOffset_X = 200;
            sleekFloat32Field.sizeOffset_Y = 30;
            sleekFloat32Field.state = volume.fogIntensity;
            sleekFloat32Field.addLabel("Fog Intensity", ESleekSide.RIGHT);
            sleekFloat32Field.onTypedSingle += OnFogIntensityChanged;
            AddChild(sleekFloat32Field);
            ISleekToggle sleekToggle4 = Glazier.Get().CreateToggle();
            sleekToggle4.positionOffset_Y = sleekFloat32Field.positionOffset_Y + 40;
            sleekToggle4.sizeOffset_X = 40;
            sleekToggle4.sizeOffset_Y = 40;
            sleekToggle4.state = volume.overrideAtmosphericFog;
            sleekToggle4.addLabel("Override Atmospheric Fog", ESleekSide.RIGHT);
            sleekToggle4.onToggled += OnOverrideAtmosphericFogToggled;
            AddChild(sleekToggle4);
            base.sizeOffset_Y = sleekToggle4.positionOffset_Y + 40;
        }

        private void OnIdChanged(ISleekField field, string effectIdString)
        {
            if (ushort.TryParse(effectIdString, out volume._id))
            {
                volume._effectGuid = Guid.Empty;
                return;
            }
            if (Guid.TryParse(effectIdString, out volume._effectGuid))
            {
                volume._id = 0;
                return;
            }
            volume._effectGuid = Guid.Empty;
            volume._id = 0;
        }

        private void OnNoWaterToggled(ISleekToggle toggle, bool noWater)
        {
            volume.noWater = noWater;
        }

        private void OnNoLightingToggled(ISleekToggle toggle, bool noLighting)
        {
            volume.noLighting = noLighting;
        }

        private void OnWeatherMaskChanged(ISleekUInt32Field field, uint mask)
        {
            volume.weatherMask = mask;
        }

        private void OnOverrideFogToggled(ISleekToggle toggle, bool overrideFog)
        {
            volume.overrideFog = overrideFog;
        }

        private void OnFogColorPicked(SleekColorPicker picker, Color color)
        {
            volume.fogColor = color;
        }

        private void OnFogIntensityChanged(ISleekFloat32Field field, float value)
        {
            volume.fogIntensity = value;
        }

        private void OnOverrideAtmosphericFogToggled(ISleekToggle toggle, bool overrideAtmosphericFog)
        {
            volume.overrideAtmosphericFog = overrideAtmosphericFog;
        }
    }

    [SerializeField]
    internal Guid _effectGuid;

    [SerializeField]
    protected ushort _id;

    [SerializeField]
    protected bool _noWater;

    [SerializeField]
    protected bool _noLighting;

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
