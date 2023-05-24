using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class MenuPlaySingleplayerUI
{
    public static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static LevelInfo[] levels;

    private static ISleekBox previewBox;

    private static ISleekImage previewImage;

    private static ISleekScrollView levelScrollBox;

    private static SleekLevel[] levelButtons;

    private static SleekButtonIcon playButton;

    private static SleekButtonState modeButtonState;

    private static ISleekButton configButton;

    private static SleekButtonIconConfirm resetButton;

    private static ISleekButton browseServersButton;

    private static ISleekBox selectedBox;

    private static ISleekBox descriptionBox;

    private static ISleekToggle cheatsToggle;

    private static ISleekBox creditsBox;

    private static ISleekButton itemButton;

    private static ISleekButton feedbackButton;

    private static ISleekButton newsButton;

    private static ISleekButton officalMapsButton;

    private static ISleekButton curatedMapsButton;

    private static ISleekButton workshopMapsButton;

    private static ISleekButton miscMapsButton;

    private static SleekNew curatedStatusLabel;

    private static int featuredItemDefId;

    private bool hasCreatedFeaturedMapLabel;

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
            MenuSettings.save();
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private static void updateSelection()
    {
        if (string.IsNullOrEmpty(PlaySettings.singleplayerMap))
        {
            UnturnedLog.warn("Singleplayer map selection empty");
            return;
        }
        LevelInfo levelInfo = null;
        LevelInfo[] array = levels;
        foreach (LevelInfo levelInfo2 in array)
        {
            if (string.Equals(levelInfo2.name, PlaySettings.singleplayerMap, StringComparison.InvariantCultureIgnoreCase))
            {
                levelInfo = levelInfo2;
                break;
            }
        }
        if (levelInfo == null)
        {
            UnturnedLog.warn("Unable to find singleplayer selected map '{0}'", PlaySettings.singleplayerMap);
            return;
        }
        Local local = levelInfo.getLocalization();
        if (local != null)
        {
            string desc = local.format("Description");
            desc = ItemTool.filterRarityRichText(desc);
            RichTextUtil.replaceNewlineMarkup(ref desc);
            descriptionBox.text = desc;
        }
        if (local != null && local.has("Name"))
        {
            selectedBox.text = local.format("Name");
        }
        else
        {
            selectedBox.text = PlaySettings.singleplayerMap;
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
        int num = creditsBox.positionOffset_Y;
        if (levelInfo.configData.Creators.Length != 0 || levelInfo.configData.Collaborators.Length != 0 || levelInfo.configData.Thanks.Length != 0)
        {
            int num2 = 0;
            string text = string.Empty;
            if (levelInfo.configData.Creators.Length != 0)
            {
                text += localization.format("Creators");
                num2 += 15;
                for (int j = 0; j < levelInfo.configData.Creators.Length; j++)
                {
                    text = text + "\n" + levelInfo.configData.Creators[j];
                    num2 += 15;
                }
            }
            if (levelInfo.configData.Collaborators.Length != 0)
            {
                if (text.Length > 0)
                {
                    text += "\n\n";
                    num2 += 30;
                }
                text += localization.format("Collaborators");
                num2 += 15;
                for (int k = 0; k < levelInfo.configData.Collaborators.Length; k++)
                {
                    text = text + "\n" + levelInfo.configData.Collaborators[k];
                    num2 += 15;
                }
            }
            if (levelInfo.configData.Thanks.Length != 0)
            {
                if (text.Length > 0)
                {
                    text += "\n\n";
                    num2 += 30;
                }
                text += localization.format("Thanks");
                num2 += 15;
                for (int l = 0; l < levelInfo.configData.Thanks.Length; l++)
                {
                    text = text + "\n" + levelInfo.configData.Thanks[l];
                    num2 += 15;
                }
            }
            num2 = Mathf.Max(num2, 40);
            creditsBox.sizeOffset_Y = num2;
            creditsBox.text = text;
            creditsBox.isVisible = true;
            num += num2 + 10;
        }
        else
        {
            creditsBox.isVisible = false;
        }
        List<int> list = new List<int>(4);
        if (levelInfo.configData.Item > 0 && !Provider.provider.economyService.isItemHiddenByCountryRestrictions(levelInfo.configData.Item))
        {
            list.Add(levelInfo.configData.Item);
        }
        if (levelInfo.configData.Associated_Stockpile_Items.Length != 0)
        {
            int[] associated_Stockpile_Items = levelInfo.configData.Associated_Stockpile_Items;
            foreach (int num3 in associated_Stockpile_Items)
            {
                if (num3 > 0 && !Provider.provider.economyService.isItemHiddenByCountryRestrictions(num3))
                {
                    list.Add(num3);
                }
            }
        }
        featuredItemDefId = list.RandomOrDefault();
        if (featuredItemDefId > 0)
        {
            itemButton.positionOffset_Y = num;
            itemButton.text = localization.format("Credits_Text", selectedBox.text, "<color=" + Palette.hex(Provider.provider.economyService.getInventoryColor(featuredItemDefId)) + ">" + Provider.provider.economyService.getInventoryName(featuredItemDefId) + "</color>");
            itemButton.tooltipText = localization.format("Credits_Tooltip");
            itemButton.isVisible = true;
            num += itemButton.sizeOffset_Y + 10;
        }
        else
        {
            itemButton.isVisible = false;
        }
        if (string.IsNullOrEmpty(levelInfo.feedbackUrl))
        {
            feedbackButton.isVisible = false;
        }
        else
        {
            feedbackButton.positionOffset_Y = num;
            feedbackButton.isVisible = true;
            num += feedbackButton.sizeOffset_Y + 10;
        }
        MainMenuWorkshopFeaturedLiveConfig featured = LiveConfig.Get().mainMenuWorkshop.featured;
        if (featured.IsFeatured(levelInfo.publishedFileId) && !string.IsNullOrEmpty(featured.linkURL))
        {
            newsButton.text = featured.linkText;
            newsButton.positionOffset_Y = num;
            newsButton.isVisible = true;
            _ = num + (newsButton.sizeOffset_Y + 10);
        }
        else
        {
            newsButton.isVisible = false;
        }
    }

    private static void onClickedLevel(SleekLevel level, byte index)
    {
        if (index < levels.Length && levels[index] != null)
        {
            PlaySettings.singleplayerMap = levels[index].name;
            updateSelection();
        }
    }

    private static void onClickedPlayButton(ISleekElement button)
    {
        if (!string.IsNullOrEmpty(PlaySettings.singleplayerMap))
        {
            Provider.map = PlaySettings.singleplayerMap;
            MenuSettings.save();
            Provider.singleplayer(PlaySettings.singleplayerMode, PlaySettings.singleplayerCheats);
        }
    }

    private static void onClickedOfficialMapsButton(ISleekElement button)
    {
        PlaySettings.singleplayerCategory = ESingleplayerMapCategory.OFFICIAL;
        refreshLevels();
    }

    private static void onClickedCuratedMapsButton(ISleekElement button)
    {
        if (curatedStatusLabel != null)
        {
            curatedMapsButton.RemoveChild(curatedStatusLabel);
            curatedStatusLabel = null;
            MainMenuWorkshopFeaturedLiveConfig featured = LiveConfig.Get().mainMenuWorkshop.featured;
            ConvenientSavedata.get().write("SingleplayerCuratedSeenId", featured.id);
        }
        PlaySettings.singleplayerCategory = ESingleplayerMapCategory.CURATED;
        refreshLevels();
    }

    private static void onClickedWorkshopMapsButton(ISleekElement button)
    {
        PlaySettings.singleplayerCategory = ESingleplayerMapCategory.WORKSHOP;
        refreshLevels();
    }

    private static void onClickedMiscMapsButton(ISleekElement button)
    {
        PlaySettings.singleplayerCategory = ESingleplayerMapCategory.MISC;
        refreshLevels();
    }

    private static void onClickedManageSubscriptionsButton(ISleekElement button)
    {
        MenuUI.closeAll();
        MenuWorkshopSubscriptionsUI.instance.open();
    }

    private static void onSwappedModeState(SleekButtonState button, int index)
    {
        PlaySettings.singleplayerMode = (EGameMode)index;
    }

    private static void onClickedConfigButton(ISleekElement button)
    {
        if (PlaySettings.singleplayerMap != null && PlaySettings.singleplayerMap.Length != 0)
        {
            MenuPlayConfigUI.open();
            close();
        }
    }

    private static void onClickedBrowseServersButton(ISleekElement button)
    {
        if (!string.IsNullOrEmpty(PlaySettings.singleplayerMap))
        {
            MenuPlayServersUI.openForMap(PlaySettings.singleplayerMap);
            close();
        }
    }

    private static void onClickedResetButton(SleekButtonIconConfirm button)
    {
        if (PlaySettings.singleplayerMap != null && PlaySettings.singleplayerMap.Length != 0)
        {
            if (ReadWrite.folderExists("/Worlds/Singleplayer_" + Characters.selected + "/Level/" + PlaySettings.singleplayerMap))
            {
                ReadWrite.deleteFolder("/Worlds/Singleplayer_" + Characters.selected + "/Level/" + PlaySettings.singleplayerMap);
            }
            if (ReadWrite.folderExists("/Worlds/Singleplayer_" + Characters.selected + "/Players/" + Provider.user.ToString() + "_" + Characters.selected + "/" + PlaySettings.singleplayerMap))
            {
                ReadWrite.deleteFolder("/Worlds/Singleplayer_" + Characters.selected + "/Players/" + Provider.user.ToString() + "_" + Characters.selected + "/" + PlaySettings.singleplayerMap);
            }
        }
    }

    private static void onToggledCheatsToggle(ISleekToggle toggle, bool state)
    {
        PlaySettings.singleplayerCheats = state;
    }

    private static void refreshLevels()
    {
        if (levelScrollBox == null)
        {
            return;
        }
        levelScrollBox.RemoveAllChildren();
        levels = Level.getLevels(PlaySettings.singleplayerCategory);
        bool flag = false;
        int num = 0;
        levelButtons = new SleekLevel[levels.Length];
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null)
            {
                SleekLevel sleekLevel = new SleekLevel(levels[i], isEditor: false);
                sleekLevel.positionOffset_Y = num;
                sleekLevel.onClickedLevel = onClickedLevel;
                levelScrollBox.AddChild(sleekLevel);
                num += 110;
                levelButtons[i] = sleekLevel;
                if (!flag && string.Equals(levels[i].name, PlaySettings.singleplayerMap, StringComparison.InvariantCultureIgnoreCase))
                {
                    flag = true;
                }
            }
        }
        if (levels.Length == 0)
        {
            PlaySettings.singleplayerMap = "";
        }
        else if (!flag || PlaySettings.singleplayerMap == null || PlaySettings.singleplayerMap.Length == 0)
        {
            PlaySettings.singleplayerMap = levels[0].name;
        }
        updateSelection();
        if (PlaySettings.singleplayerCategory == ESingleplayerMapCategory.CURATED)
        {
            foreach (CuratedMapLink curated_Map_Link in Provider.statusData.Maps.Curated_Map_Links)
            {
                if (!Provider.provider.workshopService.getSubscribed(curated_Map_Link.Workshop_File_Id) && curated_Map_Link.Visible_In_Singleplayer_Recommendations_List)
                {
                    SleekCuratedLevelLink sleekCuratedLevelLink = new SleekCuratedLevelLink(curated_Map_Link);
                    sleekCuratedLevelLink.positionOffset_Y = num;
                    levelScrollBox.AddChild(sleekCuratedLevelLink);
                    num += 110;
                }
            }
        }
        if (PlaySettings.singleplayerCategory == ESingleplayerMapCategory.WORKSHOP)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.positionOffset_Y = num;
            sleekButton.sizeOffset_X = 400;
            sleekButton.sizeOffset_Y = 30;
            sleekButton.text = localization.format("Manage_Workshop_Label");
            sleekButton.tooltipText = localization.format("Manage_Workshop_Tooltip");
            sleekButton.onClickedButton += onClickedManageSubscriptionsButton;
            levelScrollBox.AddChild(sleekButton);
            num += 40;
        }
        levelScrollBox.contentSizeOffset = new Vector2(0f, num - 10);
    }

    private static void onLevelsRefreshed()
    {
        refreshLevels();
    }

    private static void onClickedItemButton(ISleekElement button)
    {
        if (PlaySettings.singleplayerMap != null && PlaySettings.singleplayerMap.Length != 0 && Level.getLevel(PlaySettings.singleplayerMap) != null && featuredItemDefId > 0)
        {
            ItemStore.Get().ViewItem(featuredItemDefId);
        }
    }

    private static void onClickedFeedbackButton(ISleekElement button)
    {
        if (PlaySettings.singleplayerMap == null || PlaySettings.singleplayerMap.Length == 0)
        {
            return;
        }
        LevelInfo level = Level.getLevel(PlaySettings.singleplayerMap);
        if (level != null)
        {
            if (WebUtils.ParseThirdPartyUrl(level.feedbackUrl, out var result))
            {
                Provider.provider.browserService.open(result);
                return;
            }
            UnturnedLog.warn("Ignoring potentially unsafe level feedback url {0}", level.feedbackUrl);
        }
    }

    private static void onClickedNewsButton(ISleekElement button)
    {
        MainMenuWorkshopFeaturedLiveConfig featured = LiveConfig.Get().mainMenuWorkshop.featured;
        Provider.provider.browserService.open(featured.linkURL);
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuPlayUI.open();
        close();
    }

    private void OnLiveConfigRefreshed()
    {
        if (!hasCreatedFeaturedMapLabel)
        {
            MainMenuWorkshopFeaturedLiveConfig featured = LiveConfig.Get().mainMenuWorkshop.featured;
            if (featured.status != 0 && featured.type == EFeaturedWorkshopType.Curated && (!ConvenientSavedata.get().read("SingleplayerCuratedSeenId", out long value) || value < featured.id))
            {
                curatedStatusLabel = new SleekNew(featured.status == EMapStatus.Updated);
                curatedMapsButton.AddChild(curatedStatusLabel);
                hasCreatedFeaturedMapLabel = true;
            }
        }
    }

    public void OnDestroy()
    {
        Level.onLevelsRefreshed = (LevelsRefreshed)Delegate.Remove(Level.onLevelsRefreshed, new LevelsRefreshed(onLevelsRefreshed));
        LiveConfig.OnRefreshed -= OnLiveConfigRefreshed;
    }

    public MenuPlaySingleplayerUI()
    {
        localization = Localization.read("/Menu/Play/MenuPlaySingleplayer.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Play/MenuPlaySingleplayer/MenuPlaySingleplayer.unity3d");
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
        levelScrollBox.positionOffset_Y = 340;
        levelScrollBox.positionScale_X = 0.5f;
        levelScrollBox.sizeOffset_X = 430;
        levelScrollBox.sizeOffset_Y = -440;
        levelScrollBox.sizeScale_Y = 1f;
        levelScrollBox.scaleContentToWidth = true;
        container.AddChild(levelScrollBox);
        officalMapsButton = Glazier.Get().CreateButton();
        officalMapsButton.positionOffset_X = -95;
        officalMapsButton.positionOffset_Y = 290;
        officalMapsButton.positionScale_X = 0.5f;
        officalMapsButton.sizeOffset_X = 100;
        officalMapsButton.sizeOffset_Y = 50;
        officalMapsButton.text = localization.format("Maps_Official");
        officalMapsButton.tooltipText = localization.format("Maps_Official_Tooltip");
        officalMapsButton.onClickedButton += onClickedOfficialMapsButton;
        officalMapsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(officalMapsButton);
        curatedMapsButton = Glazier.Get().CreateButton();
        curatedMapsButton.positionOffset_X = 5;
        curatedMapsButton.positionOffset_Y = 290;
        curatedMapsButton.positionScale_X = 0.5f;
        curatedMapsButton.sizeOffset_X = 100;
        curatedMapsButton.sizeOffset_Y = 50;
        curatedMapsButton.text = localization.format("Maps_Curated");
        curatedMapsButton.tooltipText = localization.format("Maps_Curated_Tooltip");
        curatedMapsButton.onClickedButton += onClickedCuratedMapsButton;
        curatedMapsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(curatedMapsButton);
        hasCreatedFeaturedMapLabel = false;
        LiveConfig.OnRefreshed -= OnLiveConfigRefreshed;
        OnLiveConfigRefreshed();
        workshopMapsButton = Glazier.Get().CreateButton();
        workshopMapsButton.positionOffset_X = 105;
        workshopMapsButton.positionOffset_Y = 290;
        workshopMapsButton.positionScale_X = 0.5f;
        workshopMapsButton.sizeOffset_X = 100;
        workshopMapsButton.sizeOffset_Y = 50;
        workshopMapsButton.text = localization.format("Maps_Workshop");
        workshopMapsButton.tooltipText = localization.format("Maps_Workshop_Tooltip");
        workshopMapsButton.onClickedButton += onClickedWorkshopMapsButton;
        workshopMapsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(workshopMapsButton);
        miscMapsButton = Glazier.Get().CreateButton();
        miscMapsButton.positionOffset_X = 205;
        miscMapsButton.positionOffset_Y = 290;
        miscMapsButton.positionScale_X = 0.5f;
        miscMapsButton.sizeOffset_X = 100;
        miscMapsButton.sizeOffset_Y = 50;
        miscMapsButton.text = localization.format("Maps_Misc");
        miscMapsButton.tooltipText = localization.format("Maps_Misc_Tooltip");
        miscMapsButton.onClickedButton += onClickedMiscMapsButton;
        miscMapsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(miscMapsButton);
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
        creditsBox = Glazier.Get().CreateBox();
        creditsBox.positionOffset_X = 345;
        creditsBox.positionOffset_Y = 100;
        creditsBox.positionScale_X = 0.5f;
        creditsBox.sizeOffset_X = 250;
        container.AddChild(creditsBox);
        creditsBox.isVisible = false;
        itemButton = Glazier.Get().CreateButton();
        itemButton.enableRichText = true;
        itemButton.positionOffset_X = 345;
        itemButton.positionOffset_Y = 100;
        itemButton.positionScale_X = 0.5f;
        itemButton.sizeOffset_X = 250;
        itemButton.sizeOffset_Y = 100;
        itemButton.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        itemButton.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        itemButton.onClickedButton += onClickedItemButton;
        container.AddChild(itemButton);
        itemButton.isVisible = false;
        feedbackButton = Glazier.Get().CreateButton();
        feedbackButton.positionOffset_X = 345;
        feedbackButton.positionOffset_Y = 100;
        feedbackButton.positionScale_X = 0.5f;
        feedbackButton.sizeOffset_X = 250;
        feedbackButton.sizeOffset_Y = 30;
        feedbackButton.text = localization.format("Feedback_Button");
        feedbackButton.tooltipText = localization.format("Feedback_Button_Tooltip");
        feedbackButton.onClickedButton += onClickedFeedbackButton;
        container.AddChild(feedbackButton);
        feedbackButton.isVisible = false;
        newsButton = Glazier.Get().CreateButton();
        newsButton.positionOffset_X = 345;
        newsButton.positionOffset_Y = 100;
        newsButton.positionScale_X = 0.5f;
        newsButton.sizeOffset_X = 250;
        newsButton.sizeOffset_Y = 30;
        newsButton.onClickedButton += onClickedNewsButton;
        container.AddChild(newsButton);
        newsButton.isVisible = false;
        playButton = new SleekButtonIcon(bundle.load<Texture2D>("Play"));
        playButton.positionOffset_X = -305;
        playButton.positionOffset_Y = 290;
        playButton.positionScale_X = 0.5f;
        playButton.sizeOffset_X = 200;
        playButton.sizeOffset_Y = 30;
        playButton.text = localization.format("Play_Button");
        playButton.tooltip = localization.format("Play_Button_Tooltip");
        playButton.iconColor = ESleekTint.FOREGROUND;
        playButton.onClickedButton += onClickedPlayButton;
        container.AddChild(playButton);
        browseServersButton = Glazier.Get().CreateButton();
        browseServersButton.positionOffset_X = -305;
        browseServersButton.positionOffset_Y = 420;
        browseServersButton.positionScale_X = 0.5f;
        browseServersButton.sizeOffset_X = 200;
        browseServersButton.sizeOffset_Y = 30;
        browseServersButton.text = localization.format("Browse_Servers_Label");
        browseServersButton.tooltipText = localization.format("Browse_Servers_Tooltip");
        browseServersButton.onClickedButton += onClickedBrowseServersButton;
        container.AddChild(browseServersButton);
        modeButtonState = new SleekButtonState(new GUIContent(localization.format("Easy_Button"), bundle.load<Texture>("Easy")), new GUIContent(localization.format("Normal_Button"), bundle.load<Texture>("Normal")), new GUIContent(localization.format("Hard_Button"), bundle.load<Texture>("Hard")));
        modeButtonState.positionOffset_X = -305;
        modeButtonState.positionOffset_Y = 330;
        modeButtonState.positionScale_X = 0.5f;
        modeButtonState.sizeOffset_X = 105;
        modeButtonState.sizeOffset_Y = 30;
        modeButtonState.state = (int)PlaySettings.singleplayerMode;
        modeButtonState.onSwappedState = onSwappedModeState;
        container.AddChild(modeButtonState);
        configButton = Glazier.Get().CreateButton();
        configButton.positionOffset_X = -195;
        configButton.positionOffset_Y = 330;
        configButton.positionScale_X = 0.5f;
        configButton.sizeOffset_X = 85;
        configButton.sizeOffset_Y = 30;
        configButton.text = localization.format("Config_Button");
        configButton.tooltipText = localization.format("Config_Button_Tooltip");
        configButton.onClickedButton += onClickedConfigButton;
        container.AddChild(configButton);
        cheatsToggle = Glazier.Get().CreateToggle();
        cheatsToggle.positionOffset_X = -305;
        cheatsToggle.positionOffset_Y = 370;
        cheatsToggle.positionScale_X = 0.5f;
        cheatsToggle.sizeOffset_X = 40;
        cheatsToggle.sizeOffset_Y = 40;
        cheatsToggle.addLabel(localization.format("Cheats_Label"), ESleekSide.RIGHT);
        cheatsToggle.state = PlaySettings.singleplayerCheats;
        cheatsToggle.onToggled += onToggledCheatsToggle;
        container.AddChild(cheatsToggle);
        resetButton = new SleekButtonIconConfirm(null, localization.format("Reset_Button_Confirm"), localization.format("Reset_Button_Confirm_Tooltip"), localization.format("Reset_Button_Deny"), localization.format("Reset_Button_Deny_Tooltip"));
        resetButton.positionOffset_X = -305;
        resetButton.positionOffset_Y = 480;
        resetButton.positionScale_X = 0.5f;
        resetButton.sizeOffset_X = 200;
        resetButton.sizeOffset_Y = 30;
        resetButton.text = localization.format("Reset_Button");
        resetButton.tooltip = localization.format("Reset_Button_Tooltip");
        resetButton.onConfirmed = onClickedResetButton;
        container.AddChild(resetButton);
        bundle.unload();
        refreshLevels();
        Level.onLevelsRefreshed = (LevelsRefreshed)Delegate.Combine(Level.onLevelsRefreshed, new LevelsRefreshed(onLevelsRefreshed));
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
        new MenuPlayConfigUI();
    }
}
