using System;
using UnityEngine;

namespace SDG.Unturned;

public class MenuSurvivorsGroupUI
{
    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static SteamGroup[] groups;

    private static ISleekBox markerColorBox;

    private static SleekColorPicker markerColorPicker;

    private static SleekButtonIcon groupButton;

    private static ISleekScrollView groupsBox;

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
        if (index != Characters.selected)
        {
            return;
        }
        markerColorPicker.state = character.markerColor;
        for (int i = 0; i < groups.Length; i++)
        {
            if (groups[i].steamID == character.group)
            {
                groupButton.text = groups[i].name;
                groupButton.icon = groups[i].icon;
                return;
            }
        }
        groupButton.text = localization.format("Group_Box");
        groupButton.icon = null;
    }

    private static void onTypedNickField(ISleekField field, string text)
    {
        Characters.renick(text);
    }

    private static void onPickedMarkerColor(SleekColorPicker picker, Color state)
    {
        Characters.paintMarkerColor(state);
    }

    private static void onClickedGroupButton(ISleekElement button)
    {
        Characters.group(groups[button.positionOffset_Y / 40].steamID);
    }

    private static void onClickedUngroupButton(ISleekElement button)
    {
        Characters.ungroup();
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

    public MenuSurvivorsGroupUI()
    {
        localization = Localization.read("/Menu/Survivors/MenuSurvivorsGroup.dat");
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
        groups = Provider.provider.communityService.getGroups();
        markerColorBox = Glazier.Get().CreateBox();
        markerColorBox.positionOffset_X = -120;
        markerColorBox.positionOffset_Y = 100;
        markerColorBox.positionScale_X = 0.75f;
        markerColorBox.sizeOffset_X = 240;
        markerColorBox.sizeOffset_Y = 30;
        markerColorBox.text = localization.format("Marker_Color_Box");
        container.AddChild(markerColorBox);
        markerColorPicker = new SleekColorPicker();
        markerColorPicker.positionOffset_X = -120;
        markerColorPicker.positionOffset_Y = 140;
        markerColorPicker.positionScale_X = 0.75f;
        markerColorPicker.onColorPicked = onPickedMarkerColor;
        container.AddChild(markerColorPicker);
        groupButton = new SleekButtonIcon(null, 20);
        groupButton.positionOffset_X = -120;
        groupButton.positionOffset_Y = 270;
        groupButton.positionScale_X = 0.75f;
        groupButton.sizeOffset_X = 240;
        groupButton.sizeOffset_Y = 30;
        groupButton.addLabel(localization.format("Group_Box_Label"), ESleekSide.LEFT);
        groupButton.onClickedButton += onClickedUngroupButton;
        container.AddChild(groupButton);
        groupsBox = Glazier.Get().CreateScrollView();
        groupsBox.positionOffset_X = -120;
        groupsBox.positionOffset_Y = 310;
        groupsBox.positionScale_X = 0.75f;
        groupsBox.sizeOffset_X = 270;
        groupsBox.sizeOffset_Y = -410;
        groupsBox.sizeScale_Y = 1f;
        groupsBox.scaleContentToWidth = true;
        groupsBox.contentSizeOffset = new Vector2(0f, groups.Length * 40 - 10);
        container.AddChild(groupsBox);
        for (int i = 0; i < groups.Length; i++)
        {
            SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(groups[i].icon, 20);
            sleekButtonIcon.positionOffset_Y = i * 40;
            sleekButtonIcon.sizeOffset_X = 240;
            sleekButtonIcon.sizeOffset_Y = 30;
            sleekButtonIcon.text = groups[i].name;
            sleekButtonIcon.onClickedButton += onClickedGroupButton;
            groupsBox.AddChild(sleekButtonIcon);
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
