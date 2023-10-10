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
            descriptionBox.Text = desc;
        }
        if (localization != null && localization.has("Name"))
        {
            selectedBox.Text = localization.format("Name");
        }
        else
        {
            selectedBox.Text = PlaySettings.editorMap;
        }
        if (previewImage.Texture != null && previewImage.ShouldDestroyTexture)
        {
            UnityEngine.Object.Destroy(previewImage.Texture);
            previewImage.Texture = null;
        }
        string previewImageFilePath = levelInfo.GetPreviewImageFilePath();
        if (!string.IsNullOrEmpty(previewImageFilePath))
        {
            previewImage.Texture = ReadWrite.readTextureFromFile(previewImageFilePath);
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
        if (mapNameField.Text != "")
        {
            Level.add(mapNameField.Text, (ELevelSize)(mapSizeState.state + 1), (mapTypeState.state != 0) ? ELevelType.ARENA : ELevelType.SURVIVAL);
            mapNameField.Text = "";
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
                sleekLevel.PositionOffset_Y = i * 110;
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
        levelScrollBox.ContentSizeOffset = new Vector2(0f, levels.Length * 110 - 10);
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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        previewBox = Glazier.Get().CreateBox();
        previewBox.PositionOffset_X = -305f;
        previewBox.PositionOffset_Y = 80f;
        previewBox.PositionScale_X = 0.5f;
        previewBox.SizeOffset_X = 340f;
        previewBox.SizeOffset_Y = 200f;
        container.AddChild(previewBox);
        previewImage = Glazier.Get().CreateImage();
        previewImage.PositionOffset_X = 10f;
        previewImage.PositionOffset_Y = 10f;
        previewImage.SizeOffset_X = -20f;
        previewImage.SizeOffset_Y = -20f;
        previewImage.SizeScale_X = 1f;
        previewImage.SizeScale_Y = 1f;
        previewImage.ShouldDestroyTexture = true;
        previewBox.AddChild(previewImage);
        levelScrollBox = Glazier.Get().CreateScrollView();
        levelScrollBox.PositionOffset_X = -95f;
        levelScrollBox.PositionOffset_Y = 290f;
        levelScrollBox.PositionScale_X = 0.5f;
        levelScrollBox.SizeOffset_X = 430f;
        levelScrollBox.SizeOffset_Y = -390f;
        levelScrollBox.SizeScale_Y = 1f;
        levelScrollBox.ScaleContentToWidth = true;
        container.AddChild(levelScrollBox);
        selectedBox = Glazier.Get().CreateBox();
        selectedBox.PositionOffset_X = 45f;
        selectedBox.PositionOffset_Y = 80f;
        selectedBox.PositionScale_X = 0.5f;
        selectedBox.SizeOffset_X = 260f;
        selectedBox.SizeOffset_Y = 30f;
        container.AddChild(selectedBox);
        descriptionBox = Glazier.Get().CreateBox();
        descriptionBox.PositionOffset_X = 45f;
        descriptionBox.PositionOffset_Y = 120f;
        descriptionBox.PositionScale_X = 0.5f;
        descriptionBox.SizeOffset_X = 260f;
        descriptionBox.SizeOffset_Y = 160f;
        descriptionBox.TextAlignment = TextAnchor.UpperCenter;
        descriptionBox.AllowRichText = true;
        descriptionBox.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        container.AddChild(descriptionBox);
        mapNameField = Glazier.Get().CreateStringField();
        mapNameField.PositionOffset_X = -305f;
        mapNameField.PositionOffset_Y = 370f;
        mapNameField.PositionScale_X = 0.5f;
        mapNameField.SizeOffset_X = 200f;
        mapNameField.SizeOffset_Y = 30f;
        mapNameField.MaxLength = 24;
        mapNameField.AddLabel(local.format("Name_Field_Label"), ESleekSide.LEFT);
        container.AddChild(mapNameField);
        mapSizeState = new SleekButtonState(new GUIContent(MenuPlaySingleplayerUI.localization.format("Small")), new GUIContent(MenuPlaySingleplayerUI.localization.format("Medium")), new GUIContent(MenuPlaySingleplayerUI.localization.format("Large")));
        mapSizeState.PositionOffset_X = -305f;
        mapSizeState.PositionOffset_Y = 410f;
        mapSizeState.PositionScale_X = 0.5f;
        mapSizeState.SizeOffset_X = 200f;
        mapSizeState.SizeOffset_Y = 30f;
        container.AddChild(mapSizeState);
        mapTypeState = new SleekButtonState(new GUIContent(MenuPlaySingleplayerUI.localization.format("Survival")), new GUIContent(MenuPlaySingleplayerUI.localization.format("Arena")));
        mapTypeState.PositionOffset_X = -305f;
        mapTypeState.PositionOffset_Y = 450f;
        mapTypeState.PositionScale_X = 0.5f;
        mapTypeState.SizeOffset_X = 200f;
        mapTypeState.SizeOffset_Y = 30f;
        container.AddChild(mapTypeState);
        addButton = new SleekButtonIcon(icons.load<Texture2D>("Add"));
        addButton.PositionOffset_X = -305f;
        addButton.PositionOffset_Y = 490f;
        addButton.PositionScale_X = 0.5f;
        addButton.SizeOffset_X = 200f;
        addButton.SizeOffset_Y = 30f;
        addButton.text = local.format("Add_Button");
        addButton.tooltip = local.format("Add_Button_Tooltip");
        addButton.onClickedButton += onClickedAddButton;
        container.AddChild(addButton);
        removeButton = new SleekButtonIconConfirm(icons.load<Texture2D>("Remove"), local.format("Remove_Button_Confirm"), local.format("Remove_Button_Confirm_Tooltip"), local.format("Remove_Button_Deny"), local.format("Remove_Button_Deny_Tooltip"));
        removeButton.PositionOffset_X = -305f;
        removeButton.PositionOffset_Y = 530f;
        removeButton.PositionScale_X = 0.5f;
        removeButton.SizeOffset_X = 200f;
        removeButton.SizeOffset_Y = 30f;
        removeButton.text = local.format("Remove_Button");
        removeButton.tooltip = local.format("Remove_Button_Tooltip");
        removeButton.onConfirmed = onClickedRemoveButton;
        container.AddChild(removeButton);
        if (ReadWrite.SupportsOpeningFileBrowser)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.PositionOffset_X = -305f;
            sleekButton.PositionOffset_Y = 330f;
            sleekButton.PositionScale_X = 0.5f;
            sleekButton.SizeOffset_X = 200f;
            sleekButton.SizeOffset_Y = 30f;
            sleekButton.Text = local.format("BrowseFiles_Label");
            sleekButton.OnClicked += OnClickedBrowseFilesButton;
            container.AddChild(sleekButton);
        }
        editButton = new SleekButtonIcon(icons.load<Texture2D>("Edit"));
        editButton.PositionOffset_X = -305f;
        editButton.PositionOffset_Y = 290f;
        editButton.PositionScale_X = 0.5f;
        editButton.SizeOffset_X = 200f;
        editButton.SizeOffset_Y = 30f;
        editButton.text = local.format("Edit_Button");
        editButton.tooltip = local.format("Edit_Button_Tooltip");
        editButton.iconColor = ESleekTint.FOREGROUND;
        editButton.onClickedButton += onClickedEditButton;
        container.AddChild(editButton);
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
        onLevelsRefreshed();
        Level.onLevelsRefreshed = (LevelsRefreshed)Delegate.Combine(Level.onLevelsRefreshed, new LevelsRefreshed(onLevelsRefreshed));
    }
}
