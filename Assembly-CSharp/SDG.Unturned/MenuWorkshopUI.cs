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
        IconUtils.captureItemIcon(Assets.find(EAssetType.ITEM, itemIDField.Value) as ItemAsset);
    }

    private static void onClickedCaptureAllItemIconsButton(ISleekElement button)
    {
        IconUtils.CreateExtrasDirectory();
        IconUtils.captureAllItemIcons();
    }

    private static void onClickedCaptureItemDefIconButton(ISleekElement button)
    {
        IconUtils.CreateExtrasDirectory();
        if (Guid.TryParse(guidField.Text, out var result))
        {
            Asset asset = Assets.find(result);
            ItemAsset itemAsset = asset as ItemAsset;
            VehicleAsset vehicleAsset = asset as VehicleAsset;
            if (itemAsset != null || vehicleAsset != null)
            {
                IconUtils.getItemDefIcon(itemAsset, vehicleAsset, skinIDField.Value);
                return;
            }
        }
        IconUtils.getItemDefIcon(itemIDField.Value, vehicleIDField.Value, skinIDField.Value);
    }

    private static void OnCaptureOutfitPreviewClicked(ISleekElement button)
    {
        IconUtils.CreateExtrasDirectory();
        IconUtils.CaptureOutfitPreview(new Guid(guidField.Text));
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
        iconToolsContainer.IsVisible = !iconToolsContainer.IsVisible;
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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = -1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        browseButton = new SleekButtonIcon(bundle.load<Texture2D>("Browse"));
        browseButton.PositionOffset_X = -205f;
        browseButton.PositionOffset_Y = -115f;
        browseButton.PositionScale_X = 0.5f;
        browseButton.PositionScale_Y = 0.5f;
        browseButton.SizeOffset_X = 200f;
        browseButton.SizeOffset_Y = 50f;
        browseButton.text = localization.format("BrowseButtonText");
        browseButton.tooltip = localization.format("BrowseButtonTooltip");
        browseButton.onClickedButton += onClickedBrowseButton;
        browseButton.fontSize = ESleekFontSize.Medium;
        browseButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(browseButton);
        submitButton = new SleekButtonIcon(bundle.load<Texture2D>("Submit"));
        submitButton.PositionOffset_X = -205f;
        submitButton.PositionOffset_Y = -55f;
        submitButton.PositionScale_X = 0.5f;
        submitButton.PositionScale_Y = 0.5f;
        submitButton.SizeOffset_X = 200f;
        submitButton.SizeOffset_Y = 50f;
        submitButton.text = localization.format("SubmitButtonText");
        submitButton.tooltip = localization.format("SubmitButtonTooltip");
        submitButton.onClickedButton += onClickedSubmitButton;
        submitButton.fontSize = ESleekFontSize.Medium;
        submitButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(submitButton);
        editorButton = new SleekButtonIcon(bundle.load<Texture2D>("Editor"));
        editorButton.PositionOffset_X = 5f;
        editorButton.PositionOffset_Y = -55f;
        editorButton.PositionScale_X = 0.5f;
        editorButton.PositionScale_Y = 0.5f;
        editorButton.SizeOffset_X = 200f;
        editorButton.SizeOffset_Y = 50f;
        editorButton.text = localization.format("EditorButtonText");
        editorButton.tooltip = localization.format("EditorButtonTooltip");
        editorButton.onClickedButton += onClickedEditorButton;
        editorButton.fontSize = ESleekFontSize.Medium;
        editorButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(editorButton);
        errorButton = new SleekButtonIcon(bundle.load<Texture2D>("Error"));
        errorButton.PositionOffset_X = -205f;
        errorButton.PositionOffset_Y = 5f;
        errorButton.PositionScale_X = 0.5f;
        errorButton.PositionScale_Y = 0.5f;
        errorButton.SizeOffset_X = 200f;
        errorButton.SizeOffset_Y = 50f;
        errorButton.text = localization.format("ErrorButtonText");
        errorButton.tooltip = localization.format("ErrorButtonTooltip");
        errorButton.onClickedButton += onClickedErrorButton;
        errorButton.fontSize = ESleekFontSize.Medium;
        errorButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(errorButton);
        localizationButton = new SleekButtonIcon(bundle.load<Texture2D>("Localization"));
        localizationButton.PositionOffset_X = 5f;
        localizationButton.PositionOffset_Y = 65f;
        localizationButton.PositionScale_X = 0.5f;
        localizationButton.PositionScale_Y = 0.5f;
        localizationButton.SizeOffset_X = 200f;
        localizationButton.SizeOffset_Y = 50f;
        localizationButton.text = localization.format("LocalizationButtonText");
        localizationButton.tooltip = localization.format("LocalizationButtonTooltip");
        localizationButton.onClickedButton += onClickedLocalizationButton;
        localizationButton.fontSize = ESleekFontSize.Medium;
        localizationButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(localizationButton);
        spawnsButton = new SleekButtonIcon(bundle.load<Texture2D>("Spawns"));
        spawnsButton.PositionOffset_X = -205f;
        spawnsButton.PositionOffset_Y = 65f;
        spawnsButton.PositionScale_X = 0.5f;
        spawnsButton.PositionScale_Y = 0.5f;
        spawnsButton.SizeOffset_X = 200f;
        spawnsButton.SizeOffset_Y = 50f;
        spawnsButton.text = localization.format("SpawnsButtonText");
        spawnsButton.tooltip = localization.format("SpawnsButtonTooltip");
        spawnsButton.onClickedButton += onClickedSpawnsButton;
        spawnsButton.fontSize = ESleekFontSize.Medium;
        spawnsButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(spawnsButton);
        subscriptionsButton = new SleekButtonIcon(bundle.load<Texture2D>("Subscriptions"));
        subscriptionsButton.PositionOffset_X = 5f;
        subscriptionsButton.PositionOffset_Y = -115f;
        subscriptionsButton.PositionScale_X = 0.5f;
        subscriptionsButton.PositionScale_Y = 0.5f;
        subscriptionsButton.SizeOffset_X = 200f;
        subscriptionsButton.SizeOffset_Y = 50f;
        subscriptionsButton.text = localization.format("SubscriptionsButtonText");
        subscriptionsButton.tooltip = localization.format("SubscriptionsButtonTooltip");
        subscriptionsButton.onClickedButton += onClickedSubscriptionsButton;
        subscriptionsButton.fontSize = ESleekFontSize.Medium;
        subscriptionsButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(subscriptionsButton);
        docsButton = new SleekButtonIcon(bundle.load<Texture2D>("Docs"));
        docsButton.PositionOffset_X = 5f;
        docsButton.PositionOffset_Y = 5f;
        docsButton.PositionScale_X = 0.5f;
        docsButton.PositionScale_Y = 0.5f;
        docsButton.SizeOffset_X = 200f;
        docsButton.SizeOffset_Y = 50f;
        docsButton.text = localization.format("DocsButtonText");
        docsButton.tooltip = localization.format("DocsButtonTooltip");
        docsButton.onClickedButton += onClickedDocsButton;
        docsButton.fontSize = ESleekFontSize.Medium;
        docsButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(docsButton);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.PositionOffset_X = -100f;
        backButton.PositionOffset_Y = 125f;
        backButton.PositionScale_X = 0.5f;
        backButton.PositionScale_Y = 0.5f;
        backButton.SizeOffset_X = 200f;
        backButton.SizeOffset_Y = 50f;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
        bundle.unload();
        iconToolsContainer = Glazier.Get().CreateFrame();
        iconToolsContainer.PositionOffset_X = 40f;
        iconToolsContainer.PositionOffset_Y = 40f;
        iconToolsContainer.SizeOffset_X = -80f;
        iconToolsContainer.SizeOffset_Y = -80f;
        iconToolsContainer.SizeScale_X = 1f;
        iconToolsContainer.SizeScale_Y = 1f;
        container.AddChild(iconToolsContainer);
        iconToolsContainer.IsVisible = false;
        itemIDField = Glazier.Get().CreateUInt16Field();
        itemIDField.SizeOffset_X = 150f;
        itemIDField.SizeOffset_Y = 25f;
        itemIDField.AddLabel("Item ID", ESleekSide.RIGHT);
        iconToolsContainer.AddChild(itemIDField);
        vehicleIDField = Glazier.Get().CreateUInt16Field();
        vehicleIDField.PositionOffset_Y = 25f;
        vehicleIDField.SizeOffset_X = 150f;
        vehicleIDField.SizeOffset_Y = 25f;
        vehicleIDField.AddLabel("Vehicle ID", ESleekSide.RIGHT);
        iconToolsContainer.AddChild(vehicleIDField);
        skinIDField = Glazier.Get().CreateUInt16Field();
        skinIDField.PositionOffset_Y = 50f;
        skinIDField.SizeOffset_X = 150f;
        skinIDField.SizeOffset_Y = 25f;
        skinIDField.AddLabel("Skin ID", ESleekSide.RIGHT);
        iconToolsContainer.AddChild(skinIDField);
        captureItemIconButton = Glazier.Get().CreateButton();
        captureItemIconButton.PositionOffset_Y = 75f;
        captureItemIconButton.SizeOffset_X = 150f;
        captureItemIconButton.SizeOffset_Y = 25f;
        captureItemIconButton.Text = "Item Icon";
        captureItemIconButton.OnClicked += onClickedCaptureItemIconButton;
        iconToolsContainer.AddChild(captureItemIconButton);
        captureAllItemIconsButton = Glazier.Get().CreateButton();
        captureAllItemIconsButton.PositionOffset_Y = 100f;
        captureAllItemIconsButton.SizeOffset_X = 150f;
        captureAllItemIconsButton.SizeOffset_Y = 25f;
        captureAllItemIconsButton.Text = "All Item Icons";
        captureAllItemIconsButton.OnClicked += onClickedCaptureAllItemIconsButton;
        iconToolsContainer.AddChild(captureAllItemIconsButton);
        captureItemDefIconButton = Glazier.Get().CreateButton();
        captureItemDefIconButton.PositionOffset_Y = 125f;
        captureItemDefIconButton.SizeOffset_X = 150f;
        captureItemDefIconButton.SizeOffset_Y = 25f;
        captureItemDefIconButton.Text = "Econ Icon";
        captureItemDefIconButton.OnClicked += onClickedCaptureItemDefIconButton;
        iconToolsContainer.AddChild(captureItemDefIconButton);
        guidField = Glazier.Get().CreateStringField();
        guidField.PositionOffset_Y = 150f;
        guidField.SizeOffset_X = 150f;
        guidField.SizeOffset_Y = 25f;
        guidField.AddLabel("GUID", ESleekSide.RIGHT);
        iconToolsContainer.AddChild(guidField);
        captureOutfitPreviewButton = Glazier.Get().CreateButton();
        captureOutfitPreviewButton.PositionOffset_Y = 175f;
        captureOutfitPreviewButton.SizeOffset_X = 150f;
        captureOutfitPreviewButton.SizeOffset_Y = 25f;
        captureOutfitPreviewButton.Text = "Outfit Preview";
        captureOutfitPreviewButton.OnClicked += OnCaptureOutfitPreviewClicked;
        iconToolsContainer.AddChild(captureOutfitPreviewButton);
        captureCosmeticPreviewsButton = Glazier.Get().CreateButton();
        captureCosmeticPreviewsButton.PositionOffset_Y = 200f;
        captureCosmeticPreviewsButton.SizeOffset_X = 150f;
        captureCosmeticPreviewsButton.SizeOffset_Y = 25f;
        captureCosmeticPreviewsButton.Text = "All Cosmetic Previews";
        captureCosmeticPreviewsButton.OnClicked += OnCaptureCosmeticPreviewsClicked;
        iconToolsContainer.AddChild(captureCosmeticPreviewsButton);
        captureAllOutfitPreviewsButton = Glazier.Get().CreateButton();
        captureAllOutfitPreviewsButton.PositionOffset_Y = 225f;
        captureAllOutfitPreviewsButton.SizeOffset_X = 150f;
        captureAllOutfitPreviewsButton.SizeOffset_Y = 25f;
        captureAllOutfitPreviewsButton.Text = "All Outfit Previews";
        captureAllOutfitPreviewsButton.OnClicked += OnCaptureAllOutfitPreviewsClicked;
        iconToolsContainer.AddChild(captureAllOutfitPreviewsButton);
        submitUI = new MenuWorkshopSubmitUI();
        editorUI = new MenuWorkshopEditorUI();
        errorUI = new MenuWorkshopErrorUI();
        localizationUI = new MenuWorkshopLocalizationUI();
        spawnsUI = new MenuWorkshopSpawnsUI();
        subscriptionsUI = new MenuWorkshopSubscriptionsUI();
    }
}
