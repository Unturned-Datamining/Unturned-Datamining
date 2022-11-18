using System;
using UnityEngine;

namespace SDG.Unturned;

public class MenuSurvivorsCharacterUI
{
    public static Local localization;

    public static Bundle icons;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static ISleekScrollView characterBox;

    private static SleekCharacter[] characterButtons;

    private static ISleekField nameField;

    private static ISleekField nickField;

    private static SleekBoxIcon skillsetBox;

    private static ISleekScrollView skillsetsBox;

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

    private static void onCharacterUpdated(byte index, Character character)
    {
        if (index == Characters.selected)
        {
            nameField.text = character.name;
            nickField.text = character.nick;
            SleekBoxIcon sleekBoxIcon = skillsetBox;
            Bundle bundle = icons;
            int skillset = (int)character.skillset;
            sleekBoxIcon.icon = bundle.load<Texture2D>("Skillset_" + skillset);
            skillsetBox.text = localization.format("Skillset_" + (byte)character.skillset);
        }
        characterButtons[index].updateCharacter(character);
    }

    private static void onTypedNameField(ISleekField field, string text)
    {
        Characters.rename(text);
    }

    private static void onTypedNickField(ISleekField field, string text)
    {
        Characters.renick(text);
    }

    private static void onClickedCharacter(SleekCharacter character, byte index)
    {
        Characters.selected = index;
    }

    private static void onClickedSkillset(ISleekElement button)
    {
        Characters.skillify((EPlayerSkillset)(button.positionOffset_Y / 40));
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

    public MenuSurvivorsCharacterUI()
    {
        if (icons != null)
        {
            icons.unload();
        }
        localization = Localization.read("/Menu/Survivors/MenuSurvivorsCharacter.dat");
        icons = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Survivors/MenuSurvivorsCharacter/MenuSurvivorsCharacter.unity3d");
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
        characterBox = Glazier.Get().CreateScrollView();
        characterBox.positionOffset_X = -100;
        characterBox.positionOffset_Y = 45;
        characterBox.positionScale_X = 0.75f;
        characterBox.positionScale_Y = 0.5f;
        characterBox.sizeOffset_X = 230;
        characterBox.sizeOffset_Y = -145;
        characterBox.sizeScale_Y = 0.5f;
        characterBox.scaleContentToWidth = true;
        characterBox.contentSizeOffset = new Vector2(0f, (Customization.FREE_CHARACTERS + Customization.PRO_CHARACTERS) * 80 - 10);
        container.AddChild(characterBox);
        characterButtons = new SleekCharacter[Customization.FREE_CHARACTERS + Customization.PRO_CHARACTERS];
        for (byte b = 0; b < characterButtons.Length; b = (byte)(b + 1))
        {
            SleekCharacter sleekCharacter = new SleekCharacter(b)
            {
                positionOffset_Y = b * 80,
                sizeOffset_X = 200,
                sizeOffset_Y = 70,
                onClickedCharacter = onClickedCharacter
            };
            characterBox.AddChild(sleekCharacter);
            characterButtons[b] = sleekCharacter;
        }
        nameField = Glazier.Get().CreateStringField();
        nameField.positionOffset_X = -100;
        nameField.positionOffset_Y = 100;
        nameField.positionScale_X = 0.75f;
        nameField.sizeOffset_X = 200;
        nameField.sizeOffset_Y = 30;
        nameField.maxLength = 32;
        nameField.addLabel(localization.format("Name_Field_Label"), ESleekSide.LEFT);
        nameField.onTyped += onTypedNameField;
        container.AddChild(nameField);
        nickField = Glazier.Get().CreateStringField();
        nickField.positionOffset_X = -100;
        nickField.positionOffset_Y = 140;
        nickField.positionScale_X = 0.75f;
        nickField.sizeOffset_X = 200;
        nickField.sizeOffset_Y = 30;
        nickField.maxLength = 32;
        nickField.addLabel(localization.format("Nick_Field_Label"), ESleekSide.LEFT);
        nickField.onTyped += onTypedNickField;
        container.AddChild(nickField);
        skillsetBox = new SleekBoxIcon(null);
        skillsetBox.positionOffset_X = -100;
        skillsetBox.positionOffset_Y = 180;
        skillsetBox.positionScale_X = 0.75f;
        skillsetBox.sizeOffset_X = 200;
        skillsetBox.sizeOffset_Y = 30;
        skillsetBox.iconColor = ESleekTint.FOREGROUND;
        skillsetBox.addLabel(localization.format("Skillset_Box_Label"), ESleekSide.LEFT);
        container.AddChild(skillsetBox);
        skillsetsBox = Glazier.Get().CreateScrollView();
        skillsetsBox.positionOffset_X = -100;
        skillsetsBox.positionOffset_Y = 220;
        skillsetsBox.positionScale_X = 0.75f;
        skillsetsBox.sizeOffset_X = 230;
        skillsetsBox.sizeOffset_Y = -185;
        skillsetsBox.sizeScale_Y = 0.5f;
        skillsetsBox.scaleContentToWidth = true;
        skillsetsBox.contentSizeOffset = new Vector2(0f, Customization.SKILLSETS * 40 - 10);
        container.AddChild(skillsetsBox);
        for (int i = 0; i < Customization.SKILLSETS; i++)
        {
            SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(icons.load<Texture2D>("Skillset_" + i));
            sleekButtonIcon.positionOffset_Y = i * 40;
            sleekButtonIcon.sizeOffset_X = 200;
            sleekButtonIcon.sizeOffset_Y = 30;
            sleekButtonIcon.text = localization.format("Skillset_" + i);
            sleekButtonIcon.iconColor = ESleekTint.FOREGROUND;
            sleekButtonIcon.onClickedButton += onClickedSkillset;
            skillsetsBox.AddChild(sleekButtonIcon);
        }
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
