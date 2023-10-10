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
            ((ISleekImage)faceButtons[i].GetChildAtIndex(0)).TintColor = color;
        }
    }

    private static void updateColors(Color color)
    {
        for (int i = 1; i < hairButtons.Length; i++)
        {
            ((ISleekImage)hairButtons[i].GetChildAtIndex(0)).TintColor = color;
        }
        for (int j = 1; j < beardButtons.Length; j++)
        {
            ((ISleekImage)beardButtons[j].GetChildAtIndex(0)).TintColor = color;
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
        Characters.growFace((byte)Mathf.FloorToInt(button.PositionOffset_X / 50f + (button.PositionOffset_Y - 40f) / 50f * 5f));
    }

    private static void onClickedHairButton(ISleekElement button)
    {
        Characters.growHair((byte)Mathf.FloorToInt(button.PositionOffset_X / 50f + (button.PositionOffset_Y - 40f) / 50f * 5f));
    }

    private static void onClickedBeardButton(ISleekElement button)
    {
        Characters.growBeard((byte)Mathf.FloorToInt(button.PositionOffset_X / 50f + (button.PositionOffset_Y - 40f) / 50f * 5f));
    }

    private static void onClickedSkinButton(ISleekElement button)
    {
        int num = Mathf.FloorToInt(button.PositionOffset_X / 50f + (button.PositionOffset_Y - 40f) / 50f * 5f);
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
        int num = Mathf.FloorToInt(button.PositionOffset_X / 50f + (button.PositionOffset_Y - 40f) / 50f * 5f);
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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        customizationBox = Glazier.Get().CreateScrollView();
        customizationBox.PositionOffset_X = -140f;
        customizationBox.PositionOffset_Y = 100f;
        customizationBox.PositionScale_X = 0.75f;
        customizationBox.SizeOffset_X = 270f;
        customizationBox.SizeOffset_Y = -270f;
        customizationBox.SizeScale_Y = 1f;
        container.AddChild(customizationBox);
        faceBox = Glazier.Get().CreateBox();
        faceBox.SizeOffset_X = 240f;
        faceBox.SizeOffset_Y = 30f;
        faceBox.Text = localization.format("Face_Box");
        faceBox.TooltipText = localization.format("Face_Box_Tooltip");
        customizationBox.AddChild(faceBox);
        faceButtons = new ISleekButton[Customization.FACES_FREE + Customization.FACES_PRO];
        for (int i = 0; i < faceButtons.Length; i++)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.PositionOffset_X = i % 5 * 50;
            sleekButton.PositionOffset_Y = 40 + Mathf.FloorToInt((float)i / 5f) * 50;
            sleekButton.SizeOffset_X = 40f;
            sleekButton.SizeOffset_Y = 40f;
            faceBox.AddChild(sleekButton);
            ISleekImage sleekImage = Glazier.Get().CreateImage();
            sleekImage.PositionOffset_X = 10f;
            sleekImage.PositionOffset_Y = 10f;
            sleekImage.SizeOffset_X = 20f;
            sleekImage.SizeOffset_Y = 20f;
            sleekImage.Texture = (Texture2D)GlazierResources.PixelTexture;
            sleekButton.AddChild(sleekImage);
            ISleekImage sleekImage2 = Glazier.Get().CreateImage();
            sleekImage2.PositionOffset_X = 2f;
            sleekImage2.PositionOffset_Y = 2f;
            sleekImage2.SizeOffset_X = 16f;
            sleekImage2.SizeOffset_Y = 16f;
            sleekImage2.Texture = (Texture2D)Resources.Load("Faces/" + i + "/Texture");
            sleekImage.AddChild(sleekImage2);
            if (i >= Customization.FACES_FREE)
            {
                if (Provider.isPro)
                {
                    sleekButton.OnClicked += onClickedFaceButton;
                }
                else
                {
                    sleekButton.BackgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
                    Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
                    ISleekImage sleekImage3 = Glazier.Get().CreateImage();
                    sleekImage3.PositionOffset_X = -10f;
                    sleekImage3.PositionOffset_Y = -10f;
                    sleekImage3.PositionScale_X = 0.5f;
                    sleekImage3.PositionScale_Y = 0.5f;
                    sleekImage3.SizeOffset_X = 20f;
                    sleekImage3.SizeOffset_Y = 20f;
                    sleekImage3.Texture = bundle.load<Texture2D>("Lock_Small");
                    sleekButton.AddChild(sleekImage3);
                    bundle.unload();
                }
            }
            else
            {
                sleekButton.OnClicked += onClickedFaceButton;
            }
            faceButtons[i] = sleekButton;
        }
        hairBox = Glazier.Get().CreateBox();
        hairBox.PositionOffset_Y = 80 + Mathf.CeilToInt((float)faceButtons.Length / 5f) * 50;
        hairBox.SizeOffset_X = 240f;
        hairBox.SizeOffset_Y = 30f;
        hairBox.Text = localization.format("Hair_Box");
        hairBox.TooltipText = localization.format("Hair_Box_Tooltip");
        customizationBox.AddChild(hairBox);
        hairButtons = new ISleekButton[Customization.HAIRS_FREE + Customization.HAIRS_PRO];
        for (int j = 0; j < hairButtons.Length; j++)
        {
            ISleekButton sleekButton2 = Glazier.Get().CreateButton();
            sleekButton2.PositionOffset_X = j % 5 * 50;
            sleekButton2.PositionOffset_Y = 40 + Mathf.FloorToInt((float)j / 5f) * 50;
            sleekButton2.SizeOffset_X = 40f;
            sleekButton2.SizeOffset_Y = 40f;
            hairBox.AddChild(sleekButton2);
            ISleekImage sleekImage4 = Glazier.Get().CreateImage();
            sleekImage4.PositionOffset_X = 10f;
            sleekImage4.PositionOffset_Y = 10f;
            sleekImage4.SizeOffset_X = 20f;
            sleekImage4.SizeOffset_Y = 20f;
            sleekImage4.Texture = (Texture2D)Resources.Load("Hairs/" + j + "/Texture");
            sleekButton2.AddChild(sleekImage4);
            if (j >= Customization.HAIRS_FREE)
            {
                if (Provider.isPro)
                {
                    sleekButton2.OnClicked += onClickedHairButton;
                }
                else
                {
                    sleekButton2.BackgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
                    Bundle bundle2 = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
                    ISleekImage sleekImage5 = Glazier.Get().CreateImage();
                    sleekImage5.PositionOffset_X = -10f;
                    sleekImage5.PositionOffset_Y = -10f;
                    sleekImage5.PositionScale_X = 0.5f;
                    sleekImage5.PositionScale_Y = 0.5f;
                    sleekImage5.SizeOffset_X = 20f;
                    sleekImage5.SizeOffset_Y = 20f;
                    sleekImage5.Texture = bundle2.load<Texture2D>("Lock_Small");
                    sleekButton2.AddChild(sleekImage5);
                    bundle2.unload();
                }
            }
            else
            {
                sleekButton2.OnClicked += onClickedHairButton;
            }
            hairButtons[j] = sleekButton2;
        }
        beardBox = Glazier.Get().CreateBox();
        beardBox.PositionOffset_Y = 160 + Mathf.CeilToInt((float)faceButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)hairButtons.Length / 5f) * 50;
        beardBox.SizeOffset_X = 240f;
        beardBox.SizeOffset_Y = 30f;
        beardBox.Text = localization.format("Beard_Box");
        beardBox.TooltipText = localization.format("Beard_Box_Tooltip");
        customizationBox.AddChild(beardBox);
        beardButtons = new ISleekButton[Customization.BEARDS_FREE + Customization.BEARDS_PRO];
        for (int k = 0; k < beardButtons.Length; k++)
        {
            ISleekButton sleekButton3 = Glazier.Get().CreateButton();
            sleekButton3.PositionOffset_X = k % 5 * 50;
            sleekButton3.PositionOffset_Y = 40 + Mathf.FloorToInt((float)k / 5f) * 50;
            sleekButton3.SizeOffset_X = 40f;
            sleekButton3.SizeOffset_Y = 40f;
            beardBox.AddChild(sleekButton3);
            ISleekImage sleekImage6 = Glazier.Get().CreateImage();
            sleekImage6.PositionOffset_X = 10f;
            sleekImage6.PositionOffset_Y = 10f;
            sleekImage6.SizeOffset_X = 20f;
            sleekImage6.SizeOffset_Y = 20f;
            sleekImage6.Texture = (Texture2D)Resources.Load("Beards/" + k + "/Texture");
            sleekButton3.AddChild(sleekImage6);
            if (k >= Customization.BEARDS_FREE)
            {
                if (Provider.isPro)
                {
                    sleekButton3.OnClicked += onClickedBeardButton;
                }
                else
                {
                    sleekButton3.BackgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
                    Bundle bundle3 = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
                    ISleekImage sleekImage7 = Glazier.Get().CreateImage();
                    sleekImage7.PositionOffset_X = -10f;
                    sleekImage7.PositionOffset_Y = -10f;
                    sleekImage7.PositionScale_X = 0.5f;
                    sleekImage7.PositionScale_Y = 0.5f;
                    sleekImage7.SizeOffset_X = 20f;
                    sleekImage7.SizeOffset_Y = 20f;
                    sleekImage7.Texture = bundle3.load<Texture2D>("Lock_Small");
                    sleekButton3.AddChild(sleekImage7);
                    bundle3.unload();
                }
            }
            else
            {
                sleekButton3.OnClicked += onClickedBeardButton;
            }
            beardButtons[k] = sleekButton3;
        }
        skinBox = Glazier.Get().CreateBox();
        skinBox.PositionOffset_Y = 240 + Mathf.CeilToInt((float)faceButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)hairButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)beardButtons.Length / 5f) * 50;
        skinBox.SizeOffset_X = 240f;
        skinBox.SizeOffset_Y = 30f;
        skinBox.Text = localization.format("Skin_Box");
        skinBox.TooltipText = localization.format("Skin_Box_Tooltip");
        customizationBox.AddChild(skinBox);
        skinButtons = new ISleekButton[Customization.SKINS.Length];
        for (int l = 0; l < skinButtons.Length; l++)
        {
            ISleekButton sleekButton4 = Glazier.Get().CreateButton();
            sleekButton4.PositionOffset_X = l % 5 * 50;
            sleekButton4.PositionOffset_Y = 40 + Mathf.FloorToInt((float)l / 5f) * 50;
            sleekButton4.SizeOffset_X = 40f;
            sleekButton4.SizeOffset_Y = 40f;
            sleekButton4.OnClicked += onClickedSkinButton;
            skinBox.AddChild(sleekButton4);
            ISleekImage sleekImage8 = Glazier.Get().CreateImage();
            sleekImage8.PositionOffset_X = 10f;
            sleekImage8.PositionOffset_Y = 10f;
            sleekImage8.SizeOffset_X = 20f;
            sleekImage8.SizeOffset_Y = 20f;
            sleekImage8.Texture = (Texture2D)GlazierResources.PixelTexture;
            sleekImage8.TintColor = Customization.SKINS[l];
            sleekButton4.AddChild(sleekImage8);
            skinButtons[l] = sleekButton4;
        }
        skinColorPicker = new SleekColorPicker();
        skinColorPicker.PositionOffset_Y = 280 + Mathf.CeilToInt((float)faceButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)hairButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)beardButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)skinButtons.Length / 5f) * 50;
        customizationBox.AddChild(skinColorPicker);
        if (Provider.isPro)
        {
            skinColorPicker.onColorPicked = onSkinColorPicked;
        }
        else
        {
            Bundle bundle4 = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
            ISleekImage sleekImage9 = Glazier.Get().CreateImage();
            sleekImage9.PositionOffset_X = -40f;
            sleekImage9.PositionOffset_Y = -40f;
            sleekImage9.PositionScale_X = 0.5f;
            sleekImage9.PositionScale_Y = 0.5f;
            sleekImage9.SizeOffset_X = 80f;
            sleekImage9.SizeOffset_Y = 80f;
            sleekImage9.Texture = bundle4.load<Texture2D>("Lock_Large");
            skinColorPicker.AddChild(sleekImage9);
            bundle4.unload();
        }
        colorBox = Glazier.Get().CreateBox();
        colorBox.PositionOffset_Y = 440 + Mathf.CeilToInt((float)faceButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)hairButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)beardButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)skinButtons.Length / 5f) * 50;
        colorBox.SizeOffset_X = 240f;
        colorBox.SizeOffset_Y = 30f;
        colorBox.Text = localization.format("Color_Box");
        colorBox.TooltipText = localization.format("Color_Box_Tooltip");
        customizationBox.AddChild(colorBox);
        colorButtons = new ISleekButton[Customization.COLORS.Length];
        for (int m = 0; m < colorButtons.Length; m++)
        {
            ISleekButton sleekButton5 = Glazier.Get().CreateButton();
            sleekButton5.PositionOffset_X = m % 5 * 50;
            sleekButton5.PositionOffset_Y = 40 + Mathf.FloorToInt((float)m / 5f) * 50;
            sleekButton5.SizeOffset_X = 40f;
            sleekButton5.SizeOffset_Y = 40f;
            sleekButton5.OnClicked += onClickedColorButton;
            colorBox.AddChild(sleekButton5);
            ISleekImage sleekImage10 = Glazier.Get().CreateImage();
            sleekImage10.PositionOffset_X = 10f;
            sleekImage10.PositionOffset_Y = 10f;
            sleekImage10.SizeOffset_X = 20f;
            sleekImage10.SizeOffset_Y = 20f;
            sleekImage10.Texture = (Texture2D)GlazierResources.PixelTexture;
            sleekImage10.TintColor = Customization.COLORS[m];
            sleekButton5.AddChild(sleekImage10);
            colorButtons[m] = sleekButton5;
        }
        colorColorPicker = new SleekColorPicker();
        colorColorPicker.PositionOffset_Y = 480 + Mathf.CeilToInt((float)faceButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)hairButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)beardButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)skinButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)colorButtons.Length / 5f) * 50;
        customizationBox.AddChild(colorColorPicker);
        if (Provider.isPro)
        {
            colorColorPicker.onColorPicked = onColorColorPicked;
        }
        else
        {
            Bundle bundle5 = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
            ISleekImage sleekImage11 = Glazier.Get().CreateImage();
            sleekImage11.PositionOffset_X = -40f;
            sleekImage11.PositionOffset_Y = -40f;
            sleekImage11.PositionScale_X = 0.5f;
            sleekImage11.PositionScale_Y = 0.5f;
            sleekImage11.SizeOffset_X = 80f;
            sleekImage11.SizeOffset_Y = 80f;
            sleekImage11.Texture = bundle5.load<Texture2D>("Lock_Large");
            colorColorPicker.AddChild(sleekImage11);
            bundle5.unload();
        }
        customizationBox.ScaleContentToWidth = true;
        customizationBox.ContentSizeOffset = new Vector2(0f, 600 + Mathf.CeilToInt((float)faceButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)hairButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)beardButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)skinButtons.Length / 5f) * 50 + Mathf.CeilToInt((float)colorButtons.Length / 5f) * 50);
        handState = new SleekButtonState(new GUIContent(localization.format("Right")), new GUIContent(localization.format("Left")));
        handState.PositionOffset_X = -140f;
        handState.PositionOffset_Y = -160f;
        handState.PositionScale_X = 0.75f;
        handState.PositionScale_Y = 1f;
        handState.SizeOffset_X = 240f;
        handState.SizeOffset_Y = 30f;
        handState.onSwappedState = onSwappedHandState;
        container.AddChild(handState);
        characterSlider = Glazier.Get().CreateSlider();
        characterSlider.PositionOffset_X = -140f;
        characterSlider.PositionOffset_Y = -120f;
        characterSlider.PositionScale_X = 0.75f;
        characterSlider.PositionScale_Y = 1f;
        characterSlider.SizeOffset_X = 240f;
        characterSlider.SizeOffset_Y = 20f;
        characterSlider.Orientation = ESleekOrientation.HORIZONTAL;
        characterSlider.OnValueChanged += onDraggedCharacterSlider;
        container.AddChild(characterSlider);
        Characters.onCharacterUpdated = (CharacterUpdated)Delegate.Combine(Characters.onCharacterUpdated, new CharacterUpdated(onCharacterUpdated));
        onCharacterUpdated(Characters.selected, Characters.list[Characters.selected]);
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
    }
}
