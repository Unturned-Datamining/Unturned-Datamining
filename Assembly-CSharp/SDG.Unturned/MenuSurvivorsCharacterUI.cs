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
            nameField.Text = character.name;
            nickField.Text = character.nick;
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
        Characters.skillify((EPlayerSkillset)(button.PositionOffset_Y / 40f));
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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        characterBox = Glazier.Get().CreateScrollView();
        characterBox.PositionOffset_X = -100f;
        characterBox.PositionOffset_Y = 45f;
        characterBox.PositionScale_X = 0.75f;
        characterBox.PositionScale_Y = 0.5f;
        characterBox.SizeOffset_X = 230f;
        characterBox.SizeOffset_Y = -145f;
        characterBox.SizeScale_Y = 0.5f;
        characterBox.ScaleContentToWidth = true;
        characterBox.ContentSizeOffset = new Vector2(0f, (Customization.FREE_CHARACTERS + Customization.PRO_CHARACTERS) * 80 - 10);
        container.AddChild(characterBox);
        characterButtons = new SleekCharacter[Customization.FREE_CHARACTERS + Customization.PRO_CHARACTERS];
        for (byte b = 0; b < characterButtons.Length; b++)
        {
            SleekCharacter sleekCharacter = new SleekCharacter(b)
            {
                PositionOffset_Y = b * 80,
                SizeOffset_X = 200f,
                SizeOffset_Y = 70f,
                onClickedCharacter = onClickedCharacter
            };
            characterBox.AddChild(sleekCharacter);
            characterButtons[b] = sleekCharacter;
        }
        nameField = Glazier.Get().CreateStringField();
        nameField.PositionOffset_X = -100f;
        nameField.PositionOffset_Y = 100f;
        nameField.PositionScale_X = 0.75f;
        nameField.SizeOffset_X = 200f;
        nameField.SizeOffset_Y = 30f;
        nameField.MaxLength = 32;
        nameField.AddLabel(localization.format("Name_Field_Label"), ESleekSide.LEFT);
        nameField.OnTextChanged += onTypedNameField;
        container.AddChild(nameField);
        nickField = Glazier.Get().CreateStringField();
        nickField.PositionOffset_X = -100f;
        nickField.PositionOffset_Y = 140f;
        nickField.PositionScale_X = 0.75f;
        nickField.SizeOffset_X = 200f;
        nickField.SizeOffset_Y = 30f;
        nickField.MaxLength = 32;
        nickField.AddLabel(localization.format("Nick_Field_Label"), ESleekSide.LEFT);
        nickField.OnTextChanged += onTypedNickField;
        container.AddChild(nickField);
        skillsetBox = new SleekBoxIcon(null);
        skillsetBox.PositionOffset_X = -100f;
        skillsetBox.PositionOffset_Y = 180f;
        skillsetBox.PositionScale_X = 0.75f;
        skillsetBox.SizeOffset_X = 200f;
        skillsetBox.SizeOffset_Y = 30f;
        skillsetBox.iconColor = ESleekTint.FOREGROUND;
        skillsetBox.AddLabel(localization.format("Skillset_Box_Label"), ESleekSide.LEFT);
        container.AddChild(skillsetBox);
        skillsetsBox = Glazier.Get().CreateScrollView();
        skillsetsBox.PositionOffset_X = -100f;
        skillsetsBox.PositionOffset_Y = 220f;
        skillsetsBox.PositionScale_X = 0.75f;
        skillsetsBox.SizeOffset_X = 230f;
        skillsetsBox.SizeOffset_Y = -185f;
        skillsetsBox.SizeScale_Y = 0.5f;
        skillsetsBox.ScaleContentToWidth = true;
        skillsetsBox.ContentSizeOffset = new Vector2(0f, Customization.SKILLSETS * 40 - 10);
        container.AddChild(skillsetsBox);
        for (int i = 0; i < Customization.SKILLSETS; i++)
        {
            SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(icons.load<Texture2D>("Skillset_" + i));
            sleekButtonIcon.PositionOffset_Y = i * 40;
            sleekButtonIcon.SizeOffset_X = 200f;
            sleekButtonIcon.SizeOffset_Y = 30f;
            sleekButtonIcon.text = localization.format("Skillset_" + i);
            sleekButtonIcon.iconColor = ESleekTint.FOREGROUND;
            sleekButtonIcon.onClickedButton += onClickedSkillset;
            skillsetsBox.AddChild(sleekButtonIcon);
        }
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
