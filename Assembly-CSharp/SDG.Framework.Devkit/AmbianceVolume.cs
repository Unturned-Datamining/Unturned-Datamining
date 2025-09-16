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

        private SleekButtonStateEnum<EAmbianceVolumeFogOverrideMode> fogMode;

        private SleekButtonStateEnum<ELightingTime> timeButton;

        private SleekColorPicker fogColor;

        private ISleekFloat32Field fogIntensityField;

        private ISleekFloat32Field audioFadeInField;

        private ISleekFloat32Field audioFadeOutField;

        private ISleekFloat32Field fogFadeInField;

        private ISleekFloat32Field fogFadeOutField;

        private ISleekFloat32Field lightingFadeInField;

        private ISleekFloat32Field lightingFadeOutField;

        public Menu(AmbianceVolume volume)
        {
            this.volume = volume;
            base.SizeOffset_X = 400f;
            float num = 0f;
            ISleekField sleekField = Glazier.Get().CreateStringField();
            sleekField.PositionOffset_Y = num;
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
            num += sleekField.SizeOffset_Y + 10f;
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.PositionOffset_Y = num;
            sleekToggle.SizeOffset_X = 40f;
            sleekToggle.SizeOffset_Y = 40f;
            sleekToggle.Value = volume.noWater;
            sleekToggle.AddLabel("No Water", ESleekSide.RIGHT);
            sleekToggle.OnValueChanged += OnNoWaterToggled;
            AddChild(sleekToggle);
            num += sleekToggle.SizeOffset_Y + 10f;
            ISleekToggle sleekToggle2 = Glazier.Get().CreateToggle();
            sleekToggle2.PositionOffset_Y = num;
            sleekToggle2.SizeOffset_X = 40f;
            sleekToggle2.SizeOffset_Y = 40f;
            sleekToggle2.Value = volume.noLighting;
            sleekToggle2.AddLabel("No Lighting", ESleekSide.RIGHT);
            sleekToggle2.OnValueChanged += OnNoLightingToggled;
            AddChild(sleekToggle2);
            num += sleekToggle2.SizeOffset_Y + 10f;
            ISleekUInt32Field sleekUInt32Field = Glazier.Get().CreateUInt32Field();
            sleekUInt32Field.PositionOffset_Y = num;
            sleekUInt32Field.SizeOffset_X = 200f;
            sleekUInt32Field.SizeOffset_Y = 30f;
            sleekUInt32Field.Value = volume.weatherMask;
            sleekUInt32Field.AddLabel("Weather Mask", ESleekSide.RIGHT);
            sleekUInt32Field.OnValueChanged += OnWeatherMaskChanged;
            AddChild(sleekUInt32Field);
            num += sleekUInt32Field.SizeOffset_Y + 10f;
            fogMode = new SleekButtonStateEnum<EAmbianceVolumeFogOverrideMode>();
            fogMode.PositionOffset_Y = num;
            fogMode.SizeOffset_X = 200f;
            fogMode.SizeOffset_Y = 30f;
            fogMode.SetEnum(volume.FogOverrideMode);
            fogMode.AddLabel("Override Fog", ESleekSide.RIGHT);
            SleekButtonStateEnum<EAmbianceVolumeFogOverrideMode> sleekButtonStateEnum = fogMode;
            sleekButtonStateEnum.OnSwappedEnum = (Action<SleekButtonStateEnum<EAmbianceVolumeFogOverrideMode>, EAmbianceVolumeFogOverrideMode>)Delegate.Combine(sleekButtonStateEnum.OnSwappedEnum, new Action<SleekButtonStateEnum<EAmbianceVolumeFogOverrideMode>, EAmbianceVolumeFogOverrideMode>(OnFogModeChanged));
            AddChild(fogMode);
            num += fogMode.SizeOffset_Y + 10f;
            timeButton = new SleekButtonStateEnum<ELightingTime>();
            timeButton.PositionOffset_Y = num;
            timeButton.SizeOffset_X = 200f;
            timeButton.SizeOffset_Y = 30f;
            timeButton.AddLabel("Fog Time", ESleekSide.RIGHT);
            SleekButtonStateEnum<ELightingTime> sleekButtonStateEnum2 = timeButton;
            sleekButtonStateEnum2.OnSwappedEnum = (Action<SleekButtonStateEnum<ELightingTime>, ELightingTime>)Delegate.Combine(sleekButtonStateEnum2.OnSwappedEnum, new Action<SleekButtonStateEnum<ELightingTime>, ELightingTime>(OnFogTimeChanged));
            AddChild(timeButton);
            num += timeButton.SizeOffset_Y + 10f;
            fogColor = new SleekColorPicker();
            fogColor.PositionOffset_Y = num;
            SleekColorPicker sleekColorPicker = fogColor;
            sleekColorPicker.onColorPicked = (ColorPicked)Delegate.Combine(sleekColorPicker.onColorPicked, new ColorPicked(OnFogColorPicked));
            AddChild(fogColor);
            num += fogColor.SizeOffset_Y + 10f;
            fogIntensityField = Glazier.Get().CreateFloat32Field();
            fogIntensityField.PositionOffset_Y = num;
            fogIntensityField.SizeOffset_X = 200f;
            fogIntensityField.SizeOffset_Y = 30f;
            fogIntensityField.AddLabel("Fog Intensity", ESleekSide.RIGHT);
            fogIntensityField.OnValueChanged += OnFogIntensityChanged;
            AddChild(fogIntensityField);
            num += fogIntensityField.SizeOffset_Y + 10f;
            ISleekToggle sleekToggle3 = Glazier.Get().CreateToggle();
            sleekToggle3.PositionOffset_Y = num;
            sleekToggle3.SizeOffset_X = 40f;
            sleekToggle3.SizeOffset_Y = 40f;
            sleekToggle3.Value = volume.overrideAtmosphericFog;
            sleekToggle3.AddLabel("Override Atmospheric Fog", ESleekSide.RIGHT);
            sleekToggle3.OnValueChanged += OnOverrideAtmosphericFogToggled;
            AddChild(sleekToggle3);
            num += sleekToggle3.SizeOffset_Y + 10f;
            ISleekToggle sleekToggle4 = Glazier.Get().CreateToggle();
            sleekToggle4.PositionOffset_Y = num;
            sleekToggle4.SizeOffset_X = 40f;
            sleekToggle4.SizeOffset_Y = 40f;
            sleekToggle4.Value = volume.enableFalloff;
            sleekToggle4.AddLabel("Use Falloff", ESleekSide.RIGHT);
            sleekToggle4.OnValueChanged += OnEnableFalloffToggled;
            AddChild(sleekToggle4);
            num += sleekToggle4.SizeOffset_Y + 10f;
            ISleekInt32Field sleekInt32Field = Glazier.Get().CreateInt32Field();
            sleekInt32Field.PositionOffset_Y = num;
            sleekInt32Field.SizeOffset_X = 200f;
            sleekInt32Field.SizeOffset_Y = 30f;
            sleekInt32Field.Value = volume.priority;
            sleekInt32Field.AddLabel("Priority", ESleekSide.RIGHT);
            sleekInt32Field.OnValueChanged += OnPriorityChanged;
            AddChild(sleekInt32Field);
            num += sleekInt32Field.SizeOffset_Y + 10f;
            audioFadeInField = Glazier.Get().CreateFloat32Field();
            audioFadeInField.PositionOffset_Y = num;
            audioFadeInField.SizeOffset_X = 200f;
            audioFadeInField.SizeOffset_Y = 30f;
            audioFadeInField.Value = volume.audioFadeInDuration;
            audioFadeInField.AddLabel("Audio Fade-In", ESleekSide.RIGHT);
            audioFadeInField.TooltipText = "Seconds for effect audio to fade in when distance falloff is disabled.";
            audioFadeInField.OnValueChanged += OnAudioFadeInDurationChanged;
            AddChild(audioFadeInField);
            num += audioFadeInField.SizeOffset_Y + 10f;
            audioFadeOutField = Glazier.Get().CreateFloat32Field();
            audioFadeOutField.PositionOffset_Y = num;
            audioFadeOutField.SizeOffset_X = 200f;
            audioFadeOutField.SizeOffset_Y = 30f;
            audioFadeOutField.Value = volume.audioFadeOutDuration;
            audioFadeOutField.AddLabel("Audio Fade-Out", ESleekSide.RIGHT);
            audioFadeOutField.TooltipText = "Seconds for effect audio to fade out when distance falloff is disabled.";
            audioFadeOutField.OnValueChanged += OnAudioFadeOutDurationChanged;
            AddChild(audioFadeOutField);
            num += audioFadeOutField.SizeOffset_Y + 10f;
            fogFadeInField = Glazier.Get().CreateFloat32Field();
            fogFadeInField.PositionOffset_Y = num;
            fogFadeInField.SizeOffset_X = 200f;
            fogFadeInField.SizeOffset_Y = 30f;
            fogFadeInField.Value = volume.fogFadeInDuration;
            fogFadeInField.AddLabel("Fog Fade-In", ESleekSide.RIGHT);
            fogFadeInField.TooltipText = "Seconds for fog to fade in when distance falloff is disabled.";
            fogFadeInField.OnValueChanged += OnFogFadeInDurationChanged;
            AddChild(fogFadeInField);
            num += fogFadeInField.SizeOffset_Y + 10f;
            fogFadeOutField = Glazier.Get().CreateFloat32Field();
            fogFadeOutField.PositionOffset_Y = num;
            fogFadeOutField.SizeOffset_X = 200f;
            fogFadeOutField.SizeOffset_Y = 30f;
            fogFadeOutField.Value = volume.fogFadeOutDuration;
            fogFadeOutField.AddLabel("Fog Fade-Out", ESleekSide.RIGHT);
            fogFadeOutField.TooltipText = "Seconds for fog to fade out when distance falloff is disabled.";
            fogFadeOutField.OnValueChanged += OnFogFadeOutDurationChanged;
            AddChild(fogFadeOutField);
            num += fogFadeOutField.SizeOffset_Y + 10f;
            lightingFadeInField = Glazier.Get().CreateFloat32Field();
            lightingFadeInField.PositionOffset_Y = num;
            lightingFadeInField.SizeOffset_X = 200f;
            lightingFadeInField.SizeOffset_Y = 30f;
            lightingFadeInField.Value = volume.lightingFadeInDuration;
            lightingFadeInField.AddLabel("Lighting Fade-In", ESleekSide.RIGHT);
            lightingFadeInField.TooltipText = "Seconds for lighting to fade in when distance falloff is disabled.";
            lightingFadeInField.OnValueChanged += OnLightingFadeInDurationChanged;
            AddChild(lightingFadeInField);
            num += lightingFadeInField.SizeOffset_Y + 10f;
            lightingFadeOutField = Glazier.Get().CreateFloat32Field();
            lightingFadeOutField.PositionOffset_Y = num;
            lightingFadeOutField.SizeOffset_X = 200f;
            lightingFadeOutField.SizeOffset_Y = 30f;
            lightingFadeOutField.Value = volume.lightingFadeOutDuration;
            lightingFadeOutField.AddLabel("Lighting Fade-Out", ESleekSide.RIGHT);
            lightingFadeOutField.TooltipText = "Seconds for lighting to fade out when distance falloff is disabled.";
            lightingFadeOutField.OnValueChanged += OnLightingFadeOutDurationChanged;
            AddChild(lightingFadeOutField);
            num += lightingFadeOutField.SizeOffset_Y + 10f;
            UpdateFogSettings();
            UpdateFade();
            base.SizeOffset_Y = num - 10f;
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

        private void OnFogModeChanged(SleekButtonStateEnum<EAmbianceVolumeFogOverrideMode> button, EAmbianceVolumeFogOverrideMode newFogMode)
        {
            volume.FogOverrideMode = newFogMode;
            UpdateFogSettings();
            LevelHierarchy.MarkDirty();
        }

        private void OnFogTimeChanged(SleekButtonStateEnum<ELightingTime> button, ELightingTime newTime)
        {
            UpdateFogSettings();
        }

        private void OnFogColorPicked(SleekColorPicker picker, Color color)
        {
            if (volume.FogOverrideMode == EAmbianceVolumeFogOverrideMode.PerTimeOfDay)
            {
                volume.GetFogSettings(timeButton.GetEnum()).fogColor = color;
            }
            else
            {
                volume.fogColor = color;
            }
            LevelHierarchy.MarkDirty();
        }

        private void OnFogIntensityChanged(ISleekFloat32Field field, float value)
        {
            if (volume.FogOverrideMode == EAmbianceVolumeFogOverrideMode.PerTimeOfDay)
            {
                volume.GetFogSettings(timeButton.GetEnum()).fogIntensity = value;
            }
            else
            {
                volume.fogIntensity = value;
            }
            LevelHierarchy.MarkDirty();
        }

        private void OnOverrideAtmosphericFogToggled(ISleekToggle toggle, bool overrideAtmosphericFog)
        {
            volume.overrideAtmosphericFog = overrideAtmosphericFog;
            LevelHierarchy.MarkDirty();
        }

        private void OnEnableFalloffToggled(ISleekToggle toggle, bool enableFalloff)
        {
            volume.enableFalloff = enableFalloff;
            UpdateFade();
            LevelHierarchy.MarkDirty();
        }

        private void OnPriorityChanged(ISleekInt32Field field, int value)
        {
            volume.priority = value;
            LevelHierarchy.MarkDirty();
        }

        private void OnAudioFadeInDurationChanged(ISleekFloat32Field field, float value)
        {
            volume.audioFadeInDuration = value;
            LevelHierarchy.MarkDirty();
        }

        private void OnAudioFadeOutDurationChanged(ISleekFloat32Field field, float value)
        {
            volume.audioFadeOutDuration = value;
            LevelHierarchy.MarkDirty();
        }

        private void OnFogFadeInDurationChanged(ISleekFloat32Field field, float value)
        {
            volume.fogFadeInDuration = value;
            LevelHierarchy.MarkDirty();
        }

        private void OnFogFadeOutDurationChanged(ISleekFloat32Field field, float value)
        {
            volume.fogFadeOutDuration = value;
            LevelHierarchy.MarkDirty();
        }

        private void OnLightingFadeInDurationChanged(ISleekFloat32Field field, float value)
        {
            volume.lightingFadeInDuration = value;
            LevelHierarchy.MarkDirty();
        }

        private void OnLightingFadeOutDurationChanged(ISleekFloat32Field field, float value)
        {
            volume.lightingFadeOutDuration = value;
            LevelHierarchy.MarkDirty();
        }

        private void UpdateFade()
        {
            bool isClickable = !volume.enableFalloff;
            audioFadeInField.IsClickable = isClickable;
            audioFadeOutField.IsClickable = isClickable;
            fogFadeInField.IsClickable = isClickable;
            fogFadeOutField.IsClickable = isClickable;
            lightingFadeInField.IsClickable = isClickable;
            lightingFadeOutField.IsClickable = isClickable;
        }

        private void UpdateFogSettings()
        {
            timeButton.isInteractable = volume.FogOverrideMode == EAmbianceVolumeFogOverrideMode.PerTimeOfDay;
            if (volume.FogOverrideMode == EAmbianceVolumeFogOverrideMode.PerTimeOfDay)
            {
                ref AmbianceVolumeTimeOfDaySettings fogSettings = ref volume.GetFogSettings(timeButton.GetEnum());
                fogColor.state = fogSettings.fogColor;
                fogIntensityField.Value = fogSettings.fogIntensity;
            }
            else
            {
                fogColor.state = volume.fogColor;
                fogIntensityField.Value = volume.fogIntensity;
            }
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
    protected EAmbianceVolumeFogOverrideMode _fogOverrideMode;

    /// <summary>
    /// Kept for backwards compatibility with fog volumes created in Unity / by mods.
    /// </summary>
    [SerializeField]
    protected bool _overrideFog;

    [SerializeField]
    protected Color _fogColor = Color.white;

    [SerializeField]
    protected float _fogIntensity;

    [SerializeField]
    internal AmbianceVolumeTimeOfDaySettings[] perTimeOfDaySettings;

    [SerializeField]
    public bool overrideAtmosphericFog;

    /// <summary>
    /// Distinguishes from zero falloff which may be useful deep in a cave.
    /// </summary>
    [SerializeField]
    public bool enableFalloff;

    /// <summary>
    /// Higher priority volumes override lower priority volumes.
    /// </summary>
    public int priority;

    /// <summary>
    /// When falloff is OFF, how long to fade in audio by time.
    /// </summary>
    public float audioFadeInDuration = 2f;

    /// <summary>
    /// When falloff is OFF, how long to fade out audio by time.
    /// </summary>
    public float audioFadeOutDuration = 2f;

    /// <summary>
    /// When falloff is OFF, how long to fade in audio by time.
    /// </summary>
    public float fogFadeInDuration = 20f;

    /// <summary>
    /// When falloff is OFF, how long to fade out audio by time.
    /// </summary>
    public float fogFadeOutDuration = 8f;

    /// <summary>
    /// When falloff is OFF, how long to fade in lighting by time.
    /// </summary>
    public float lightingFadeInDuration = 4f;

    /// <summary>
    /// When falloff is OFF, how long to fade out lighting by time.
    /// </summary>
    public float lightingFadeOutDuration = 4f;

    private EffectAsset cachedEffectAsset;

    public Guid EffectGuid
    {
        get
        {
            return _effectGuid;
        }
        set
        {
            _effectGuid = value;
            cachedEffectAsset = null;
        }
    }

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
            cachedEffectAsset = null;
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

    public EAmbianceVolumeFogOverrideMode FogOverrideMode
    {
        get
        {
            return _fogOverrideMode;
        }
        set
        {
            _fogOverrideMode = value;
        }
    }

    [Obsolete]
    public bool overrideFog
    {
        get
        {
            return _overrideFog;
        }
        set
        {
            _overrideFog = false;
            _fogOverrideMode = (value ? EAmbianceVolumeFogOverrideMode.Constant : EAmbianceVolumeFogOverrideMode.PerTimeOfDay);
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
        if (cachedEffectAsset == null || cachedEffectAsset.hasBeenReplaced)
        {
            cachedEffectAsset = Assets.FindEffectAssetByGuidOrLegacyId(_effectGuid, _id);
        }
        return cachedEffectAsset;
    }

    internal void GetFogSettings(int blendKey, int currentKey, float timeAlpha, out Color overrideFogColor, out float overrideFogIntensity)
    {
        if (_fogOverrideMode != EAmbianceVolumeFogOverrideMode.PerTimeOfDay)
        {
            overrideFogColor = fogColor;
            overrideFogIntensity = fogIntensity;
            return;
        }
        if (perTimeOfDaySettings == null || perTimeOfDaySettings.Length != 4)
        {
            overrideFogColor = Color.white;
            overrideFogIntensity = 0f;
            return;
        }
        ref AmbianceVolumeTimeOfDaySettings reference = ref perTimeOfDaySettings[currentKey];
        ref AmbianceVolumeTimeOfDaySettings reference2 = ref perTimeOfDaySettings[(blendKey == -1) ? currentKey : blendKey];
        overrideFogColor = Color.Lerp(reference2.fogColor, reference.fogColor, timeAlpha);
        overrideFogIntensity = Mathf.Lerp(reference2.fogIntensity, reference.fogIntensity, timeAlpha);
    }

    internal ref AmbianceVolumeTimeOfDaySettings GetFogSettings(ELightingTime time)
    {
        if (perTimeOfDaySettings == null || perTimeOfDaySettings.Length != 4)
        {
            perTimeOfDaySettings = new AmbianceVolumeTimeOfDaySettings[4];
            for (int i = 0; i < 4; i++)
            {
                ref AmbianceVolumeTimeOfDaySettings reference = ref perTimeOfDaySettings[i];
                reference.fogColor = Color.white;
                reference.fogIntensity = 1f;
            }
        }
        return ref perTimeOfDaySettings[(int)time];
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
        if (reader.containsKey("Override_Fog_Mode"))
        {
            _fogOverrideMode = reader.readValue<EAmbianceVolumeFogOverrideMode>("Override_Fog_Mode");
            if (_fogOverrideMode == EAmbianceVolumeFogOverrideMode.PerTimeOfDay)
            {
                perTimeOfDaySettings = new AmbianceVolumeTimeOfDaySettings[4];
                perTimeOfDaySettings[0] = new AmbianceVolumeTimeOfDaySettings(reader.readObject("Dawn"));
                perTimeOfDaySettings[1] = new AmbianceVolumeTimeOfDaySettings(reader.readObject("Midday"));
                perTimeOfDaySettings[2] = new AmbianceVolumeTimeOfDaySettings(reader.readObject("Dusk"));
                perTimeOfDaySettings[3] = new AmbianceVolumeTimeOfDaySettings(reader.readObject("Midnight"));
            }
        }
        else if (reader.readValue<bool>("Override_Fog"))
        {
            _fogOverrideMode = EAmbianceVolumeFogOverrideMode.Constant;
        }
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
        enableFalloff = reader.readValue<bool>("Enable_Falloff");
        priority = reader.readValue<int>("Priority");
        if (reader.containsKey("Audio_FadeIn"))
        {
            audioFadeInDuration = reader.readValue<float>("Audio_FadeIn");
        }
        if (reader.containsKey("Audio_FadeOut"))
        {
            audioFadeOutDuration = reader.readValue<float>("Audio_FadeOut");
        }
        if (reader.containsKey("Fog_FadeIn"))
        {
            fogFadeInDuration = reader.readValue<float>("Fog_FadeIn");
        }
        if (reader.containsKey("Fog_FadeOut"))
        {
            fogFadeOutDuration = reader.readValue<float>("Fog_FadeOut");
        }
        if (reader.containsKey("Lighting_FadeIn"))
        {
            lightingFadeInDuration = reader.readValue<float>("Lighting_FadeIn");
        }
        if (reader.containsKey("Lighting_FadeOut"))
        {
            lightingFadeOutDuration = reader.readValue<float>("Lighting_FadeOut");
        }
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
        writer.writeValue("Override_Fog_Mode", _fogOverrideMode);
        if (_fogOverrideMode == EAmbianceVolumeFogOverrideMode.PerTimeOfDay && perTimeOfDaySettings != null && perTimeOfDaySettings.Length == 4)
        {
            writer.beginObject("Dawn");
            perTimeOfDaySettings[0].Write(writer);
            writer.endObject();
            writer.beginObject("Midday");
            perTimeOfDaySettings[1].Write(writer);
            writer.endObject();
            writer.beginObject("Dusk");
            perTimeOfDaySettings[2].Write(writer);
            writer.endObject();
            writer.beginObject("Midnight");
            perTimeOfDaySettings[3].Write(writer);
            writer.endObject();
        }
        writer.writeValue("Fog_Color", fogColor);
        writer.writeValue("Fog_Intensity", fogIntensity);
        writer.writeValue("Override_Atmospheric_Fog", overrideAtmosphericFog);
        writer.writeValue("Enable_Falloff", enableFalloff);
        writer.writeValue("Priority", priority);
        writer.writeValue("Audio_FadeIn", audioFadeInDuration);
        writer.writeValue("Audio_FadeOut", audioFadeOutDuration);
        writer.writeValue("Fog_FadeIn", fogFadeInDuration);
        writer.writeValue("Fog_FadeOut", fogFadeOutDuration);
        writer.writeValue("Lighting_FadeIn", lightingFadeInDuration);
        writer.writeValue("Lighting_FadeOut", lightingFadeOutDuration);
    }

    protected override void Awake()
    {
        supportsFalloff = true;
        if (_overrideFog)
        {
            _overrideFog = false;
            _fogOverrideMode = EAmbianceVolumeFogOverrideMode.Constant;
        }
        base.Awake();
    }
}
