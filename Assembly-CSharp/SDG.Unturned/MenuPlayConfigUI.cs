using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SDG.Unturned;

public class MenuPlayConfigUI
{
    public static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static ISleekButton defaultButton;

    private static ISleekScrollView configBox;

    private static ConfigData configData;

    private static ModeConfigData modeConfigData;

    private static int configOffset;

    private static List<object> configGroups;

    public static void open()
    {
        if (active)
        {
            return;
        }
        active = true;
        configData = ConfigData.CreateDefault(singleplayer: true);
        string path = "/Worlds/Singleplayer_" + Characters.selected + "/Config.json";
        if (ReadWrite.fileExists(path, useCloud: false))
        {
            try
            {
                ReadWrite.populateJSON(path, configData);
            }
            catch (Exception e)
            {
                UnturnedLog.error("Exception while parsing singleplayer config for menu:");
                UnturnedLog.exception(e);
            }
        }
        switch (PlaySettings.singleplayerMode)
        {
        case EGameMode.EASY:
            modeConfigData = configData.Easy;
            break;
        case EGameMode.NORMAL:
            modeConfigData = configData.Normal;
            break;
        case EGameMode.HARD:
            modeConfigData = configData.Hard;
            break;
        }
        refreshConfig();
        container.AnimateIntoView();
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            ReadWrite.serializeJSON("/Worlds/Singleplayer_" + Characters.selected + "/Config.json", useCloud: false, configData);
            container.AnimateOutOfView(0f, 1f);
        }
    }

    public static string sanitizeName(string fieldName)
    {
        if (localization.has(fieldName))
        {
            return localization.format(fieldName);
        }
        return fieldName.Replace('_', ' ');
    }

    private static void refreshConfig()
    {
        configBox.RemoveAllChildren();
        configOffset = 0;
        configGroups.Clear();
        FieldInfo[] fields = modeConfigData.GetType().GetFields();
        foreach (FieldInfo fieldInfo in fields)
        {
            ISleekBox sleekBox = Glazier.Get().CreateBox();
            sleekBox.PositionOffset_Y = configOffset;
            sleekBox.SizeOffset_Y = 30f;
            sleekBox.SizeScale_X = 1f;
            sleekBox.Text = sanitizeName(fieldInfo.Name);
            configBox.AddChild(sleekBox);
            int num = 40;
            configOffset += 40;
            object value = fieldInfo.GetValue(modeConfigData);
            FieldInfo[] fields2 = value.GetType().GetFields();
            foreach (FieldInfo fieldInfo2 in fields2)
            {
                object value2 = fieldInfo2.GetValue(value);
                Type type = value2.GetType();
                if (type == typeof(uint))
                {
                    ISleekUInt32Field sleekUInt32Field = Glazier.Get().CreateUInt32Field();
                    sleekUInt32Field.PositionOffset_Y = num;
                    sleekUInt32Field.SizeOffset_X = 200f;
                    sleekUInt32Field.SizeOffset_Y = 30f;
                    sleekUInt32Field.Value = (uint)value2;
                    sleekUInt32Field.AddLabel(sanitizeName(fieldInfo2.Name), ESleekSide.RIGHT);
                    sleekUInt32Field.OnValueChanged += onTypedUInt32;
                    sleekBox.AddChild(sleekUInt32Field);
                    num += 40;
                    configOffset += 40;
                }
                else if (type == typeof(float))
                {
                    ISleekFloat32Field sleekFloat32Field = Glazier.Get().CreateFloat32Field();
                    sleekFloat32Field.PositionOffset_Y = num;
                    sleekFloat32Field.SizeOffset_X = 200f;
                    sleekFloat32Field.SizeOffset_Y = 30f;
                    sleekFloat32Field.Value = (float)value2;
                    sleekFloat32Field.AddLabel(sanitizeName(fieldInfo2.Name), ESleekSide.RIGHT);
                    sleekFloat32Field.OnValueChanged += onTypedSingle;
                    sleekBox.AddChild(sleekFloat32Field);
                    num += 40;
                    configOffset += 40;
                }
                else if (type == typeof(bool))
                {
                    ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
                    sleekToggle.PositionOffset_Y = num;
                    sleekToggle.SizeOffset_X = 40f;
                    sleekToggle.SizeOffset_Y = 40f;
                    sleekToggle.Value = (bool)value2;
                    sleekToggle.AddLabel(sanitizeName(fieldInfo2.Name), ESleekSide.RIGHT);
                    sleekToggle.OnValueChanged += onToggled;
                    sleekBox.AddChild(sleekToggle);
                    num += 50;
                    configOffset += 50;
                }
            }
            configOffset += 40;
            configGroups.Add(value);
        }
        configBox.ContentSizeOffset = new Vector2(0f, configOffset - 50);
    }

    private static void updateValue(ISleekElement sleek, object state)
    {
        int index = configBox.FindIndexOfChild(sleek.Parent);
        object obj = configGroups[index];
        FieldInfo[] fields = obj.GetType().GetFields();
        int num = sleek.Parent.FindIndexOfChild(sleek);
        fields[num].SetValue(obj, state);
    }

    private static void onTypedUInt32(ISleekUInt32Field uint32Field, uint state)
    {
        updateValue(uint32Field, state);
    }

    private static void onTypedSingle(ISleekFloat32Field singleField, float state)
    {
        updateValue(singleField, state);
    }

    private static void onToggled(ISleekToggle toggle, bool state)
    {
        updateValue(toggle, state);
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuPlaySingleplayerUI.open();
        close();
    }

    private static void onClickedDefaultButton(ISleekElement button)
    {
        modeConfigData = new ModeConfigData(PlaySettings.singleplayerMode);
        modeConfigData.InitSingleplayerDefaults();
        switch (PlaySettings.singleplayerMode)
        {
        case EGameMode.EASY:
            configData.Easy = modeConfigData;
            break;
        case EGameMode.NORMAL:
            configData.Normal = modeConfigData;
            break;
        case EGameMode.HARD:
            configData.Hard = modeConfigData;
            break;
        }
        refreshConfig();
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
        configBox.PositionOffset_X = -200f;
        configBox.PositionOffset_Y = 100f;
        configBox.PositionScale_X = 0.5f;
        configBox.SizeOffset_X = 430f;
        configBox.SizeOffset_Y = -200f;
        configBox.SizeScale_Y = 1f;
        configBox.ScaleContentToWidth = true;
        container.AddChild(configBox);
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
        configGroups = new List<object>();
    }
}
