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
            sleekBox.positionOffset_Y = configOffset;
            sleekBox.sizeOffset_Y = 30;
            sleekBox.sizeScale_X = 1f;
            sleekBox.text = sanitizeName(fieldInfo.Name);
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
                    sleekUInt32Field.positionOffset_Y = num;
                    sleekUInt32Field.sizeOffset_X = 200;
                    sleekUInt32Field.sizeOffset_Y = 30;
                    sleekUInt32Field.state = (uint)value2;
                    sleekUInt32Field.addLabel(sanitizeName(fieldInfo2.Name), ESleekSide.RIGHT);
                    sleekUInt32Field.onTypedUInt32 += onTypedUInt32;
                    sleekBox.AddChild(sleekUInt32Field);
                    num += 40;
                    configOffset += 40;
                }
                else if (type == typeof(float))
                {
                    ISleekFloat32Field sleekFloat32Field = Glazier.Get().CreateFloat32Field();
                    sleekFloat32Field.positionOffset_Y = num;
                    sleekFloat32Field.sizeOffset_X = 200;
                    sleekFloat32Field.sizeOffset_Y = 30;
                    sleekFloat32Field.state = (float)value2;
                    sleekFloat32Field.addLabel(sanitizeName(fieldInfo2.Name), ESleekSide.RIGHT);
                    sleekFloat32Field.onTypedSingle += onTypedSingle;
                    sleekBox.AddChild(sleekFloat32Field);
                    num += 40;
                    configOffset += 40;
                }
                else if (type == typeof(bool))
                {
                    ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
                    sleekToggle.positionOffset_Y = num;
                    sleekToggle.sizeOffset_X = 40;
                    sleekToggle.sizeOffset_Y = 40;
                    sleekToggle.state = (bool)value2;
                    sleekToggle.addLabel(sanitizeName(fieldInfo2.Name), ESleekSide.RIGHT);
                    sleekToggle.onToggled += onToggled;
                    sleekBox.AddChild(sleekToggle);
                    num += 50;
                    configOffset += 50;
                }
            }
            configOffset += 40;
            configGroups.Add(value);
        }
        configBox.contentSizeOffset = new Vector2(0f, configOffset - 50);
    }

    private static void updateValue(ISleekElement sleek, object state)
    {
        int index = configBox.FindIndexOfChild(sleek.parent);
        object obj = configGroups[index];
        FieldInfo[] fields = obj.GetType().GetFields();
        int num = sleek.parent.FindIndexOfChild(sleek);
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
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_Y = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        configBox = Glazier.Get().CreateScrollView();
        configBox.positionOffset_X = -200;
        configBox.positionOffset_Y = 100;
        configBox.positionScale_X = 0.5f;
        configBox.sizeOffset_X = 430;
        configBox.sizeOffset_Y = -200;
        configBox.sizeScale_Y = 1f;
        configBox.scaleContentToWidth = true;
        container.AddChild(configBox);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.positionOffset_Y = -50;
        backButton.positionScale_Y = 1f;
        backButton.sizeOffset_X = 200;
        backButton.sizeOffset_Y = 50;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
        defaultButton = Glazier.Get().CreateButton();
        defaultButton.positionOffset_X = -200;
        defaultButton.positionOffset_Y = -50;
        defaultButton.positionScale_X = 1f;
        defaultButton.positionScale_Y = 1f;
        defaultButton.sizeOffset_X = 200;
        defaultButton.sizeOffset_Y = 50;
        defaultButton.text = localization.format("Default");
        defaultButton.tooltipText = localization.format("Default_Tooltip");
        defaultButton.onClickedButton += onClickedDefaultButton;
        defaultButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(defaultButton);
        configGroups = new List<object>();
    }
}
