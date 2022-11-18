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
        timeSlider.state = LevelLighting.time;
    }

    private static void OnClickedPreviewWeather(ISleekElement button)
    {
        Guid result;
        WeatherAssetBase weatherAssetBase = ((!Guid.TryParse(weatherGuidField.text, out result)) ? null : Assets.find(new AssetReference<WeatherAssetBase>(result)));
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
            singleSliders[j].state = LevelLighting.times[(int)selectedTime].singles[j];
        }
    }

    public EditorEnvironmentLightingUI()
    {
        Local local = Localization.read("/Editor/EditorEnvironmentLighting.dat");
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_X = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        selectedTime = ELightingTime.DAWN;
        azimuthSlider = Glazier.Get().CreateSlider();
        azimuthSlider.positionOffset_X = -230;
        azimuthSlider.positionOffset_Y = 80;
        azimuthSlider.positionScale_X = 1f;
        azimuthSlider.sizeOffset_X = 230;
        azimuthSlider.sizeOffset_Y = 20;
        azimuthSlider.state = LevelLighting.azimuth / 360f;
        azimuthSlider.orientation = ESleekOrientation.HORIZONTAL;
        azimuthSlider.addLabel(local.format("AzimuthSliderLabelText"), ESleekSide.LEFT);
        azimuthSlider.onDragged += onDraggedAzimuthSlider;
        container.AddChild(azimuthSlider);
        biasSlider = Glazier.Get().CreateSlider();
        biasSlider.positionOffset_X = -230;
        biasSlider.positionOffset_Y = 110;
        biasSlider.positionScale_X = 1f;
        biasSlider.sizeOffset_X = 230;
        biasSlider.sizeOffset_Y = 20;
        biasSlider.state = LevelLighting.bias;
        biasSlider.orientation = ESleekOrientation.HORIZONTAL;
        biasSlider.addLabel(local.format("BiasSliderLabelText"), ESleekSide.LEFT);
        biasSlider.onDragged += onDraggedBiasSlider;
        container.AddChild(biasSlider);
        fadeSlider = Glazier.Get().CreateSlider();
        fadeSlider.positionOffset_X = -230;
        fadeSlider.positionOffset_Y = 140;
        fadeSlider.positionScale_X = 1f;
        fadeSlider.sizeOffset_X = 230;
        fadeSlider.sizeOffset_Y = 20;
        fadeSlider.state = LevelLighting.fade;
        fadeSlider.orientation = ESleekOrientation.HORIZONTAL;
        fadeSlider.addLabel(local.format("FadeSliderLabelText"), ESleekSide.LEFT);
        fadeSlider.onDragged += onDraggedFadeSlider;
        container.AddChild(fadeSlider);
        lightingScrollBox = Glazier.Get().CreateScrollView();
        lightingScrollBox.positionOffset_X = -470;
        lightingScrollBox.positionOffset_Y = 170;
        lightingScrollBox.positionScale_X = 1f;
        lightingScrollBox.sizeOffset_X = 470;
        lightingScrollBox.sizeOffset_Y = -170;
        lightingScrollBox.sizeScale_Y = 1f;
        lightingScrollBox.scaleContentToWidth = true;
        container.AddChild(lightingScrollBox);
        seaLevelSlider = new SleekValue();
        seaLevelSlider.positionOffset_Y = -130;
        seaLevelSlider.positionScale_Y = 1f;
        seaLevelSlider.sizeOffset_X = 200;
        seaLevelSlider.sizeOffset_Y = 30;
        seaLevelSlider.state = LevelLighting.seaLevel;
        seaLevelSlider.addLabel(local.format("Sea_Level_Slider_Label"), ESleekSide.RIGHT);
        seaLevelSlider.onValued = onValuedSeaLevelSlider;
        container.AddChild(seaLevelSlider);
        snowLevelSlider = new SleekValue();
        snowLevelSlider.positionOffset_Y = -90;
        snowLevelSlider.positionScale_Y = 1f;
        snowLevelSlider.sizeOffset_X = 200;
        snowLevelSlider.sizeOffset_Y = 30;
        snowLevelSlider.state = LevelLighting.snowLevel;
        snowLevelSlider.addLabel(local.format("Snow_Level_Slider_Label"), ESleekSide.RIGHT);
        snowLevelSlider.onValued = onValuedSnowLevelSlider;
        container.AddChild(snowLevelSlider);
        rainFreqField = Glazier.Get().CreateFloat32Field();
        rainFreqField.positionOffset_Y = -370;
        rainFreqField.positionScale_Y = 1f;
        rainFreqField.sizeOffset_X = 100;
        rainFreqField.sizeOffset_Y = 30;
        rainFreqField.state = LevelLighting.rainFreq;
        rainFreqField.addLabel(local.format("Rain_Freq_Label"), ESleekSide.RIGHT);
        rainFreqField.onTypedSingle += onTypedRainFreqField;
        container.AddChild(rainFreqField);
        rainDurField = Glazier.Get().CreateFloat32Field();
        rainDurField.positionOffset_Y = -330;
        rainDurField.positionScale_Y = 1f;
        rainDurField.sizeOffset_X = 100;
        rainDurField.sizeOffset_Y = 30;
        rainDurField.state = LevelLighting.rainDur;
        rainDurField.addLabel(local.format("Rain_Dur_Label"), ESleekSide.RIGHT);
        rainDurField.onTypedSingle += onTypedRainDurField;
        container.AddChild(rainDurField);
        snowFreqField = Glazier.Get().CreateFloat32Field();
        snowFreqField.positionOffset_Y = -290;
        snowFreqField.positionScale_Y = 1f;
        snowFreqField.sizeOffset_X = 100;
        snowFreqField.sizeOffset_Y = 30;
        snowFreqField.state = LevelLighting.snowFreq;
        snowFreqField.addLabel(local.format("Snow_Freq_Label"), ESleekSide.RIGHT);
        snowFreqField.onTypedSingle += onTypedSnowFreqField;
        container.AddChild(snowFreqField);
        snowDurField = Glazier.Get().CreateFloat32Field();
        snowDurField.positionOffset_Y = -250;
        snowDurField.positionScale_Y = 1f;
        snowDurField.sizeOffset_X = 100;
        snowDurField.sizeOffset_Y = 30;
        snowDurField.state = LevelLighting.snowDur;
        snowDurField.addLabel(local.format("Snow_Dur_Label"), ESleekSide.RIGHT);
        snowDurField.onTypedSingle += onTypedSnowDurField;
        container.AddChild(snowDurField);
        weatherGuidField = Glazier.Get().CreateStringField();
        weatherGuidField.positionOffset_Y = -210;
        weatherGuidField.positionScale_Y = 1f;
        weatherGuidField.sizeOffset_X = 200;
        weatherGuidField.sizeOffset_Y = 30;
        weatherGuidField.maxLength = 32;
        container.AddChild(weatherGuidField);
        previewWeatherButton = Glazier.Get().CreateButton();
        previewWeatherButton.positionOffset_X = 210;
        previewWeatherButton.positionOffset_Y = -210;
        previewWeatherButton.positionScale_Y = 1f;
        previewWeatherButton.sizeOffset_X = 200;
        previewWeatherButton.sizeOffset_Y = 30;
        previewWeatherButton.text = local.format("WeatherPreview_Label");
        previewWeatherButton.onClickedButton += OnClickedPreviewWeather;
        container.AddChild(previewWeatherButton);
        rainToggle = Glazier.Get().CreateToggle();
        rainToggle.positionOffset_Y = -175;
        rainToggle.positionScale_Y = 1f;
        rainToggle.sizeOffset_X = 40;
        rainToggle.sizeOffset_Y = 40;
        rainToggle.state = LevelLighting.canRain;
        rainToggle.addLabel(local.format("Rain_Toggle_Label"), ESleekSide.RIGHT);
        rainToggle.onToggled += onToggledRainToggle;
        container.AddChild(rainToggle);
        snowToggle = Glazier.Get().CreateToggle();
        snowToggle.positionOffset_X = 110;
        snowToggle.positionOffset_Y = -175;
        snowToggle.positionScale_Y = 1f;
        snowToggle.sizeOffset_X = 40;
        snowToggle.sizeOffset_Y = 40;
        snowToggle.state = LevelLighting.canSnow;
        snowToggle.addLabel(local.format("Snow_Toggle_Label"), ESleekSide.RIGHT);
        snowToggle.onToggled += onToggledSnowToggle;
        container.AddChild(snowToggle);
        moonSlider = Glazier.Get().CreateSlider();
        moonSlider.positionOffset_Y = -50;
        moonSlider.positionScale_Y = 1f;
        moonSlider.sizeOffset_X = 200;
        moonSlider.sizeOffset_Y = 20;
        moonSlider.state = (float)(int)LevelLighting.moon / (float)(int)LevelLighting.MOON_CYCLES;
        moonSlider.orientation = ESleekOrientation.HORIZONTAL;
        moonSlider.addLabel(local.format("MoonSliderLabelText"), ESleekSide.RIGHT);
        moonSlider.onDragged += onDraggedMoonSlider;
        container.AddChild(moonSlider);
        timeSlider = Glazier.Get().CreateSlider();
        timeSlider.positionOffset_Y = -20;
        timeSlider.positionScale_Y = 1f;
        timeSlider.sizeOffset_X = 200;
        timeSlider.sizeOffset_Y = 20;
        timeSlider.state = LevelLighting.time;
        timeSlider.orientation = ESleekOrientation.HORIZONTAL;
        timeSlider.addLabel(local.format("TimeSliderLabelText"), ESleekSide.RIGHT);
        timeSlider.onDragged += onDraggedTimeSlider;
        container.AddChild(timeSlider);
        timeButtons = new ISleekButton[4];
        for (int i = 0; i < timeButtons.Length; i++)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.positionOffset_X = 240;
            sleekButton.positionOffset_Y = i * 40;
            sleekButton.sizeOffset_X = 200;
            sleekButton.sizeOffset_Y = 30;
            sleekButton.text = local.format("Time_" + i);
            sleekButton.onClickedButton += onClickedTimeButton;
            lightingScrollBox.AddChild(sleekButton);
            timeButtons[i] = sleekButton;
        }
        infoBoxes = new ISleekBox[12];
        colorPickers = new SleekColorPicker[infoBoxes.Length];
        singleSliders = new ISleekSlider[5];
        for (int j = 0; j < colorPickers.Length; j++)
        {
            ISleekBox sleekBox = Glazier.Get().CreateBox();
            sleekBox.positionOffset_X = 240;
            sleekBox.positionOffset_Y = timeButtons.Length * 40 + j * 170;
            sleekBox.sizeOffset_X = 200;
            sleekBox.sizeOffset_Y = 30;
            sleekBox.text = local.format("Color_" + j);
            lightingScrollBox.AddChild(sleekBox);
            infoBoxes[j] = sleekBox;
            SleekColorPicker sleekColorPicker = new SleekColorPicker
            {
                positionOffset_X = 200,
                positionOffset_Y = timeButtons.Length * 40 + j * 170 + 40
            };
            sleekColorPicker.onColorPicked = (ColorPicked)Delegate.Combine(sleekColorPicker.onColorPicked, new ColorPicked(onPickedColorPicker));
            lightingScrollBox.AddChild(sleekColorPicker);
            colorPickers[j] = sleekColorPicker;
        }
        for (int k = 0; k < singleSliders.Length; k++)
        {
            ISleekSlider sleekSlider = Glazier.Get().CreateSlider();
            sleekSlider.positionOffset_X = 240;
            sleekSlider.positionOffset_Y = timeButtons.Length * 40 + colorPickers.Length * 170 + k * 30;
            sleekSlider.sizeOffset_X = 200;
            sleekSlider.sizeOffset_Y = 20;
            sleekSlider.orientation = ESleekOrientation.HORIZONTAL;
            sleekSlider.addLabel(local.format("Single_" + k), ESleekSide.LEFT);
            sleekSlider.onDragged += onDraggedSingleSlider;
            lightingScrollBox.AddChild(sleekSlider);
            singleSliders[k] = sleekSlider;
        }
        lightingScrollBox.contentSizeOffset = new Vector2(0f, timeButtons.Length * 40 + colorPickers.Length * 170 + singleSliders.Length * 30 - 10);
        updateSelection();
    }
}
