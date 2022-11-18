using System;
using UnityEngine;

namespace SDG.Unturned;

public class MenuSurvivorsAppearanceUI
{
    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static ISleekScrollView customizationBox;

    private static ISleekBox faceBox;

    private static ISleekBox hairBox;

    private static ISleekBox beardBox;

    private static ISleekButton[] faceButtons;

    private static ISleekButton[] hairButtons;

    private static ISleekButton[] beardButtons;

    private static ISleekBox skinBox;

    private static ISleekBox colorBox;

    private static ISleekButton[] skinButtons;

    private static ISleekButton[] colorButtons;

    private static SleekColorPicker skinColorPicker;

    private static SleekColorPicker colorColorPicker;

    private static SleekButtonState handState;

    private static ISleekSlider characterSlider;

    public static void open()
    {
        if (!active)
        {
            active = true;
            Characters.apply(showItems: false, showCosmetics: false);
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            Characters.apply(showItems: true, showCosmetics: true);
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private static void updateFaces(Color color)
    {
        for (int i = 0; i < faceButtons.Length; i++)
        {
            ((ISleekImage)faceButtons[i].GetChildAtIndex(0)).color = color;
        }
    }

    private static void updateColors(Color color)
    {
        for (int i = 1; i < hairButtons.Length; i++)
        {
            ((ISleekImage)hairButtons[i].GetChildAtIndex(0)).color = color;
        }
        for (int j = 1; j < beardButtons.Length; j++)
        {
            ((ISleekImage)beardButtons[j].GetChildAtIndex(0)).color = color;
        }
    }

    private static void onCharacterUpdated(byte index, Character character)
    {
        if (index == Characters.selected)
        {
            skinColorPicker.state = character.skin;
            colorColorPicker.state = character.color;
            updateFaces(character.skin);
            updateColors(character.color);
            handState.state = (character.hand ? 1 : 0);
        }
    }

    private static void onClickedFaceButton(ISleekElement button)
    {
        Characters.growFace((byte)(button.positionOffset_X / 50 + (button.positionOffset_Y - 40) / 50 * 5));
    }

    private static void onClickedHairButton(ISleekElement button)
    {
        Characters.growHair((byte)(button.positionOffset_X / 50 + (button.positionOffset_Y - 40) / 50 * 5));
    }

    private static void onClickedBeardButton(ISleekElement button)
    {
        Characters.growBeard((byte)(button.positionOffset_X / 50 + (button.positionOffset_Y - 40) / 50 * 5));
    }

    private static void onClickedSkinButton(ISleekElement button)
    {
        int num = button.positionOffset_X / 50 + (button.positionOffset_Y - 40) / 50 * 5;
        Color color = Customization.SKINS[num];
        Characters.paintSkin(color);
        skinColorPicker.state = color;
        updateFaces(color);
    }

    private static void onSkinColorPicked(SleekColorPicker picker, Color color)
    {
        Characters.paintSkin(color);
        updateFaces(color);
    }

    private static void onClickedColorButton(ISleekElement button)
    {
        int num = button.positionOffset_X / 50 + (button.positionOffset_Y - 40) / 50 * 5;
        Color color = Customization.COLORS[num];
        Characters.paintColor(color);
        colorColorPicker.state = color;
        updateColors(color);
    }

    private static void onColorColorPicked(SleekColorPicker picker, Color color)
    {
        Characters.paintColor(color);
        updateColors(color);
    }

    private static void onSwappedHandState(SleekButtonState button, int index)
    {
        Characters.hand(index != 0);
    }

    private static void onDraggedCharacterSlider(ISleekSlider slider, float state)
    {
        Characters.characterYaw = state * 360f;
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuSurvivorsUI.open();
        close();
    }

    public void OnDestroy()
    {
        Characters.onCharacterUpdated = (CharacterUpdated)Delegate.Remove(Characters.onCharacterUpdated, new CharacterUpdated(onCharacterUpdated));
    }

    public MenuSurvivorsAppearanceUI()
    {
        localization = Localization.read("/Menu/Survivors/MenuSurvivorsAppearance.dat");
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
        customizationBox = Glazier.Get().CreateScrollView();
        customizationBox.positionOffset_X = -140;
        customizationBox.positionOffset_Y = 100;
        customizationBox.positionScale_X = 0.75f;
        customizationBox.sizeOffset_X = 270;
        customizationBox.sizeOffset_Y = -270;
        customizationBox.sizeScale_Y = 1f;
        container.AddChild(customizationBox);
        faceBox = Glazier.Get().CreateBox();
        faceBox.sizeOffset_X = 240;
        faceBox.sizeOffset_Y = 30;
        faceBox.text = localization.format("Face_Box");
        faceBox.tooltipText = localization.format("Face_Box_Tooltip");
        customizationBox.AddChild(faceBox);
        faceButtons = new ISleekButton[Customization.FACES_FREE + Customization.FACES_PRO];
        for (int i = 0; i < faceButtons.Length; i++)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.positionOffset_X = i % 5 * 50;
            sleekButton.positionOffset_Y = 40 + Mathf.FloorToInt((float)i / 5f) * 50;
            sleekButton.sizeOffset_X = 40;
            sleekButton.sizeOffset_Y = 40;
            faceBox.AddChild(sleekButton);
            ISleekImage sleekImage = Glazier.Get().CreateImage();
            sleekImage.positionOffset_X = 10;
            sleekImage.positionOffset_Y = 10;
            sleekImage.sizeOffset_X = 20;
            sleekImage.sizeOffset_Y = 20;
            sleekImage.texture = (Texture2D)GlazierResources.PixelTexture;
            sleekButton.AddChild(sleekImage);
            ISleekImage sleekImage2 = Glazier.Get().CreateImage();
            sleekImage2.positionOffset_X = 2;
            sleekImage2.positionOffset_Y = 2;
            sleekImage2.sizeOffset_X = 16;
            sleekImage2.sizeOffset_Y = 16;
            sleekImage2.texture = (Texture2D)Resources.Load("Faces/" + i + "/Texture");
            sleekImage.AddChild(sleekImage2);
            if (i >= Customization.FACES_FREE)
            {
                if (Provider.isPro)
                {
                    sleekButton.onClickedButton += onClickedFaceButton;
                }
                else
                {
                    sleekButton.backgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
                    Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
                    ISleekImage sleekImage3 = Glazier.Get().CreateImage();
                    sleekImage3.positionOffset_X = -10;
                    sleekImage3.positionOffset_Y = -10;
                    sleekImage3.positionScale_X = 0.5f;
                    sleekImage3.positionScale_Y = 0.5f;
                    sleekImage3.sizeOffset_X = 20;
                    sleekImage3.sizeOffset_Y = 20;
                    sleekImage3.texture = bundle.load<Texture2D>("Lock_Small");
                    sleekButton.AddChild(sleekImage3);
                    bundle.unload();
                }
            }
            else
            {
                sleekButton.onClickedButton += onClickedFaceButton;
            }
            faceButtons[i] = sleekButton;
        }
        hairBox = Glazier.Get().CreateBox();
        hairBox.positionOffset_Y = 80 + Mathf.CeilToInt((float)faceButtons.Length / 5f) * 50;
        hairBox.sizeOffset_X = 240;
        hairBox.sizeOffset_Y = 30;
        hairBox.text = localization.format("Hair_Box");
        hairBox.tooltipText = localization.format("Hair_Box_Tooltip");
        customizationBox.AddChild(hairBox);
        hairButtons = new ISleekButton[Customization.HAIRS_FREE + Customization.HAIRS_PRO];
        for (int j = 0; j < hairButtons.Length; j++)
        {
            ISleekButton sleekButton2 = Glazier.Get().CreateButton();
            sleekButton2.positionOffset_X = j % 5 * 50;
            sleekButton2.positionOffset_Y = 40 + Mathf.FloorToInt((float)j / 5f) * 50;
            sleekButton2.sizeOffset_X = 40;
            sleekButton2.sizeOffset_Y = 40;
            hairBox.AddChild(sleekButton2);
            ISleekImage sleekImage4 = Glazier.Get().CreateImage();
            sleekImage4.positionOffset_X = 10;
            sleekImage4.positionOffset_Y = 10;
            sleekImage4.sizeOffset_X = 20;
            sleekImage4.sizeOffset_Y = 20;
            sleekImage4.texture = (Texture2D)Resources.Load("Hairs/" + j + "/Texture");
            sleekButton2.AddChild(sleekImage4);
            if (j >= Customization.HAIRS_FREE)
            {
                if (Provider.isPro)
                {
                    sleekButton2.onClickedButton += onClickedHairButton;
                }
                else
                {
                    sleekButton2.backgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
                    Bundle bundle2 = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
                    ISleekImage sleekImage5 = Glazier.Get().CreateImage();
                    sleekImage5.positionOffset_X = -10;
                    sleekImage5.positionOffset_Y = -10;
                    sleekImage5.positionScale_X = 0.5f;
                    sleekImage5.positionScale_Y = 0.5f;
                    sleekImage5.sizeOffset_X = 20;
                    sleekImage5.sizeOffset_Y = 20;
                    sleekImage5.texture = bundle2.load<Texture2D>("Lock_Small");
                    sleekButton2.AddChild(sleekImage5);
                    bundle2.unload();
                }
            }
            else
            {
                sleekButton2.onClickedButton += onClickedHairButton;
            }
            hairButtons[j] = sleekButton2;
        }
        beardBox = Glazier.Get().CreateBox();
        beardBox.positionOffset_Y = 160 + Mathf.CeilToInt((float)faceButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)hairButtons.Length / 5f) * 50;
        beardBox.sizeOffset_X = 240;
        beardBox.sizeOffset_Y = 30;
        beardBox.text = localization.format("Beard_Box");
        beardBox.tooltipText = localization.format("Beard_Box_Tooltip");
        customizationBox.AddChild(beardBox);
        beardButtons = new ISleekButton[Customization.BEARDS_FREE + Customization.BEARDS_PRO];
        for (int k = 0; k < beardButtons.Length; k++)
        {
            ISleekButton sleekButton3 = Glazier.Get().CreateButton();
            sleekButton3.positionOffset_X = k % 5 * 50;
            sleekButton3.positionOffset_Y = 40 + Mathf.FloorToInt((float)k / 5f) * 50;
            sleekButton3.sizeOffset_X = 40;
            sleekButton3.sizeOffset_Y = 40;
            beardBox.AddChild(sleekButton3);
            ISleekImage sleekImage6 = Glazier.Get().CreateImage();
            sleekImage6.positionOffset_X = 10;
            sleekImage6.positionOffset_Y = 10;
            sleekImage6.sizeOffset_X = 20;
            sleekImage6.sizeOffset_Y = 20;
            sleekImage6.texture = (Texture2D)Resources.Load("Beards/" + k + "/Texture");
            sleekButton3.AddChild(sleekImage6);
            if (k >= Customization.BEARDS_FREE)
            {
                if (Provider.isPro)
                {
                    sleekButton3.onClickedButton += onClickedBeardButton;
                }
                else
                {
                    sleekButton3.backgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
                    Bundle bundle3 = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
                    ISleekImage sleekImage7 = Glazier.Get().CreateImage();
                    sleekImage7.positionOffset_X = -10;
                    sleekImage7.positionOffset_Y = -10;
                    sleekImage7.positionScale_X = 0.5f;
                    sleekImage7.positionScale_Y = 0.5f;
                    sleekImage7.sizeOffset_X = 20;
                    sleekImage7.sizeOffset_Y = 20;
                    sleekImage7.texture = bundle3.load<Texture2D>("Lock_Small");
                    sleekButton3.AddChild(sleekImage7);
                    bundle3.unload();
                }
            }
            else
            {
                sleekButton3.onClickedButton += onClickedBeardButton;
            }
            beardButtons[k] = sleekButton3;
        }
        skinBox = Glazier.Get().CreateBox();
        skinBox.positionOffset_Y = 240 + Mathf.CeilToInt((float)faceButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)hairButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)beardButtons.Length / 5f) * 50;
        skinBox.sizeOffset_X = 240;
        skinBox.sizeOffset_Y = 30;
        skinBox.text = localization.format("Skin_Box");
        skinBox.tooltipText = localization.format("Skin_Box_Tooltip");
        customizationBox.AddChild(skinBox);
        skinButtons = new ISleekButton[Customization.SKINS.Length];
        for (int l = 0; l < skinButtons.Length; l++)
        {
            ISleekButton sleekButton4 = Glazier.Get().CreateButton();
            sleekButton4.positionOffset_X = l % 5 * 50;
            sleekButton4.positionOffset_Y = 40 + Mathf.FloorToInt((float)l / 5f) * 50;
            sleekButton4.sizeOffset_X = 40;
            sleekButton4.sizeOffset_Y = 40;
            sleekButton4.onClickedButton += onClickedSkinButton;
            skinBox.AddChild(sleekButton4);
            ISleekImage sleekImage8 = Glazier.Get().CreateImage();
            sleekImage8.positionOffset_X = 10;
            sleekImage8.positionOffset_Y = 10;
            sleekImage8.sizeOffset_X = 20;
            sleekImage8.sizeOffset_Y = 20;
            sleekImage8.texture = (Texture2D)GlazierResources.PixelTexture;
            sleekImage8.color = Customization.SKINS[l];
            sleekButton4.AddChild(sleekImage8);
            skinButtons[l] = sleekButton4;
        }
        skinColorPicker = new SleekColorPicker();
        skinColorPicker.positionOffset_Y = 280 + Mathf.CeilToInt((float)faceButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)hairButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)beardButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)skinButtons.Length / 5f) * 50;
        customizationBox.AddChild(skinColorPicker);
        if (Provider.isPro)
        {
            skinColorPicker.onColorPicked = onSkinColorPicked;
        }
        else
        {
            Bundle bundle4 = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
            ISleekImage sleekImage9 = Glazier.Get().CreateImage();
            sleekImage9.positionOffset_X = -40;
            sleekImage9.positionOffset_Y = -40;
            sleekImage9.positionScale_X = 0.5f;
            sleekImage9.positionScale_Y = 0.5f;
            sleekImage9.sizeOffset_X = 80;
            sleekImage9.sizeOffset_Y = 80;
            sleekImage9.texture = bundle4.load<Texture2D>("Lock_Large");
            skinColorPicker.AddChild(sleekImage9);
            bundle4.unload();
        }
        colorBox = Glazier.Get().CreateBox();
        colorBox.positionOffset_Y = 440 + Mathf.CeilToInt((float)faceButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)hairButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)beardButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)skinButtons.Length / 5f) * 50;
        colorBox.sizeOffset_X = 240;
        colorBox.sizeOffset_Y = 30;
        colorBox.text = localization.format("Color_Box");
        colorBox.tooltipText = localization.format("Color_Box_Tooltip");
        customizationBox.AddChild(colorBox);
        colorButtons = new ISleekButton[Customization.COLORS.Length];
        for (int m = 0; m < colorButtons.Length; m++)
        {
            ISleekButton sleekButton5 = Glazier.Get().CreateButton();
            sleekButton5.positionOffset_X = m % 5 * 50;
            sleekButton5.positionOffset_Y = 40 + Mathf.FloorToInt((float)m / 5f) * 50;
            sleekButton5.sizeOffset_X = 40;
            sleekButton5.sizeOffset_Y = 40;
            sleekButton5.onClickedButton += onClickedColorButton;
            colorBox.AddChild(sleekButton5);
            ISleekImage sleekImage10 = Glazier.Get().CreateImage();
            sleekImage10.positionOffset_X = 10;
            sleekImage10.positionOffset_Y = 10;
            sleekImage10.sizeOffset_X = 20;
            sleekImage10.sizeOffset_Y = 20;
            sleekImage10.texture = (Texture2D)GlazierResources.PixelTexture;
            sleekImage10.color = Customization.COLORS[m];
            sleekButton5.AddChild(sleekImage10);
            colorButtons[m] = sleekButton5;
        }
        colorColorPicker = new SleekColorPicker();
        colorColorPicker.positionOffset_Y = 480 + Mathf.CeilToInt((float)faceButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)hairButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)beardButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)skinButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)colorButtons.Length / 5f) * 50;
        customizationBox.AddChild(colorColorPicker);
        if (Provider.isPro)
        {
            colorColorPicker.onColorPicked = onColorColorPicked;
        }
        else
        {
            Bundle bundle5 = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
            ISleekImage sleekImage11 = Glazier.Get().CreateImage();
            sleekImage11.positionOffset_X = -40;
            sleekImage11.positionOffset_Y = -40;
            sleekImage11.positionScale_X = 0.5f;
            sleekImage11.positionScale_Y = 0.5f;
            sleekImage11.sizeOffset_X = 80;
            sleekImage11.sizeOffset_Y = 80;
            sleekImage11.texture = bundle5.load<Texture2D>("Lock_Large");
            colorColorPicker.AddChild(sleekImage11);
            bundle5.unload();
        }
        customizationBox.scaleContentToWidth = true;
        customizationBox.contentSizeOffset = new Vector2(0f, 600 + Mathf.CeilToInt((float)faceButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)hairButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)beardButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)skinButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)colorButtons.Length / 5f) * 50);
        handState = new SleekButtonState(new GUIContent(localization.format("Right")), new GUIContent(localization.format("Left")));
        handState.positionOffset_X = -140;
        handState.positionOffset_Y = -160;
        handState.positionScale_X = 0.75f;
        handState.positionScale_Y = 1f;
        handState.sizeOffset_X = 240;
        handState.sizeOffset_Y = 30;
        handState.onSwappedState = onSwappedHandState;
        container.AddChild(handState);
        characterSlider = Glazier.Get().CreateSlider();
        characterSlider.positionOffset_X = -140;
        characterSlider.positionOffset_Y = -120;
        characterSlider.positionScale_X = 0.75f;
        characterSlider.positionScale_Y = 1f;
        characterSlider.sizeOffset_X = 240;
        characterSlider.sizeOffset_Y = 20;
        characterSlider.orientation = ESleekOrientation.HORIZONTAL;
        characterSlider.onDragged += onDraggedCharacterSlider;
        container.AddChild(characterSlider);
        Characters.onCharacterUpdated = (CharacterUpdated)Delegate.Combine(Characters.onCharacterUpdated, new CharacterUpdated(onCharacterUpdated));
        onCharacterUpdated(Characters.selected, Characters.list[Characters.selected]);
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
    }
}
