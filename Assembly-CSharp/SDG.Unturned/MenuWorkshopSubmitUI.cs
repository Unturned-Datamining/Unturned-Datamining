using System;
using System.Collections.Generic;
using System.IO;
using SDG.Provider;
using UnityEngine;

namespace SDG.Unturned;

public class MenuWorkshopSubmitUI
{
    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static ISleekField nameField;

    private static ISleekField descriptionField;

    private static ISleekField pathField;

    private static ISleekBox pathNotification;

    private static ISleekField previewField;

    private static ISleekBox previewNotification;

    private static ISleekField changeField;

    private static ISleekField allowedIPsField;

    private static SleekButtonState typeState;

    private static SleekButtonState mapTypeState;

    private static SleekButtonState itemTypeState;

    private static SleekButtonState vehicleTypeState;

    private static SleekButtonState skinTypeState;

    private static SleekButtonState objectTypeState;

    private static SleekButtonState visibilityState;

    private static SleekButtonState forState;

    private static SleekButtonIcon createButton;

    private static ISleekButton legalButton;

    private static ISleekScrollView publishedBox;

    private static List<ISleekButton> publishedButtons;

    private static string tag => typeState.state switch
    {
        0 => mapTypeState.states[mapTypeState.state].text, 
        3 => itemTypeState.states[itemTypeState.state].text, 
        4 => vehicleTypeState.states[vehicleTypeState.state].text, 
        5 => skinTypeState.states[skinTypeState.state].text, 
        2 => objectTypeState.states[objectTypeState.state].text, 
        _ => "", 
    };

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
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private static void refreshPathFieldNotification()
    {
        string text = pathField.text;
        string text2 = null;
        if (string.IsNullOrEmpty(text))
        {
            text2 = localization.format("PathFieldNotification_Empty");
        }
        else if (!ReadWrite.folderExists(text, usePath: false))
        {
            text2 = localization.format("PathFieldNotification_MissingFolder");
        }
        else if (!ReadWrite.hasDirectoryWritePermission(text))
        {
            text2 = localization.format("PathFieldNotification_NoWritePermission");
        }
        else
        {
            switch (typeState.state)
            {
            case 0:
                if (!WorkshopTool.checkMapValid(text, usePath: false))
                {
                    text2 = localization.format("PathFieldNotification_Map");
                }
                break;
            case 1:
                if (!WorkshopTool.checkLocalizationValid(text, usePath: false))
                {
                    text2 = localization.format("PathFieldNotification_Localization");
                }
                break;
            default:
                if (!WorkshopTool.checkBundleValid(text, usePath: false))
                {
                    text2 = localization.format("PathFieldNotification_Bundle");
                }
                break;
            }
        }
        pathNotification.isVisible = !string.IsNullOrEmpty(text2);
        pathNotification.tooltipText = text2;
    }

    private static void onPathFieldTyped(ISleekField field, string text)
    {
        refreshPathFieldNotification();
    }

    private static void refreshPreviewFieldNotification()
    {
        string text = previewField.text;
        string text2 = null;
        if (string.IsNullOrEmpty(text))
        {
            text2 = localization.format("PreviewFieldNotification_Empty");
        }
        else if (!ReadWrite.fileExists(text, useCloud: false, usePath: false))
        {
            text2 = localization.format("PreviewFieldNotification_MissingFile");
        }
        else if (text.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase) || text.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase))
        {
            if (new FileInfo(text).Length > 1000000)
            {
                text2 = localization.format("PreviewFieldNotification_FileSize");
            }
        }
        else
        {
            text2 = localization.format("PreviewFieldNotification_Extension");
        }
        previewNotification.isVisible = !string.IsNullOrEmpty(text2);
        previewNotification.tooltipText = text2;
    }

    private static void onPreviewFieldTyped(ISleekField field, string text)
    {
        refreshPreviewFieldNotification();
    }

    private static void onClickedCreateButton(ISleekElement button)
    {
        if (checkEntered() && checkValid())
        {
            Provider.provider.workshopService.prepareUGC(nameField.text, descriptionField.text, pathField.text, previewField.text, changeField.text, (ESteamUGCType)typeState.state, tag, allowedIPsField.text, (ESteamUGCVisibility)visibilityState.state);
            Provider.provider.workshopService.createUGC(forState.state == 1);
            resetFields();
        }
    }

    private static void onClickedLegalButton(ISleekElement button)
    {
        if (!Provider.provider.browserService.canOpenBrowser)
        {
            MenuUI.alert(localization.format("Overlay"));
        }
        else
        {
            Provider.provider.browserService.open("http://steamcommunity.com/sharedfiles/workshoplegalagreement/?appid=304930");
        }
    }

    private static void onClickedPublished(ISleekElement button)
    {
        int index = button.positionOffset_Y / 40;
        if (checkValid())
        {
            Provider.provider.workshopService.prepareUGC(nameField.text, descriptionField.text, pathField.text, previewField.text, changeField.text, (ESteamUGCType)typeState.state, tag, allowedIPsField.text, (ESteamUGCVisibility)visibilityState.state);
            Provider.provider.workshopService.prepareUGC(Provider.provider.workshopService.published[index].id);
            Provider.provider.workshopService.updateUGC();
            resetFields();
        }
    }

    private static void onPublishedAdded()
    {
        for (int i = 0; i < Provider.provider.workshopService.published.Count; i++)
        {
            SteamPublished steamPublished = Provider.provider.workshopService.published[i];
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.positionOffset_Y = i * 40;
            sleekButton.sizeOffset_Y = 30;
            sleekButton.sizeScale_X = 1f;
            sleekButton.text = steamPublished.name;
            sleekButton.onClickedButton += onClickedPublished;
            publishedBox.AddChild(sleekButton);
            publishedButtons.Add(sleekButton);
            publishedBox.contentSizeOffset = new Vector2(0f, publishedButtons.Count * 40 - 10);
        }
    }

    private static void onPublishedRemoved()
    {
        publishedBox.RemoveAllChildren();
        publishedButtons.Clear();
    }

    private static bool checkEntered()
    {
        if (nameField.text.Length == 0)
        {
            MenuUI.alert(localization.format("Alert_Name"));
            return false;
        }
        if (previewField.text.Length == 0 || !ReadWrite.fileExists(previewField.text, useCloud: false, usePath: false) || new FileInfo(previewField.text).Length > 1000000)
        {
            MenuUI.alert(localization.format("Alert_Preview"));
            return false;
        }
        return true;
    }

    private static bool checkValid()
    {
        if (pathField.text.Length == 0 || !ReadWrite.folderExists(pathField.text, usePath: false))
        {
            MenuUI.alert(localization.format("Alert_Path"));
            return false;
        }
        ESteamUGCType state = (ESteamUGCType)typeState.state;
        if (forState.state == 1)
        {
            if (state != ESteamUGCType.ITEM && state != ESteamUGCType.SKIN)
            {
                MenuUI.alert(localization.format("Alert_Curated"));
                return false;
            }
        }
        else if (state == ESteamUGCType.SKIN)
        {
            MenuUI.alert(localization.format("Alert_Curated"));
            return false;
        }
        bool flag = false;
        switch (state)
        {
        case ESteamUGCType.MAP:
            flag = WorkshopTool.checkMapValid(pathField.text, usePath: false);
            if (!flag)
            {
                MenuUI.alert(localization.format("Alert_Map"));
            }
            break;
        case ESteamUGCType.LOCALIZATION:
            flag = WorkshopTool.checkLocalizationValid(pathField.text, usePath: false);
            if (!flag)
            {
                MenuUI.alert(localization.format("Alert_Localization"));
            }
            break;
        case ESteamUGCType.OBJECT:
        case ESteamUGCType.ITEM:
        case ESteamUGCType.VEHICLE:
        case ESteamUGCType.SKIN:
            flag = WorkshopTool.checkBundleValid(pathField.text, usePath: false);
            if (!flag)
            {
                MenuUI.alert(localization.format("Alert_Object"));
            }
            break;
        }
        return flag;
    }

    private static void resetFields()
    {
        nameField.text = "";
        descriptionField.text = "";
        pathField.text = "";
        previewField.text = "";
        changeField.text = "";
        allowedIPsField.text = "";
        refreshPathFieldNotification();
        refreshPreviewFieldNotification();
    }

    private static void onSwappedTypeState(SleekButtonState button, int state)
    {
        mapTypeState.isVisible = state == 0;
        itemTypeState.isVisible = state == 3;
        vehicleTypeState.isVisible = state == 4;
        skinTypeState.isVisible = state == 5;
        objectTypeState.isVisible = state == 2;
        refreshPathFieldNotification();
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuWorkshopUI.open();
        close();
    }

    public void OnDestroy()
    {
        TempSteamworksWorkshop workshopService = Provider.provider.workshopService;
        workshopService.onPublishedAdded = (TempSteamworksWorkshop.PublishedAdded)Delegate.Remove(workshopService.onPublishedAdded, new TempSteamworksWorkshop.PublishedAdded(onPublishedAdded));
        TempSteamworksWorkshop workshopService2 = Provider.provider.workshopService;
        workshopService2.onPublishedRemoved = (TempSteamworksWorkshop.PublishedRemoved)Delegate.Remove(workshopService2.onPublishedRemoved, new TempSteamworksWorkshop.PublishedRemoved(onPublishedRemoved));
    }

    public MenuWorkshopSubmitUI()
    {
        localization = Localization.read("/Menu/Workshop/MenuWorkshopSubmit.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Workshop/MenuWorkshopSubmit/MenuWorkshopSubmit.unity3d");
        publishedButtons = new List<ISleekButton>();
        TempSteamworksWorkshop workshopService = Provider.provider.workshopService;
        workshopService.onPublishedAdded = (TempSteamworksWorkshop.PublishedAdded)Delegate.Combine(workshopService.onPublishedAdded, new TempSteamworksWorkshop.PublishedAdded(onPublishedAdded));
        TempSteamworksWorkshop workshopService2 = Provider.provider.workshopService;
        workshopService2.onPublishedRemoved = (TempSteamworksWorkshop.PublishedRemoved)Delegate.Combine(workshopService2.onPublishedRemoved, new TempSteamworksWorkshop.PublishedRemoved(onPublishedRemoved));
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
        nameField = Glazier.Get().CreateStringField();
        nameField.positionOffset_X = -200;
        nameField.positionOffset_Y = 140;
        nameField.positionScale_X = 0.5f;
        nameField.sizeOffset_X = 200;
        nameField.sizeOffset_Y = 30;
        nameField.maxLength = 24;
        nameField.addLabel(localization.format("Name_Field_Label"), ESleekSide.RIGHT);
        container.AddChild(nameField);
        descriptionField = Glazier.Get().CreateStringField();
        descriptionField.positionOffset_X = -200;
        descriptionField.positionOffset_Y = 140;
        descriptionField.positionScale_X = 0.5f;
        descriptionField.sizeOffset_X = 400;
        descriptionField.sizeOffset_Y = 30;
        descriptionField.maxLength = 128;
        descriptionField.text = "";
        descriptionField.addLabel(localization.format("Description_Field_Label"), ESleekSide.RIGHT);
        container.AddChild(descriptionField);
        descriptionField.isVisible = false;
        pathField = Glazier.Get().CreateStringField();
        pathField.positionOffset_X = -200;
        pathField.positionOffset_Y = 180;
        pathField.positionScale_X = 0.5f;
        pathField.sizeOffset_X = 400;
        pathField.sizeOffset_Y = 30;
        pathField.maxLength = 128;
        pathField.addLabel(localization.format("Path_Field_Label"), ESleekSide.RIGHT);
        pathField.onTyped += onPathFieldTyped;
        container.AddChild(pathField);
        pathNotification = Glazier.Get().CreateBox();
        pathNotification.positionOffset_X = -240;
        pathNotification.positionOffset_Y = 180;
        pathNotification.positionScale_X = 0.5f;
        pathNotification.sizeOffset_X = 30;
        pathNotification.sizeOffset_Y = 30;
        pathNotification.text = "!";
        container.AddChild(pathNotification);
        pathNotification.isVisible = false;
        previewField = Glazier.Get().CreateStringField();
        previewField.positionOffset_X = -200;
        previewField.positionOffset_Y = 220;
        previewField.positionScale_X = 0.5f;
        previewField.sizeOffset_X = 400;
        previewField.sizeOffset_Y = 30;
        previewField.maxLength = 128;
        previewField.addLabel(localization.format("Preview_Field_Label"), ESleekSide.RIGHT);
        previewField.onTyped += onPreviewFieldTyped;
        container.AddChild(previewField);
        previewNotification = Glazier.Get().CreateBox();
        previewNotification.positionOffset_X = -240;
        previewNotification.positionOffset_Y = 220;
        previewNotification.positionScale_X = 0.5f;
        previewNotification.sizeOffset_X = 30;
        previewNotification.sizeOffset_Y = 30;
        previewNotification.text = "!";
        container.AddChild(previewNotification);
        previewNotification.isVisible = false;
        changeField = Glazier.Get().CreateStringField();
        changeField.positionOffset_X = -200;
        changeField.positionOffset_Y = 260;
        changeField.positionScale_X = 0.5f;
        changeField.sizeOffset_X = 400;
        changeField.sizeOffset_Y = 30;
        changeField.maxLength = 128;
        changeField.addLabel(localization.format("Change_Field_Label"), ESleekSide.RIGHT);
        container.AddChild(changeField);
        typeState = new SleekButtonState(new GUIContent(localization.format("Map")), new GUIContent(localization.format("Localization")), new GUIContent(localization.format("Object")), new GUIContent(localization.format("Item")), new GUIContent(localization.format("Vehicle")), new GUIContent(localization.format("Skin")));
        typeState.positionOffset_X = -200;
        typeState.positionOffset_Y = 300;
        typeState.positionScale_X = 0.5f;
        typeState.sizeOffset_X = 195;
        typeState.sizeOffset_Y = 30;
        typeState.onSwappedState = onSwappedTypeState;
        container.AddChild(typeState);
        mapTypeState = new SleekButtonState(new GUIContent(localization.format("Map_Type_Survival")), new GUIContent(localization.format("Map_Type_Horde")), new GUIContent(localization.format("Map_Type_Arena")), new GUIContent(localization.format("Map_Type_Custom")));
        mapTypeState.positionOffset_X = 5;
        mapTypeState.positionOffset_Y = 300;
        mapTypeState.positionScale_X = 0.5f;
        mapTypeState.sizeOffset_X = 195;
        mapTypeState.sizeOffset_Y = 30;
        container.AddChild(mapTypeState);
        mapTypeState.isVisible = true;
        itemTypeState = new SleekButtonState(new GUIContent(localization.format("Item_Type_Backpack")), new GUIContent(localization.format("Item_Type_Barrel")), new GUIContent(localization.format("Item_Type_Barricade")), new GUIContent(localization.format("Item_Type_Fisher")), new GUIContent(localization.format("Item_Type_Food")), new GUIContent(localization.format("Item_Type_Fuel")), new GUIContent(localization.format("Item_Type_Glasses")), new GUIContent(localization.format("Item_Type_Grip")), new GUIContent(localization.format("Item_Type_Grower")), new GUIContent(localization.format("Item_Type_Gun")), new GUIContent(localization.format("Item_Type_Hat")), new GUIContent(localization.format("Item_Type_Magazine")), new GUIContent(localization.format("Item_Type_Mask")), new GUIContent(localization.format("Item_Type_Medical")), new GUIContent(localization.format("Item_Type_Melee")), new GUIContent(localization.format("Item_Type_Optic")), new GUIContent(localization.format("Item_Type_Shirt")), new GUIContent(localization.format("Item_Type_Sight")), new GUIContent(localization.format("Item_Type_Structure")), new GUIContent(localization.format("Item_Type_Supply")), new GUIContent(localization.format("Item_Type_Tactical")), new GUIContent(localization.format("Item_Type_Throwable")), new GUIContent(localization.format("Item_Type_Tool")), new GUIContent(localization.format("Item_Type_Vest")), new GUIContent(localization.format("Item_Type_Water")));
        itemTypeState.positionOffset_X = 5;
        itemTypeState.positionOffset_Y = 300;
        itemTypeState.positionScale_X = 0.5f;
        itemTypeState.sizeOffset_X = 195;
        itemTypeState.sizeOffset_Y = 30;
        container.AddChild(itemTypeState);
        itemTypeState.isVisible = false;
        vehicleTypeState = new SleekButtonState(new GUIContent(localization.format("Vehicle_Type_Wheels_2")), new GUIContent(localization.format("Vehicle_Type_Wheels_4")), new GUIContent(localization.format("Vehicle_Type_Plane")), new GUIContent(localization.format("Vehicle_Type_Helicopter")), new GUIContent(localization.format("Vehicle_Type_Boat")), new GUIContent(localization.format("Vehicle_Type_Train")));
        vehicleTypeState.positionOffset_X = 5;
        vehicleTypeState.positionOffset_Y = 300;
        vehicleTypeState.positionScale_X = 0.5f;
        vehicleTypeState.sizeOffset_X = 195;
        vehicleTypeState.sizeOffset_Y = 30;
        container.AddChild(vehicleTypeState);
        vehicleTypeState.isVisible = false;
        skinTypeState = new SleekButtonState(new GUIContent(localization.format("Skin_Type_Generic_Pattern")), new GUIContent(localization.format("Skin_Type_Ace")), new GUIContent(localization.format("Skin_Type_Augewehr")), new GUIContent(localization.format("Skin_Type_Avenger")), new GUIContent(localization.format("Skin_Type_Bluntforce")), new GUIContent(localization.format("Skin_Type_Bulldog")), new GUIContent(localization.format("Skin_Type_Butterfly_Knife")), new GUIContent(localization.format("Skin_Type_Calling_Card")), new GUIContent(localization.format("Skin_Type_Cobra")), new GUIContent(localization.format("Skin_Type_Colt")), new GUIContent(localization.format("Skin_Type_Compound_Bow")), new GUIContent(localization.format("Skin_Type_Crossbow")), new GUIContent(localization.format("Skin_Type_Desert_Falcon")), new GUIContent(localization.format("Skin_Type_Dragonfang")), new GUIContent(localization.format("Skin_Type_Eaglefire")), new GUIContent(localization.format("Skin_Type_Ekho")), new GUIContent(localization.format("Skin_Type_Fusilaut")), new GUIContent(localization.format("Skin_Type_Grizzly")), new GUIContent(localization.format("Skin_Type_Hawkhound")), new GUIContent(localization.format("Skin_Type_Heartbreaker")), new GUIContent(localization.format("Skin_Type_Hell_Fury")), new GUIContent(localization.format("Skin_Type_Honeybadger")), new GUIContent(localization.format("Skin_Type_Katana")), new GUIContent(localization.format("Skin_Type_Kryzkarek")), new GUIContent(localization.format("Skin_Type_Machete")), new GUIContent(localization.format("Skin_Type_Maplestrike")), new GUIContent(localization.format("Skin_Type_Maschinengewehr")), new GUIContent(localization.format("Skin_Type_Masterkey")), new GUIContent(localization.format("Skin_Type_Matamorez")), new GUIContent(localization.format("Skin_Type_Military_Knife")), new GUIContent(localization.format("Skin_Type_Nightraider")), new GUIContent(localization.format("Skin_Type_Nykorev")), new GUIContent(localization.format("Skin_Type_Peacemaker")), new GUIContent(localization.format("Skin_Type_Rocket_Launcher")), new GUIContent(localization.format("Skin_Type_Sabertooth")), new GUIContent(localization.format("Skin_Type_Scalar")), new GUIContent(localization.format("Skin_Type_Schofield")), new GUIContent(localization.format("Skin_Type_Shadowstalker")), new GUIContent(localization.format("Skin_Type_Snayperskya")), new GUIContent(localization.format("Skin_Type_Sportshot")), new GUIContent(localization.format("Skin_Type_Teklowvka")), new GUIContent(localization.format("Skin_Type_Timberwolf")), new GUIContent(localization.format("Skin_Type_Viper")), new GUIContent(localization.format("Skin_Type_Vonya")), new GUIContent(localization.format("Skin_Type_Yuri")), new GUIContent(localization.format("Skin_Type_Zubeknakov")));
        skinTypeState.positionOffset_X = 5;
        skinTypeState.positionOffset_Y = 300;
        skinTypeState.positionScale_X = 0.5f;
        skinTypeState.sizeOffset_X = 195;
        skinTypeState.sizeOffset_Y = 30;
        container.AddChild(skinTypeState);
        skinTypeState.isVisible = false;
        objectTypeState = new SleekButtonState(new GUIContent(localization.format("Object_Type_Model")), new GUIContent(localization.format("Object_Type_Resource")), new GUIContent(localization.format("Object_Type_Effect")), new GUIContent(localization.format("Object_Type_Animal")));
        objectTypeState.positionOffset_X = 5;
        objectTypeState.positionOffset_Y = 300;
        objectTypeState.positionScale_X = 0.5f;
        objectTypeState.sizeOffset_X = 195;
        objectTypeState.sizeOffset_Y = 30;
        container.AddChild(objectTypeState);
        objectTypeState.isVisible = false;
        visibilityState = new SleekButtonState(new GUIContent(localization.format("Public")), new GUIContent(localization.format("Friends_Only")), new GUIContent(localization.format("Private")), new GUIContent(localization.format("Unlisted")));
        visibilityState.positionOffset_X = -200;
        visibilityState.positionOffset_Y = 340;
        visibilityState.positionScale_X = 0.5f;
        visibilityState.sizeOffset_X = 195;
        visibilityState.sizeOffset_Y = 30;
        container.AddChild(visibilityState);
        forState = new SleekButtonState(new GUIContent(localization.format("Community")), new GUIContent(localization.format("Review")));
        forState.positionOffset_X = 5;
        forState.positionOffset_Y = 340;
        forState.positionScale_X = 0.5f;
        forState.sizeOffset_X = 195;
        forState.sizeOffset_Y = 30;
        container.AddChild(forState);
        allowedIPsField = Glazier.Get().CreateStringField();
        allowedIPsField.positionOffset_X = -200;
        allowedIPsField.positionOffset_Y = 380;
        allowedIPsField.positionScale_X = 0.5f;
        allowedIPsField.sizeOffset_X = 400;
        allowedIPsField.sizeOffset_Y = 30;
        allowedIPsField.maxLength = 255;
        allowedIPsField.tooltipText = localization.format("Allowed_IPs_Tooltip");
        allowedIPsField.hint = localization.format("Allowed_IPs_Hint");
        allowedIPsField.addLabel(localization.format("Allowed_IPs_Label"), ESleekSide.RIGHT);
        container.AddChild(allowedIPsField);
        createButton = new SleekButtonIcon(bundle.load<Texture2D>("Create"));
        createButton.positionOffset_X = -200;
        createButton.positionOffset_Y = 420;
        createButton.positionScale_X = 0.5f;
        createButton.sizeOffset_X = 195;
        createButton.sizeOffset_Y = 30;
        createButton.text = localization.format("Create_Button");
        createButton.tooltip = localization.format("Create_Button_Tooltip");
        createButton.onClickedButton += onClickedCreateButton;
        createButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(createButton);
        legalButton = Glazier.Get().CreateButton();
        legalButton.positionOffset_X = 5;
        legalButton.positionOffset_Y = 420;
        legalButton.positionScale_X = 0.5f;
        legalButton.sizeOffset_X = 195;
        legalButton.sizeOffset_Y = 30;
        legalButton.fontSize = ESleekFontSize.Small;
        legalButton.text = localization.format("Legal_Button");
        legalButton.tooltipText = localization.format("Legal_Button_Tooltip");
        legalButton.onClickedButton += onClickedLegalButton;
        container.AddChild(legalButton);
        publishedBox = Glazier.Get().CreateScrollView();
        publishedBox.positionOffset_X = -200;
        publishedBox.positionOffset_Y = 460;
        publishedBox.positionScale_X = 0.5f;
        publishedBox.sizeOffset_X = 430;
        publishedBox.sizeOffset_Y = -460;
        publishedBox.sizeScale_Y = 1f;
        publishedBox.scaleContentToWidth = true;
        container.AddChild(publishedBox);
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
        onPublishedAdded();
        bundle.unload();
    }
}
