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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = -1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        characterButton = new SleekButtonIcon(bundle.load<Texture2D>("Character"));
        characterButton.PositionOffset_X = -100f;
        characterButton.PositionOffset_Y = -145f;
        characterButton.PositionScale_X = 0.5f;
        characterButton.PositionScale_Y = 0.5f;
        characterButton.SizeOffset_X = 200f;
        characterButton.SizeOffset_Y = 50f;
        characterButton.text = local.format("CharacterButtonText");
        characterButton.tooltip = local.format("CharacterButtonTooltip");
        characterButton.onClickedButton += onClickedCharacterButton;
        characterButton.fontSize = ESleekFontSize.Medium;
        characterButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(characterButton);
        appearanceButton = new SleekButtonIcon(bundle.load<Texture2D>("Appearance"));
        appearanceButton.PositionOffset_X = -100f;
        appearanceButton.PositionOffset_Y = -85f;
        appearanceButton.PositionScale_X = 0.5f;
        appearanceButton.PositionScale_Y = 0.5f;
        appearanceButton.SizeOffset_X = 200f;
        appearanceButton.SizeOffset_Y = 50f;
        appearanceButton.text = local.format("AppearanceButtonText");
        appearanceButton.tooltip = local.format("AppearanceButtonTooltip");
        appearanceButton.onClickedButton += onClickedAppearanceButton;
        appearanceButton.fontSize = ESleekFontSize.Medium;
        appearanceButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(appearanceButton);
        groupButton = new SleekButtonIcon(bundle.load<Texture2D>("Group"));
        groupButton.PositionOffset_X = -100f;
        groupButton.PositionOffset_Y = -25f;
        groupButton.PositionScale_X = 0.5f;
        groupButton.PositionScale_Y = 0.5f;
        groupButton.SizeOffset_X = 200f;
        groupButton.SizeOffset_Y = 50f;
        groupButton.text = local.format("GroupButtonText");
        groupButton.tooltip = local.format("GroupButtonTooltip");
        groupButton.onClickedButton += onClickedGroupButton;
        groupButton.iconColor = ESleekTint.FOREGROUND;
        groupButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(groupButton);
        clothingButton = new SleekButtonIcon(bundle.load<Texture2D>("Clothing"));
        clothingButton.PositionOffset_X = -100f;
        clothingButton.PositionOffset_Y = 35f;
        clothingButton.PositionScale_X = 0.5f;
        clothingButton.PositionScale_Y = 0.5f;
        clothingButton.SizeOffset_X = 200f;
        clothingButton.SizeOffset_Y = 50f;
        clothingButton.text = local.format("ClothingButtonText");
        clothingButton.tooltip = local.format("ClothingButtonTooltip");
        clothingButton.onClickedButton += onClickedClothingButton;
        clothingButton.fontSize = ESleekFontSize.Medium;
        clothingButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(clothingButton);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.PositionOffset_X = -100f;
        backButton.PositionOffset_Y = 95f;
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
        characterUI = new MenuSurvivorsCharacterUI();
        appearanceUI = new MenuSurvivorsAppearanceUI();
        groupUI = new MenuSurvivorsGroupUI();
        clothingUI = new MenuSurvivorsClothingUI();
    }
}
