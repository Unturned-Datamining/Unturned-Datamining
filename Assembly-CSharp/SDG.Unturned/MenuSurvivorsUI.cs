using UnityEngine;

namespace SDG.Unturned;

public class MenuSurvivorsUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon characterButton;

    private static SleekButtonIcon appearanceButton;

    private static SleekButtonIcon groupButton;

    private static SleekButtonIcon clothingButton;

    private static SleekButtonIcon backButton;

    private MenuSurvivorsCharacterUI characterUI;

    private MenuSurvivorsAppearanceUI appearanceUI;

    private MenuSurvivorsGroupUI groupUI;

    private MenuSurvivorsClothingUI clothingUI;

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
            Characters.save();
            container.AnimateOutOfView(0f, -1f);
        }
    }

    private static void onClickedCharacterButton(ISleekElement button)
    {
        MenuSurvivorsCharacterUI.open();
        close();
    }

    private static void onClickedAppearanceButton(ISleekElement button)
    {
        MenuSurvivorsAppearanceUI.open();
        close();
    }

    private static void onClickedGroupButton(ISleekElement button)
    {
        MenuSurvivorsGroupUI.open();
        close();
    }

    private static void onClickedClothingButton(ISleekElement button)
    {
        MenuSurvivorsClothingUI.open();
        close();
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuDashboardUI.open();
        MenuTitleUI.open();
        close();
    }

    public void OnDestroy()
    {
        characterUI.OnDestroy();
        appearanceUI.OnDestroy();
        groupUI.OnDestroy();
        clothingUI.OnDestroy();
    }

    public MenuSurvivorsUI()
    {
        Local local = Localization.read("/Menu/Survivors/MenuSurvivors.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Survivors/MenuSurvivors/MenuSurvivors.unity3d");
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
        characterButton = new SleekButtonIcon(bundle.load<Texture2D>("Character"));
        characterButton.positionOffset_X = -100;
        characterButton.positionOffset_Y = -145;
        characterButton.positionScale_X = 0.5f;
        characterButton.positionScale_Y = 0.5f;
        characterButton.sizeOffset_X = 200;
        characterButton.sizeOffset_Y = 50;
        characterButton.text = local.format("CharacterButtonText");
        characterButton.tooltip = local.format("CharacterButtonTooltip");
        characterButton.onClickedButton += onClickedCharacterButton;
        characterButton.fontSize = ESleekFontSize.Medium;
        characterButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(characterButton);
        appearanceButton = new SleekButtonIcon(bundle.load<Texture2D>("Appearance"));
        appearanceButton.positionOffset_X = -100;
        appearanceButton.positionOffset_Y = -85;
        appearanceButton.positionScale_X = 0.5f;
        appearanceButton.positionScale_Y = 0.5f;
        appearanceButton.sizeOffset_X = 200;
        appearanceButton.sizeOffset_Y = 50;
        appearanceButton.text = local.format("AppearanceButtonText");
        appearanceButton.tooltip = local.format("AppearanceButtonTooltip");
        appearanceButton.onClickedButton += onClickedAppearanceButton;
        appearanceButton.fontSize = ESleekFontSize.Medium;
        appearanceButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(appearanceButton);
        groupButton = new SleekButtonIcon(bundle.load<Texture2D>("Group"));
        groupButton.positionOffset_X = -100;
        groupButton.positionOffset_Y = -25;
        groupButton.positionScale_X = 0.5f;
        groupButton.positionScale_Y = 0.5f;
        groupButton.sizeOffset_X = 200;
        groupButton.sizeOffset_Y = 50;
        groupButton.text = local.format("GroupButtonText");
        groupButton.tooltip = local.format("GroupButtonTooltip");
        groupButton.onClickedButton += onClickedGroupButton;
        groupButton.iconColor = ESleekTint.FOREGROUND;
        groupButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(groupButton);
        clothingButton = new SleekButtonIcon(bundle.load<Texture2D>("Clothing"));
        clothingButton.positionOffset_X = -100;
        clothingButton.positionOffset_Y = 35;
        clothingButton.positionScale_X = 0.5f;
        clothingButton.positionScale_Y = 0.5f;
        clothingButton.sizeOffset_X = 200;
        clothingButton.sizeOffset_Y = 50;
        clothingButton.text = local.format("ClothingButtonText");
        clothingButton.tooltip = local.format("ClothingButtonTooltip");
        clothingButton.onClickedButton += onClickedClothingButton;
        clothingButton.fontSize = ESleekFontSize.Medium;
        clothingButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(clothingButton);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.positionOffset_X = -100;
        backButton.positionOffset_Y = 95;
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
        characterUI = new MenuSurvivorsCharacterUI();
        appearanceUI = new MenuSurvivorsAppearanceUI();
        groupUI = new MenuSurvivorsGroupUI();
        clothingUI = new MenuSurvivorsClothingUI();
    }
}
