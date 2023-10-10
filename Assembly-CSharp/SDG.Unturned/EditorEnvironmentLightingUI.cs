using System;
using UnityEngine;

namespace SDG.Unturned;

public class EditorEnvironmentLightingUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static ISleekScrollView lightingScrollBox;

    private static ISleekSlider azimuthSlider;

    private static ISleekSlider biasSlider;

    private static ISleekSlider fadeSlider;

    private static ISleekButton[] timeButtons;

    private static ISleekBox[] infoBoxes;

    private static SleekColorPicker[] colorPickers;

    private static ISleekSlider[] singleSliders;

    private static ELightingTime selectedTime;

    private static SleekValue seaLevelSlider;

    private static SleekValue snowLevelSlider;

    private static ISleekFloat32Field rainFreqField;

    private static ISleekFloat32Field rainDurField;

    private static ISleekFloat32Field snowFreqField;

    private static ISleekFloat32Field snowDurField;

    private static ISleekToggle rainToggle;

    private static ISleekToggle snowToggle;

    private static ISleekField weatherGuidField;

    private static ISleekButton previewWeatherButton;

    private static ISleekSlider moonSlider;

    private static ISleekSlider timeSlider;

    public static void open()
    {
        if (!active)
        {
            active = true;
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            container.AnimateOutOfView(1f, 0f);
        }
    }

    private static void onDraggedAzimuthSlider(ISleekSlider slider, float state)
    {
        LevelLighting.azimuth = state * 360f;
    }

    private static void onDraggedBiasSlider(ISleekSlider slider, float state)
    {
        LevelLighting.bias = state;
    }

    private static void onDraggedFadeSlider(ISleekSlider slider, float state)
    {
        LevelLighting.fade = state;
    }

    private static void onValuedSeaLevelSlider(SleekValue slider, float state)
    {
        LevelLighting.seaLevel = state;
    }

    private static void onValuedSnowLevelSlider(SleekValue slider, float state)
    {
        LevelLighting.snowLevel = state;
    }

    private static void onToggledRainToggle(ISleekToggle toggle, bool state)
    {
        LevelLighting.canRain = state;
    }

    private static void onToggledSnowToggle(ISleekToggle toggle, bool state)
    {
        LevelLighting.canSnow = state;
    }

    private static void onTypedRainFreqField(ISleekFloat32Field field, float state)
    {
        LevelLighting.rainFreq = state;
    }

    private static void onTypedRainDurField(ISleekFloat32Field field, float state)
    {
        LevelLighting.rainDur = state;
    }

    private static void onTypedSnowFreqField(ISleekFloat32Field field, float state)
    {
        LevelLighting.snowFreq = state;
    }

    private static void onTypedSnowDurField(ISleekFloat32Field field, float state)
    {
        LevelLighting.snowDur = state;
    }

    private static void onDraggedMoonSlider(ISleekSlider slider, float state)
    {
        byte b = (byte)(state * (float)(int)LevelLighting.MOON_CYCLES);
        if (b >= LevelLighting.MOON_CYCLES)
        {
            b = (byte)(LevelLighting.MOON_CYCLES - 1);
        }
        LevelLighting.moon = b;
    }

    private static void onDraggedTimeSlider(ISleekSlider slider, float state)
    {
        LevelLighting.time = state;
    }

    private static void onClickedTimeButton(ISleekElement button)
    {
        int i;
        for (i = 0; i < timeButtons.Length && timeButtons[i] != button; i++)
        {
        }
        selectedTime = (ELightingTime)i;
        updateSelection();
        switch (selectedTime)
        {
        case ELightingTime.DAWN:
            LevelLighting.time = 0f;
            break;
        case ELightingTime.MIDDAY:
            LevelLighting.time = LevelLighting.bias / 2f;
            break;
        case ELightingTime.DUSK:
            LevelLighting.time = LevelLighting.bias;
            break;
        case ELightingTime.MIDNIGHT:
            LevelLighting.time = 1f - (1f - LevelLighting.bias) / 2f;
            break;
        }
        timeSlider.Value = LevelLighting.time;
    }

    private static void OnClickedPreviewWeather(ISleekElement button)
    {
        Guid result;
        WeatherAssetBase weatherAssetBase = ((!Guid.TryParse(weatherGuidField.Text, out result)) ? null : Assets.find(new AssetReference<WeatherAssetBase>(result)));
        WeatherAssetBase activeWeatherAsset = LevelLighting.GetActiveWeatherAsset();
        if (activeWeatherAsset != null && (activeWeatherAsset == weatherAssetBase || activeWeatherAsset.GUID == result))
        {
            LevelLighting.SetActiveWeatherAsset(null, 0f, default(NetId));
        }
        else
        {
            LevelLighting.SetActiveWeatherAsset(weatherAssetBase, 0f, default(NetId));
        }
    }

    private static void onPickedColorPicker(SleekColorPicker picker, Color state)
    {
        int i;
        for (i = 0; i < colorPickers.Length && colorPickers[i] != picker; i++)
        {
        }
        LevelLighting.times[(int)selectedTime].colors[i] = state;
        LevelLighting.updateLighting();
    }

    private static void onDraggedSingleSlider(ISleekSlider slider, float state)
    {
        int i;
        for (i = 0; i < singleSliders.Length && singleSliders[i] != slider; i++)
        {
        }
        LevelLighting.times[(int)selectedTime].singles[i] = state;
        LevelLighting.updateLighting();
    }

    private static void updateSelection()
    {
        for (int i = 0; i < colorPickers.Length; i++)
        {
            colorPickers[i].state = LevelLighting.times[(int)selectedTime].colors[i];
        }
        for (int j = 0; j < singleSliders.Length; j++)
        {
            singleSliders[j].Value = LevelLighting.times[(int)selectedTime].singles[j];
        }
    }

    public EditorEnvironmentLightingUI()
    {
        Local local = Localization.read("/Editor/EditorEnvironmentLighting.dat");
        container = new SleekFullscreenBox();
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_X = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        selectedTime = ELightingTime.DAWN;
        azimuthSlider = Glazier.Get().CreateSlider();
        azimuthSlider.PositionOffset_X = -230f;
        azimuthSlider.PositionOffset_Y = 80f;
        azimuthSlider.PositionScale_X = 1f;
        azimuthSlider.SizeOffset_X = 230f;
        azimuthSlider.SizeOffset_Y = 20f;
        azimuthSlider.Value = LevelLighting.azimuth / 360f;
        azimuthSlider.Orientation = ESleekOrientation.HORIZONTAL;
        azimuthSlider.AddLabel(local.format("AzimuthSliderLabelText"), ESleekSide.LEFT);
        azimuthSlider.OnValueChanged += onDraggedAzimuthSlider;
        container.AddChild(azimuthSlider);
        biasSlider = Glazier.Get().CreateSlider();
        biasSlider.PositionOffset_X = -230f;
        biasSlider.PositionOffset_Y = 110f;
        biasSlider.PositionScale_X = 1f;
        biasSlider.SizeOffset_X = 230f;
        biasSlider.SizeOffset_Y = 20f;
        biasSlider.Value = LevelLighting.bias;
        biasSlider.Orientation = ESleekOrientation.HORIZONTAL;
        biasSlider.AddLabel(local.format("BiasSliderLabelText"), ESleekSide.LEFT);
        biasSlider.OnValueChanged += onDraggedBiasSlider;
        container.AddChild(biasSlider);
        fadeSlider = Glazier.Get().CreateSlider();
        fadeSlider.PositionOffset_X = -230f;
        fadeSlider.PositionOffset_Y = 140f;
        fadeSlider.PositionScale_X = 1f;
        fadeSlider.SizeOffset_X = 230f;
        fadeSlider.SizeOffset_Y = 20f;
        fadeSlider.Value = LevelLighting.fade;
        fadeSlider.Orientation = ESleekOrientation.HORIZONTAL;
        fadeSlider.AddLabel(local.format("FadeSliderLabelText"), ESleekSide.LEFT);
        fadeSlider.OnValueChanged += onDraggedFadeSlider;
        container.AddChild(fadeSlider);
        lightingScrollBox = Glazier.Get().CreateScrollView();
        lightingScrollBox.PositionOffset_X = -470f;
        lightingScrollBox.PositionOffset_Y = 170f;
        lightingScrollBox.PositionScale_X = 1f;
        lightingScrollBox.SizeOffset_X = 470f;
        lightingScrollBox.SizeOffset_Y = -170f;
        lightingScrollBox.SizeScale_Y = 1f;
        lightingScrollBox.ScaleContentToWidth = true;
        container.AddChild(lightingScrollBox);
        seaLevelSlider = new SleekValue();
        seaLevelSlider.PositionOffset_Y = -130f;
        seaLevelSlider.PositionScale_Y = 1f;
        seaLevelSlider.SizeOffset_X = 200f;
        seaLevelSlider.SizeOffset_Y = 30f;
        seaLevelSlider.state = LevelLighting.seaLevel;
        seaLevelSlider.AddLabel(local.format("Sea_Level_Slider_Label"), ESleekSide.RIGHT);
        seaLevelSlider.onValued = onValuedSeaLevelSlider;
        container.AddChild(seaLevelSlider);
        snowLevelSlider = new SleekValue();
        snowLevelSlider.PositionOffset_Y = -90f;
        snowLevelSlider.PositionScale_Y = 1f;
        snowLevelSlider.SizeOffset_X = 200f;
        snowLevelSlider.SizeOffset_Y = 30f;
        snowLevelSlider.state = LevelLighting.snowLevel;
        snowLevelSlider.AddLabel(local.format("Snow_Level_Slider_Label"), ESleekSide.RIGHT);
        snowLevelSlider.onValued = onValuedSnowLevelSlider;
        container.AddChild(snowLevelSlider);
        rainFreqField = Glazier.Get().CreateFloat32Field();
        rainFreqField.PositionOffset_Y = -370f;
        rainFreqField.PositionScale_Y = 1f;
        rainFreqField.SizeOffset_X = 100f;
        rainFreqField.SizeOffset_Y = 30f;
        rainFreqField.Value = LevelLighting.rainFreq;
        rainFreqField.AddLabel(local.format("Rain_Freq_Label"), ESleekSide.RIGHT);
        rainFreqField.OnValueChanged += onTypedRainFreqField;
        container.AddChild(rainFreqField);
        rainDurField = Glazier.Get().CreateFloat32Field();
        rainDurField.PositionOffset_Y = -330f;
        rainDurField.PositionScale_Y = 1f;
        rainDurField.SizeOffset_X = 100f;
        rainDurField.SizeOffset_Y = 30f;
        rainDurField.Value = LevelLighting.rainDur;
        rainDurField.AddLabel(local.format("Rain_Dur_Label"), ESleekSide.RIGHT);
        rainDurField.OnValueChanged += onTypedRainDurField;
        container.AddChild(rainDurField);
        snowFreqField = Glazier.Get().CreateFloat32Field();
        snowFreqField.PositionOffset_Y = -290f;
        snowFreqField.PositionScale_Y = 1f;
        snowFreqField.SizeOffset_X = 100f;
        snowFreqField.SizeOffset_Y = 30f;
        snowFreqField.Value = LevelLighting.snowFreq;
        snowFreqField.AddLabel(local.format("Snow_Freq_Label"), ESleekSide.RIGHT);
        snowFreqField.OnValueChanged += onTypedSnowFreqField;
        container.AddChild(snowFreqField);
        snowDurField = Glazier.Get().CreateFloat32Field();
        snowDurField.PositionOffset_Y = -250f;
        snowDurField.PositionScale_Y = 1f;
        snowDurField.SizeOffset_X = 100f;
        snowDurField.SizeOffset_Y = 30f;
        snowDurField.Value = LevelLighting.snowDur;
        snowDurField.AddLabel(local.format("Snow_Dur_Label"), ESleekSide.RIGHT);
        snowDurField.OnValueChanged += onTypedSnowDurField;
        container.AddChild(snowDurField);
        weatherGuidField = Glazier.Get().CreateStringField();
        weatherGuidField.PositionOffset_Y = -210f;
        weatherGuidField.PositionScale_Y = 1f;
        weatherGuidField.SizeOffset_X = 200f;
        weatherGuidField.SizeOffset_Y = 30f;
        weatherGuidField.MaxLength = 32;
        container.AddChild(weatherGuidField);
        previewWeatherButton = Glazier.Get().CreateButton();
        previewWeatherButton.PositionOffset_X = 210f;
        previewWeatherButton.PositionOffset_Y = -210f;
        previewWeatherButton.PositionScale_Y = 1f;
        previewWeatherButton.SizeOffset_X = 200f;
        previewWeatherButton.SizeOffset_Y = 30f;
        previewWeatherButton.Text = local.format("WeatherPreview_Label");
        previewWeatherButton.OnClicked += OnClickedPreviewWeather;
        container.AddChild(previewWeatherButton);
        rainToggle = Glazier.Get().CreateToggle();
        rainToggle.PositionOffset_Y = -175f;
        rainToggle.PositionScale_Y = 1f;
        rainToggle.SizeOffset_X = 40f;
        rainToggle.SizeOffset_Y = 40f;
        rainToggle.Value = LevelLighting.canRain;
        rainToggle.AddLabel(local.format("Rain_Toggle_Label"), ESleekSide.RIGHT);
        rainToggle.OnValueChanged += onToggledRainToggle;
        container.AddChild(rainToggle);
        snowToggle = Glazier.Get().CreateToggle();
        snowToggle.PositionOffset_X = 110f;
        snowToggle.PositionOffset_Y = -175f;
        snowToggle.PositionScale_Y = 1f;
        snowToggle.SizeOffset_X = 40f;
        snowToggle.SizeOffset_Y = 40f;
        snowToggle.Value = LevelLighting.canSnow;
        snowToggle.AddLabel(local.format("Snow_Toggle_Label"), ESleekSide.RIGHT);
        snowToggle.OnValueChanged += onToggledSnowToggle;
        container.AddChild(snowToggle);
        moonSlider = Glazier.Get().CreateSlider();
        moonSlider.PositionOffset_Y = -50f;
        moonSlider.PositionScale_Y = 1f;
        moonSlider.SizeOffset_X = 200f;
        moonSlider.SizeOffset_Y = 20f;
        moonSlider.Value = (float)(int)LevelLighting.moon / (float)(int)LevelLighting.MOON_CYCLES;
        moonSlider.Orientation = ESleekOrientation.HORIZONTAL;
        moonSlider.AddLabel(local.format("MoonSliderLabelText"), ESleekSide.RIGHT);
        moonSlider.OnValueChanged += onDraggedMoonSlider;
        container.AddChild(moonSlider);
        timeSlider = Glazier.Get().CreateSlider();
        timeSlider.PositionOffset_Y = -20f;
        timeSlider.PositionScale_Y = 1f;
        timeSlider.SizeOffset_X = 200f;
        timeSlider.SizeOffset_Y = 20f;
        timeSlider.Value = LevelLighting.time;
        timeSlider.Orientation = ESleekOrientation.HORIZONTAL;
        timeSlider.AddLabel(local.format("TimeSliderLabelText"), ESleekSide.RIGHT);
        timeSlider.OnValueChanged += onDraggedTimeSlider;
        container.AddChild(timeSlider);
        timeButtons = new ISleekButton[4];
        for (int i = 0; i < timeButtons.Length; i++)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.PositionOffset_X = 240f;
            sleekButton.PositionOffset_Y = i * 40;
            sleekButton.SizeOffset_X = 200f;
            sleekButton.SizeOffset_Y = 30f;
            sleekButton.Text = local.format("Time_" + i);
            sleekButton.OnClicked += onClickedTimeButton;
            lightingScrollBox.AddChild(sleekButton);
            timeButtons[i] = sleekButton;
        }
        infoBoxes = new ISleekBox[12];
        colorPickers = new SleekColorPicker[infoBoxes.Length];
        singleSliders = new ISleekSlider[5];
        for (int j = 0; j < colorPickers.Length; j++)
        {
            ISleekBox sleekBox = Glazier.Get().CreateBox();
            sleekBox.PositionOffset_X = 240f;
            sleekBox.PositionOffset_Y = timeButtons.Length * 40 + j * 170;
            sleekBox.SizeOffset_X = 200f;
            sleekBox.SizeOffset_Y = 30f;
            sleekBox.Text = local.format("Color_" + j);
            lightingScrollBox.AddChild(sleekBox);
            infoBoxes[j] = sleekBox;
            SleekColorPicker sleekColorPicker = new SleekColorPicker
            {
                PositionOffset_X = 200f,
                PositionOffset_Y = timeButtons.Length * 40 + j * 170 + 40
            };
            sleekColorPicker.onColorPicked = (ColorPicked)Delegate.Combine(sleekColorPicker.onColorPicked, new ColorPicked(onPickedColorPicker));
            lightingScrollBox.AddChild(sleekColorPicker);
            colorPickers[j] = sleekColorPicker;
        }
        for (int k = 0; k < singleSliders.Length; k++)
        {
            ISleekSlider sleekSlider = Glazier.Get().CreateSlider();
            sleekSlider.PositionOffset_X = 240f;
            sleekSlider.PositionOffset_Y = timeButtons.Length * 40 + colorPickers.Length * 170 + k * 30;
            sleekSlider.SizeOffset_X = 200f;
            sleekSlider.SizeOffset_Y = 20f;
            sleekSlider.Orientation = ESleekOrientation.HORIZONTAL;
            sleekSlider.AddLabel(local.format("Single_" + k), ESleekSide.LEFT);
            sleekSlider.OnValueChanged += onDraggedSingleSlider;
            lightingScrollBox.AddChild(sleekSlider);
            singleSliders[k] = sleekSlider;
        }
        lightingScrollBox.ContentSizeOffset = new Vector2(0f, timeButtons.Length * 40 + colorPickers.Length * 170 + singleSliders.Length * 30 - 10);
        updateSelection();
    }
}
