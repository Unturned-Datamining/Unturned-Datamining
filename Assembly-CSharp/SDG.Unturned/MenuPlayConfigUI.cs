using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class MenuPlayConfigUI
{
    public static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static ISleekButton defaultButton;

    private static ISleekScrollView configBox;

    private static ModeConfigData defaultModeConfigData;

    private static Dictionary<FieldInfo, SleekConfigProperty> propertyWidgets;

    private static Dictionary<FieldInfo, object> propertyOverrides;

    public static void open()
    {
        if (active)
        {
            return;
        }
        active = true;
        if (propertyWidgets == null)
        {
            CreatePropertyWidgets();
        }
        defaultModeConfigData = ModeConfigData.CreateDefault(PlaySettings.singleplayerMode, singleplayer: true);
        propertyOverrides.Clear();
        string singleplayerConfigPathV = PlayConfigUtils.GetSingleplayerConfigPathV2(Characters.selected, PlaySettings.singleplayerMode);
        if (File.Exists(singleplayerConfigPathV))
        {
            IDatDictionary rootDictionary = null;
            try
            {
                using FileStream stream = new FileStream(singleplayerConfigPathV, FileMode.Open, FileAccess.Read, FileShare.Read);
                using StreamReader inputReader = new StreamReader(stream);
                DatParser datParser = new DatParser();
                datParser.EnableMetadata = true;
                rootDictionary = datParser.Parse(inputReader);
                if (datParser.HasError)
                {
                    CommandWindow.LogWarning("Error(s) parsing gameplay config:");
                    foreach (string errorMessage in datParser.ErrorMessages)
                    {
                        CommandWindow.LogWarning(errorMessage);
                    }
                }
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, "Caught exception parsing v2 gameplay config for menu:");
            }
            try
            {
                ModeConfigData config = ModeConfigData.CreateDefault(PlaySettings.singleplayerMode, singleplayer: true);
                PlayConfigUtils.ParseModeConfig(rootDictionary, config, propertyOverrides);
            }
            catch (Exception e2)
            {
                UnturnedLog.exception(e2, "Caught exception parsing mode config for menu:");
            }
        }
        else
        {
            ConfigData configData = ConfigData.CreateDefault(singleplayer: true);
            string path = "/Worlds/Singleplayer_" + Characters.selected + "/Config.json";
            if (ReadWrite.fileExists(path, useCloud: false))
            {
                try
                {
                    ReadWrite.populateJSON(path, configData);
                }
                catch (Exception e3)
                {
                    UnturnedLog.error("Exception while parsing singleplayer config json for menu:");
                    UnturnedLog.exception(e3);
                }
            }
            try
            {
                PlayConfigUtils.GatherModifiedFields(defaultModeConfigData, configData.getModeConfig(PlaySettings.singleplayerMode), propertyOverrides);
                foreach (KeyValuePair<FieldInfo, object> propertyOverride in propertyOverrides)
                {
                    CommandWindow.Log($"Config menu converted {PlayConfigUtils.GetFieldPath(propertyOverride.Key)} = \"{propertyOverride.Value}\"");
                }
            }
            catch (Exception e4)
            {
                UnturnedLog.exception(e4, "Caught exception gathering modified json fields for menu:");
            }
        }
        SyncPropertyWidgetValues();
        container.AnimateIntoView();
    }

    public static void close()
    {
        if (!active)
        {
            return;
        }
        active = false;
        IEditableDatDictionary rootDictionary = MetadataPreservingDatWriter.CreateRoot();
        try
        {
            PlayConfigUtils.ApplyModeConfigOverrides(rootDictionary, propertyOverrides);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception applying modified fields for config menu:");
        }
        string singleplayerConfigPathV = PlayConfigUtils.GetSingleplayerConfigPathV2(Characters.selected, PlaySettings.singleplayerMode);
        try
        {
            using StreamWriter output = new StreamWriter(singleplayerConfigPathV, append: false, Encoding.UTF8);
            DatWriter writer = new DatWriter(output);
            new MetadataPreservingDatWriter().WriteRootDictionary(rootDictionary, writer);
        }
        catch (Exception e2)
        {
            UnturnedLog.exception(e2, "Caught exception writing updated config file to: \"" + singleplayerConfigPathV + "\"");
        }
        container.AnimateOutOfView(0f, 1f);
    }

    public static string sanitizeName(string fieldName)
    {
        if (localization.has(fieldName))
        {
            return localization.format(fieldName);
        }
        return fieldName.Replace('_', ' ');
    }

    /// <summary>
    /// Creating all these elements is a bit slow, so we only do it once the menu is first opened.
    /// </summary>
    private static void CreatePropertyWidgets()
    {
        UnturnedCodeDocsHelper unturnedCodeDocsHelper = new UnturnedCodeDocsHelper();
        propertyWidgets = new Dictionary<FieldInfo, SleekConfigProperty>();
        StringBuilder stringBuilder = new StringBuilder();
        float num = 0f;
        FieldInfo[] fields = typeof(ModeConfigData).GetFields();
        foreach (FieldInfo fieldInfo in fields)
        {
            ISleekBox sleekBox = Glazier.Get().CreateBox();
            sleekBox.PositionOffset_X = 100f;
            sleekBox.PositionOffset_Y = num;
            sleekBox.SizeOffset_Y = 30f;
            sleekBox.SizeOffset_X = -100f;
            sleekBox.SizeScale_X = 1f;
            sleekBox.Text = sanitizeName(fieldInfo.Name);
            configBox.AddChild(sleekBox);
            float num2 = 40f;
            num += 40f;
            FieldInfo[] fields2 = fieldInfo.FieldType.GetFields();
            foreach (FieldInfo fieldInfo2 in fields2)
            {
                string text = unturnedCodeDocsHelper.GetSummary(fieldInfo.FieldType.Name, fieldInfo2.Name);
                if (!string.IsNullOrEmpty(text))
                {
                    string[] array = text.SplitLinesIncludingEmpty();
                    int num3 = array.Length;
                    if (string.IsNullOrWhiteSpace(array[num3 - 1]))
                    {
                        num3--;
                    }
                    if (num3 == 1)
                    {
                        text = array[0].Trim();
                    }
                    else
                    {
                        stringBuilder.Clear();
                        for (int k = 0; k < num3; k++)
                        {
                            if (k > 0)
                            {
                                stringBuilder.AppendLine();
                            }
                            stringBuilder.Append(array[k].Trim());
                        }
                        text = stringBuilder.ToString();
                    }
                }
                SleekConfigProperty sleekConfigProperty = new SleekConfigProperty(fieldInfo2, text);
                sleekConfigProperty.SizeScale_X = 1f;
                sleekConfigProperty.PositionOffset_Y = num2;
                sleekConfigProperty.OnValueChanged += OnPropertyOverrideChanged;
                sleekBox.AddChild(sleekConfigProperty);
                propertyWidgets.Add(fieldInfo2, sleekConfigProperty);
                num2 += sleekConfigProperty.SizeOffset_Y + 10f;
                num += sleekConfigProperty.SizeOffset_Y + 10f;
            }
            num += 40f;
        }
        configBox.ContentSizeOffset = new Vector2(0f, num - 50f);
    }

    private static void SyncPropertyWidgetValues()
    {
        FieldInfo[] fields = typeof(ModeConfigData).GetFields();
        foreach (FieldInfo obj in fields)
        {
            object value = obj.GetValue(defaultModeConfigData);
            FieldInfo[] fields2 = obj.FieldType.GetFields();
            foreach (FieldInfo fieldInfo in fields2)
            {
                fieldInfo.GetValue(value);
                object value2;
                bool isOverridden = propertyOverrides.TryGetValue(fieldInfo, out value2);
                SleekConfigProperty sleekConfigProperty = propertyWidgets[fieldInfo];
                sleekConfigProperty.defaultValue = fieldInfo.GetValue(value);
                sleekConfigProperty.SetOverrideState(isOverridden, value2);
            }
        }
    }

    private static void OnPropertyOverrideChanged(SleekConfigProperty widget, bool hasOverride, object overrideValue)
    {
        if (hasOverride)
        {
            propertyOverrides[widget.fieldInfo] = overrideValue;
            UnturnedLog.info($"Set {widget.fieldInfo.Name} override {overrideValue}");
        }
        else
        {
            propertyOverrides.Remove(widget.fieldInfo);
            UnturnedLog.info("Remove " + widget.fieldInfo.Name + " override");
        }
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuPlaySingleplayerUI.open();
        close();
    }

    private static void onClickedDefaultButton(ISleekElement button)
    {
        propertyOverrides.Clear();
        SyncPropertyWidgetValues();
    }

    public MenuPlayConfigUI()
    {
        localization = Localization.read("/Menu/Play/MenuPlayConfig.dat");
        container = new SleekFullscreenBox();
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        configBox = Glazier.Get().CreateScrollView();
        configBox.PositionOffset_X = -300f;
        configBox.PositionOffset_Y = 100f;
        configBox.PositionScale_X = 0.5f;
        configBox.SizeOffset_X = 530f;
        configBox.SizeOffset_Y = -200f;
        configBox.SizeScale_Y = 1f;
        configBox.ScaleContentToWidth = true;
        container.AddChild(configBox);
        propertyWidgets = null;
        propertyOverrides = new Dictionary<FieldInfo, object>();
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.PositionOffset_Y = -50f;
        backButton.PositionScale_Y = 1f;
        backButton.SizeOffset_X = 200f;
        backButton.SizeOffset_Y = 50f;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
        defaultButton = Glazier.Get().CreateButton();
        defaultButton.PositionOffset_X = -200f;
        defaultButton.PositionOffset_Y = -50f;
        defaultButton.PositionScale_X = 1f;
        defaultButton.PositionScale_Y = 1f;
        defaultButton.SizeOffset_X = 200f;
        defaultButton.SizeOffset_Y = 50f;
        defaultButton.Text = localization.format("Default");
        defaultButton.TooltipText = localization.format("Default_Tooltip");
        defaultButton.OnClicked += onClickedDefaultButton;
        defaultButton.FontSize = ESleekFontSize.Medium;
        container.AddChild(defaultButton);
    }
}
