using System;
using UnityEngine;

namespace SDG.Unturned;

public class MenuWorkshopEditorUI
{
    public static Bundle icons;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static LevelInfo[] levels;

    private static ISleekBox previewBox;

    private static ISleekImage previewImage;

    private static ISleekScrollView levelScrollBox;

    private static SleekLevel[] levelButtons;

    private static ISleekField mapNameField;

    private static SleekButtonState mapSizeState;

    private static SleekButtonState mapTypeState;

    private static SleekButtonIcon addButton;

    private static SleekButtonIconConfirm removeButton;

    private static SleekButtonIcon editButton;

    private static ISleekBox selectedBox;

    private static ISleekBox descriptionBox;

    public static void open()
    {
        if (!active)
        {
            active = true;
            removeButton.reset();
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

    private static void updateSelection()
    {
        if (string.IsNullOrEmpty(PlaySettings.editorMap))
        {
            UnturnedLog.warn("Editor map selection empty");
            return;
        }
        LevelInfo levelInfo = null;
        LevelInfo[] array = levels;
        foreach (LevelInfo levelInfo2 in array)
        {
            if (string.Equals(levelInfo2.name, PlaySettings.editorMap, StringComparison.InvariantCultureIgnoreCase))
            {
                levelInfo = levelInfo2;
                break;
            }
        }
        if (levelInfo == null)
        {
            UnturnedLog.warn("Unable to find editor selected map '{0}'", PlaySettings.editorMap);
            return;
        }
        Local localization = levelInfo.getLocalization();
        if (localization != null)
        {
            string desc = localization.format("Description");
            desc = ItemTool.filterRarityRichText(desc);
            RichTextUtil.replaceNewlineMarkup(ref desc);
            descriptionBox.text = desc;
        }
        if (localization != null && localization.has("Name"))
        {
            selectedBox.text = localization.format("Name");
        }
        else
        {
            selectedBox.text = PlaySettings.editorMap;
        }
        if (previewImage.texture != null && previewImage.shouldDestroyTexture)
        {
            UnityEngine.Object.Destroy(previewImage.texture);
            previewImage.texture = null;
        }
        string previewImageFilePath = levelInfo.GetPreviewImageFilePath();
        if (!string.IsNullOrEmpty(previewImageFilePath))
        {
            previewImage.texture = ReadWrite.readTextureFromFile(previewImageFilePath);
        }
    }

    private static void onClickedLevel(SleekLevel level, byte index)
    {
        if (index < levels.Length && levels[index] != null && levels[index].isEditable)
        {
            PlaySettings.editorMap = levels[index].name;
            updateSelection();
        }
    }

    private static void onClickedAddButton(ISleekElement button)
    {
        if (mapNameField.text != "")
        {
            Level.add(mapNameField.text, (ELevelSize)(mapSizeState.state + 1), (mapTypeState.state != 0) ? ELevelType.ARENA : ELevelType.SURVIVAL);
            mapNameField.text = "";
        }
    }

    private static void onClickedRemoveButton(SleekButtonIconConfirm button)
    {
        if (PlaySettings.editorMap == null || PlaySettings.editorMap.Length == 0)
        {
            return;
        }
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null && levels[i].name == PlaySettings.editorMap && levels[i].isEditable)
            {
                Level.remove(levels[i].name);
            }
        }
    }

    private static void onClickedEditButton(ISleekElement button)
    {
        if (PlaySettings.editorMap == null || PlaySettings.editorMap.Length == 0)
        {
            return;
        }
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null && levels[i].name == PlaySettings.editorMap && levels[i].isEditable)
            {
                MenuSettings.save();
                Level.edit(levels[i]);
            }
        }
    }

    protected void OnClickedBrowseFilesButton(ISleekElement button)
    {
        if (PlaySettings.editorMap == null || PlaySettings.editorMap.Length == 0)
        {
            return;
        }
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null && levels[i].name == PlaySettings.editorMap && levels[i].isEditable)
            {
                ReadWrite.OpenFileBrowser(levels[i].path);
                break;
            }
        }
    }

    private static void onLevelsRefreshed()
    {
        if (levelScrollBox == null)
        {
            return;
        }
        levelScrollBox.RemoveAllChildren();
        levels = Level.getLevels(ESingleplayerMapCategory.EDITABLE);
        bool flag = false;
        levelButtons = new SleekLevel[levels.Length];
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null)
            {
                SleekLevel sleekLevel = new SleekLevel(levels[i], isEditor: true);
                sleekLevel.positionOffset_Y = i * 110;
                sleekLevel.onClickedLevel = onClickedLevel;
                levelScrollBox.AddChild(sleekLevel);
                levelButtons[i] = sleekLevel;
                if (!flag && string.Equals(levels[i].name, PlaySettings.editorMap, StringComparison.InvariantCultureIgnoreCase))
                {
                    flag = true;
                }
            }
        }
        if (levels.Length == 0)
        {
            PlaySettings.editorMap = "";
        }
        else if (!flag || PlaySettings.editorMap == null || PlaySettings.editorMap.Length == 0)
        {
            PlaySettings.editorMap = levels[0].name;
        }
        updateSelection();
        levelScrollBox.contentSizeOffset = new Vector2(0f, levels.Length * 110 - 10);
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuWorkshopUI.open();
        close();
    }

    public void OnDestroy()
    {
        Level.onLevelsRefreshed = (LevelsRefreshed)Delegate.Remove(Level.onLevelsRefreshed, new LevelsRefreshed(onLevelsRefreshed));
    }

    public MenuWorkshopEditorUI()
    {
        if (icons != null)
        {
            icons.unload();
            icons = null;
        }
        Local local = Localization.read("/Menu/Workshop/MenuWorkshopEditor.dat");
        icons = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Workshop/MenuWorkshopEditor/MenuWorkshopEditor.unity3d");
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
        previewBox = Glazier.Get().CreateBox();
        previewBox.positionOffset_X = -305;
        previewBox.positionOffset_Y = 80;
        previewBox.positionScale_X = 0.5f;
        previewBox.sizeOffset_X = 340;
        previewBox.sizeOffset_Y = 200;
        container.AddChild(previewBox);
        previewImage = Glazier.Get().CreateImage();
        previewImage.positionOffset_X = 10;
        previewImage.positionOffset_Y = 10;
        previewImage.sizeOffset_X = -20;
        previewImage.sizeOffset_Y = -20;
        previewImage.sizeScale_X = 1f;
        previewImage.sizeScale_Y = 1f;
        previewImage.shouldDestroyTexture = true;
        previewBox.AddChild(previewImage);
        levelScrollBox = Glazier.Get().CreateScrollView();
        levelScrollBox.positionOffset_X = -95;
        levelScrollBox.positionOffset_Y = 290;
        levelScrollBox.positionScale_X = 0.5f;
        levelScrollBox.sizeOffset_X = 430;
        levelScrollBox.sizeOffset_Y = -390;
        levelScrollBox.sizeScale_Y = 1f;
        levelScrollBox.scaleContentToWidth = true;
        container.AddChild(levelScrollBox);
        selectedBox = Glazier.Get().CreateBox();
        selectedBox.positionOffset_X = 45;
        selectedBox.positionOffset_Y = 80;
        selectedBox.positionScale_X = 0.5f;
        selectedBox.sizeOffset_X = 260;
        selectedBox.sizeOffset_Y = 30;
        container.AddChild(selectedBox);
        descriptionBox = Glazier.Get().CreateBox();
        descriptionBox.positionOffset_X = 45;
        descriptionBox.positionOffset_Y = 120;
        descriptionBox.positionScale_X = 0.5f;
        descriptionBox.sizeOffset_X = 260;
        descriptionBox.sizeOffset_Y = 160;
        descriptionBox.fontAlignment = TextAnchor.UpperCenter;
        descriptionBox.enableRichText = true;
        descriptionBox.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        container.AddChild(descriptionBox);
        mapNameField = Glazier.Get().CreateStringField();
        mapNameField.positionOffset_X = -305;
        mapNameField.positionOffset_Y = 370;
        mapNameField.positionScale_X = 0.5f;
        mapNameField.sizeOffset_X = 200;
        mapNameField.sizeOffset_Y = 30;
        mapNameField.maxLength = 24;
        mapNameField.addLabel(local.format("Name_Field_Label"), ESleekSide.LEFT);
        container.AddChild(mapNameField);
        mapSizeState = new SleekButtonState(new GUIContent(MenuPlaySingleplayerUI.localization.format("Small")), new GUIContent(MenuPlaySingleplayerUI.localization.format("Medium")), new GUIContent(MenuPlaySingleplayerUI.localization.format("Large")));
        mapSizeState.positionOffset_X = -305;
        mapSizeState.positionOffset_Y = 410;
        mapSizeState.positionScale_X = 0.5f;
        mapSizeState.sizeOffset_X = 200;
        mapSizeState.sizeOffset_Y = 30;
        container.AddChild(mapSizeState);
        mapTypeState = new SleekButtonState(new GUIContent(MenuPlaySingleplayerUI.localization.format("Survival")), new GUIContent(MenuPlaySingleplayerUI.localization.format("Arena")));
        mapTypeState.positionOffset_X = -305;
        mapTypeState.positionOffset_Y = 450;
        mapTypeState.positionScale_X = 0.5f;
        mapTypeState.sizeOffset_X = 200;
        mapTypeState.sizeOffset_Y = 30;
        container.AddChild(mapTypeState);
        addButton = new SleekButtonIcon(icons.load<Texture2D>("Add"));
        addButton.positionOffset_X = -305;
        addButton.positionOffset_Y = 490;
        addButton.positionScale_X = 0.5f;
        addButton.sizeOffset_X = 200;
        addButton.sizeOffset_Y = 30;
        addButton.text = local.format("Add_Button");
        addButton.tooltip = local.format("Add_Button_Tooltip");
        addButton.onClickedButton += onClickedAddButton;
        container.AddChild(addButton);
        removeButton = new SleekButtonIconConfirm(icons.load<Texture2D>("Remove"), local.format("Remove_Button_Confirm"), local.format("Remove_Button_Confirm_Tooltip"), local.format("Remove_Button_Deny"), local.format("Remove_Button_Deny_Tooltip"));
        removeButton.positionOffset_X = -305;
        removeButton.positionOffset_Y = 530;
        removeButton.positionScale_X = 0.5f;
        removeButton.sizeOffset_X = 200;
        removeButton.sizeOffset_Y = 30;
        removeButton.text = local.format("Remove_Button");
        removeButton.tooltip = local.format("Remove_Button_Tooltip");
        removeButton.onConfirmed = onClickedRemoveButton;
        container.AddChild(removeButton);
        if (ReadWrite.SupportsOpeningFileBrowser)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.positionOffset_X = -305;
            sleekButton.positionOffset_Y = 330;
            sleekButton.positionScale_X = 0.5f;
            sleekButton.sizeOffset_X = 200;
            sleekButton.sizeOffset_Y = 30;
            sleekButton.text = local.format("BrowseFiles_Label");
            sleekButton.onClickedButton += OnClickedBrowseFilesButton;
            container.AddChild(sleekButton);
        }
        editButton = new SleekButtonIcon(icons.load<Texture2D>("Edit"));
        editButton.positionOffset_X = -305;
        editButton.positionOffset_Y = 290;
        editButton.positionScale_X = 0.5f;
        editButton.sizeOffset_X = 200;
        editButton.sizeOffset_Y = 30;
        editButton.text = local.format("Edit_Button");
        editButton.tooltip = local.format("Edit_Button_Tooltip");
        editButton.iconColor = ESleekTint.FOREGROUND;
        editButton.onClickedButton += onClickedEditButton;
        container.AddChild(editButton);
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
        onLevelsRefreshed();
        Level.onLevelsRefreshed = (LevelsRefreshed)Delegate.Combine(Level.onLevelsRefreshed, new LevelsRefreshed(onLevelsRefreshed));
    }
}
