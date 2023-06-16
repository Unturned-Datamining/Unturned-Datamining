using System;
using UnityEngine;

namespace SDG.Unturned;

public class MenuWorkshopUI
{
    private static SleekFullscreenBox container;

    private static Local localization;

    public static bool active;

    private static SleekButtonIcon browseButton;

    private static SleekButtonIcon submitButton;

    private static SleekButtonIcon editorButton;

    private static SleekButtonIcon errorButton;

    private static SleekButtonIcon localizationButton;

    private static SleekButtonIcon spawnsButton;

    private static SleekButtonIcon subscriptionsButton;

    private static SleekButtonIcon docsButton;

    private static SleekButtonIcon backButton;

    private static ISleekElement iconToolsContainer;

    private static ISleekUInt16Field itemIDField;

    private static ISleekUInt16Field vehicleIDField;

    private static ISleekUInt16Field skinIDField;

    private static ISleekField guidField;

    private static ISleekButton captureItemIconButton;

    private static ISleekButton captureAllItemIconsButton;

    private static ISleekButton captureItemDefIconButton;

    private static ISleekButton captureOutfitPreviewButton;

    private static ISleekButton captureCosmeticPreviewsButton;

    private static ISleekButton captureAllOutfitPreviewsButton;

    private MenuWorkshopSubmitUI submitUI;

    private MenuWorkshopEditorUI editorUI;

    private MenuWorkshopErrorUI errorUI;

    private MenuWorkshopLocalizationUI localizationUI;

    private MenuWorkshopSpawnsUI spawnsUI;

    private MenuWorkshopSubscriptionsUI subscriptionsUI;

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
            container.AnimateOutOfView(0f, -1f);
        }
    }

    private static void onClickedBrowseButton(ISleekElement button)
    {
        if (!Provider.provider.browserService.canOpenBrowser)
        {
            MenuUI.alert(localization.format("Overlay"));
        }
        else
        {
            Provider.provider.browserService.open("http://steamcommunity.com/app/304930/workshop/");
        }
    }

    private static void onClickedSubmitButton(ISleekElement button)
    {
        MenuWorkshopSubmitUI.open();
        close();
    }

    private static void onClickedEditorButton(ISleekElement button)
    {
        MenuWorkshopEditorUI.open();
        close();
    }

    private static void onClickedErrorButton(ISleekElement button)
    {
        MenuWorkshopErrorUI.open();
        close();
    }

    private static void onClickedLocalizationButton(ISleekElement button)
    {
        MenuWorkshopLocalizationUI.open();
        close();
    }

    private static void onClickedSpawnsButton(ISleekElement button)
    {
        MenuWorkshopSpawnsUI.open();
        close();
    }

    private static void onClickedSubscriptionsButton(ISleekElement button)
    {
        MenuWorkshopSubscriptionsUI.instance.open();
        close();
    }

    private static void onClickedDocsButton(ISleekElement button)
    {
        Provider.provider.browserService.open("https://docs.smartlydressedgames.com");
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuDashboardUI.open();
        MenuTitleUI.open();
        close();
    }

    private static void onClickedCaptureItemIconButton(ISleekElement button)
    {
        IconUtils.CreateExtrasDirectory();
        IconUtils.captureItemIcon(Assets.find(EAssetType.ITEM, itemIDField.state) as ItemAsset);
    }

    private static void onClickedCaptureAllItemIconsButton(ISleekElement button)
    {
        IconUtils.CreateExtrasDirectory();
        IconUtils.captureAllItemIcons();
    }

    private static void onClickedCaptureItemDefIconButton(ISleekElement button)
    {
        IconUtils.CreateExtrasDirectory();
        if (Guid.TryParse(guidField.text, out var result))
        {
            Asset asset = Assets.find(result);
            ItemAsset itemAsset = asset as ItemAsset;
            VehicleAsset vehicleAsset = asset as VehicleAsset;
            if (itemAsset != null || vehicleAsset != null)
            {
                IconUtils.getItemDefIcon(itemAsset, vehicleAsset, skinIDField.state);
                return;
            }
        }
        IconUtils.getItemDefIcon(itemIDField.state, vehicleIDField.state, skinIDField.state);
    }

    private static void OnCaptureOutfitPreviewClicked(ISleekElement button)
    {
        IconUtils.CreateExtrasDirectory();
        IconUtils.CaptureOutfitPreview(new Guid(guidField.text));
    }

    private static void OnCaptureCosmeticPreviewsClicked(ISleekElement button)
    {
        IconUtils.CreateExtrasDirectory();
        IconUtils.CaptureCosmeticPreviews();
    }

    private static void OnCaptureAllOutfitPreviewsClicked(ISleekElement button)
    {
        IconUtils.CreateExtrasDirectory();
        IconUtils.CaptureAllOutfitPreviews();
    }

    public static void toggleIconTools()
    {
        iconToolsContainer.isVisible = !iconToolsContainer.isVisible;
    }

    public void OnDestroy()
    {
        editorUI.OnDestroy();
        submitUI.OnDestroy();
    }

    public MenuWorkshopUI()
    {
        localization = Localization.read("/Menu/Workshop/MenuWorkshop.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Workshop/MenuWorkshop/MenuWorkshop.unity3d");
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_Y = -1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        browseButton = new SleekButtonIcon(bundle.load<Texture2D>("Browse"));
        browseButton.positionOffset_X = -205;
        browseButton.positionOffset_Y = -115;
        browseButton.positionScale_X = 0.5f;
        browseButton.positionScale_Y = 0.5f;
        browseButton.sizeOffset_X = 200;
        browseButton.sizeOffset_Y = 50;
        browseButton.text = localization.format("BrowseButtonText");
        browseButton.tooltip = localization.format("BrowseButtonTooltip");
        browseButton.onClickedButton += onClickedBrowseButton;
        browseButton.fontSize = ESleekFontSize.Medium;
        browseButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(browseButton);
        submitButton = new SleekButtonIcon(bundle.load<Texture2D>("Submit"));
        submitButton.positionOffset_X = -205;
        submitButton.positionOffset_Y = -55;
        submitButton.positionScale_X = 0.5f;
        submitButton.positionScale_Y = 0.5f;
        submitButton.sizeOffset_X = 200;
        submitButton.sizeOffset_Y = 50;
        submitButton.text = localization.format("SubmitButtonText");
        submitButton.tooltip = localization.format("SubmitButtonTooltip");
        submitButton.onClickedButton += onClickedSubmitButton;
        submitButton.fontSize = ESleekFontSize.Medium;
        submitButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(submitButton);
        editorButton = new SleekButtonIcon(bundle.load<Texture2D>("Editor"));
        editorButton.positionOffset_X = 5;
        editorButton.positionOffset_Y = -55;
        editorButton.positionScale_X = 0.5f;
        editorButton.positionScale_Y = 0.5f;
        editorButton.sizeOffset_X = 200;
        editorButton.sizeOffset_Y = 50;
        editorButton.text = localization.format("EditorButtonText");
        editorButton.tooltip = localization.format("EditorButtonTooltip");
        editorButton.onClickedButton += onClickedEditorButton;
        editorButton.fontSize = ESleekFontSize.Medium;
        editorButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(editorButton);
        errorButton = new SleekButtonIcon(bundle.load<Texture2D>("Error"));
        errorButton.positionOffset_X = -205;
        errorButton.positionOffset_Y = 5;
        errorButton.positionScale_X = 0.5f;
        errorButton.positionScale_Y = 0.5f;
        errorButton.sizeOffset_X = 200;
        errorButton.sizeOffset_Y = 50;
        errorButton.text = localization.format("ErrorButtonText");
        errorButton.tooltip = localization.format("ErrorButtonTooltip");
        errorButton.onClickedButton += onClickedErrorButton;
        errorButton.fontSize = ESleekFontSize.Medium;
        errorButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(errorButton);
        localizationButton = new SleekButtonIcon(bundle.load<Texture2D>("Localization"));
        localizationButton.positionOffset_X = 5;
        localizationButton.positionOffset_Y = 65;
        localizationButton.positionScale_X = 0.5f;
        localizationButton.positionScale_Y = 0.5f;
        localizationButton.sizeOffset_X = 200;
        localizationButton.sizeOffset_Y = 50;
        localizationButton.text = localization.format("LocalizationButtonText");
        localizationButton.tooltip = localization.format("LocalizationButtonTooltip");
        localizationButton.onClickedButton += onClickedLocalizationButton;
        localizationButton.fontSize = ESleekFontSize.Medium;
        localizationButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(localizationButton);
        spawnsButton = new SleekButtonIcon(bundle.load<Texture2D>("Spawns"));
        spawnsButton.positionOffset_X = -205;
        spawnsButton.positionOffset_Y = 65;
        spawnsButton.positionScale_X = 0.5f;
        spawnsButton.positionScale_Y = 0.5f;
        spawnsButton.sizeOffset_X = 200;
        spawnsButton.sizeOffset_Y = 50;
        spawnsButton.text = localization.format("SpawnsButtonText");
        spawnsButton.tooltip = localization.format("SpawnsButtonTooltip");
        spawnsButton.onClickedButton += onClickedSpawnsButton;
        spawnsButton.fontSize = ESleekFontSize.Medium;
        spawnsButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(spawnsButton);
        subscriptionsButton = new SleekButtonIcon(bundle.load<Texture2D>("Subscriptions"));
        subscriptionsButton.positionOffset_X = 5;
        subscriptionsButton.positionOffset_Y = -115;
        subscriptionsButton.positionScale_X = 0.5f;
        subscriptionsButton.positionScale_Y = 0.5f;
        subscriptionsButton.sizeOffset_X = 200;
        subscriptionsButton.sizeOffset_Y = 50;
        subscriptionsButton.text = localization.format("SubscriptionsButtonText");
        subscriptionsButton.tooltip = localization.format("SubscriptionsButtonTooltip");
        subscriptionsButton.onClickedButton += onClickedSubscriptionsButton;
        subscriptionsButton.fontSize = ESleekFontSize.Medium;
        subscriptionsButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(subscriptionsButton);
        docsButton = new SleekButtonIcon(bundle.load<Texture2D>("Docs"));
        docsButton.positionOffset_X = 5;
        docsButton.positionOffset_Y = 5;
        docsButton.positionScale_X = 0.5f;
        docsButton.positionScale_Y = 0.5f;
        docsButton.sizeOffset_X = 200;
        docsButton.sizeOffset_Y = 50;
        docsButton.text = localization.format("DocsButtonText");
        docsButton.tooltip = localization.format("DocsButtonTooltip");
        docsButton.onClickedButton += onClickedDocsButton;
        docsButton.fontSize = ESleekFontSize.Medium;
        docsButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(docsButton);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.positionOffset_X = -100;
        backButton.positionOffset_Y = 125;
        backButton.positionScale_X = 0.5f;
        backButton.positionScale_Y = 0.5f;
        backButton.sizeOffset_X = 200;
        backButton.sizeOffset_Y = 50;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
        bundle.unload();
        iconToolsContainer = Glazier.Get().CreateFrame();
        iconToolsContainer.positionOffset_X = 40;
        iconToolsContainer.positionOffset_Y = 40;
        iconToolsContainer.sizeOffset_X = -80;
        iconToolsContainer.sizeOffset_Y = -80;
        iconToolsContainer.sizeScale_X = 1f;
        iconToolsContainer.sizeScale_Y = 1f;
        container.AddChild(iconToolsContainer);
        iconToolsContainer.isVisible = false;
        itemIDField = Glazier.Get().CreateUInt16Field();
        itemIDField.sizeOffset_X = 150;
        itemIDField.sizeOffset_Y = 25;
        itemIDField.addLabel("Item ID", ESleekSide.RIGHT);
        iconToolsContainer.AddChild(itemIDField);
        vehicleIDField = Glazier.Get().CreateUInt16Field();
        vehicleIDField.positionOffset_Y = 25;
        vehicleIDField.sizeOffset_X = 150;
        vehicleIDField.sizeOffset_Y = 25;
        vehicleIDField.addLabel("Vehicle ID", ESleekSide.RIGHT);
        iconToolsContainer.AddChild(vehicleIDField);
        skinIDField = Glazier.Get().CreateUInt16Field();
        skinIDField.positionOffset_Y = 50;
        skinIDField.sizeOffset_X = 150;
        skinIDField.sizeOffset_Y = 25;
        skinIDField.addLabel("Skin ID", ESleekSide.RIGHT);
        iconToolsContainer.AddChild(skinIDField);
        captureItemIconButton = Glazier.Get().CreateButton();
        captureItemIconButton.positionOffset_Y = 75;
        captureItemIconButton.sizeOffset_X = 150;
        captureItemIconButton.sizeOffset_Y = 25;
        captureItemIconButton.text = "Item Icon";
        captureItemIconButton.onClickedButton += onClickedCaptureItemIconButton;
        iconToolsContainer.AddChild(captureItemIconButton);
        captureAllItemIconsButton = Glazier.Get().CreateButton();
        captureAllItemIconsButton.positionOffset_Y = 100;
        captureAllItemIconsButton.sizeOffset_X = 150;
        captureAllItemIconsButton.sizeOffset_Y = 25;
        captureAllItemIconsButton.text = "All Item Icons";
        captureAllItemIconsButton.onClickedButton += onClickedCaptureAllItemIconsButton;
        iconToolsContainer.AddChild(captureAllItemIconsButton);
        captureItemDefIconButton = Glazier.Get().CreateButton();
        captureItemDefIconButton.positionOffset_Y = 125;
        captureItemDefIconButton.sizeOffset_X = 150;
        captureItemDefIconButton.sizeOffset_Y = 25;
        captureItemDefIconButton.text = "Econ Icon";
        captureItemDefIconButton.onClickedButton += onClickedCaptureItemDefIconButton;
        iconToolsContainer.AddChild(captureItemDefIconButton);
        guidField = Glazier.Get().CreateStringField();
        guidField.positionOffset_Y = 150;
        guidField.sizeOffset_X = 150;
        guidField.sizeOffset_Y = 25;
        guidField.addLabel("GUID", ESleekSide.RIGHT);
        iconToolsContainer.AddChild(guidField);
        captureOutfitPreviewButton = Glazier.Get().CreateButton();
        captureOutfitPreviewButton.positionOffset_Y = 175;
        captureOutfitPreviewButton.sizeOffset_X = 150;
        captureOutfitPreviewButton.sizeOffset_Y = 25;
        captureOutfitPreviewButton.text = "Outfit Preview";
        captureOutfitPreviewButton.onClickedButton += OnCaptureOutfitPreviewClicked;
        iconToolsContainer.AddChild(captureOutfitPreviewButton);
        captureCosmeticPreviewsButton = Glazier.Get().CreateButton();
        captureCosmeticPreviewsButton.positionOffset_Y = 200;
        captureCosmeticPreviewsButton.sizeOffset_X = 150;
        captureCosmeticPreviewsButton.sizeOffset_Y = 25;
        captureCosmeticPreviewsButton.text = "All Cosmetic Previews";
        captureCosmeticPreviewsButton.onClickedButton += OnCaptureCosmeticPreviewsClicked;
        iconToolsContainer.AddChild(captureCosmeticPreviewsButton);
        captureAllOutfitPreviewsButton = Glazier.Get().CreateButton();
        captureAllOutfitPreviewsButton.positionOffset_Y = 225;
        captureAllOutfitPreviewsButton.sizeOffset_X = 150;
        captureAllOutfitPreviewsButton.sizeOffset_Y = 25;
        captureAllOutfitPreviewsButton.text = "All Outfit Previews";
        captureAllOutfitPreviewsButton.onClickedButton += OnCaptureAllOutfitPreviewsClicked;
        iconToolsContainer.AddChild(captureAllOutfitPreviewsButton);
        submitUI = new MenuWorkshopSubmitUI();
        editorUI = new MenuWorkshopEditorUI();
        errorUI = new MenuWorkshopErrorUI();
        localizationUI = new MenuWorkshopLocalizationUI();
        spawnsUI = new MenuWorkshopSpawnsUI();
        subscriptionsUI = new MenuWorkshopSubscriptionsUI();
    }
}
