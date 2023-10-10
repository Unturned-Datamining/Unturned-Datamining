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
        Characters.group(groups[Mathf.FloorToInt(button.PositionOffset_Y / 40f)].steamID);
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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        groups = Provider.provider.communityService.getGroups();
        markerColorBox = Glazier.Get().CreateBox();
        markerColorBox.PositionOffset_X = -120f;
        markerColorBox.PositionOffset_Y = 100f;
        markerColorBox.PositionScale_X = 0.75f;
        markerColorBox.SizeOffset_X = 240f;
        markerColorBox.SizeOffset_Y = 30f;
        markerColorBox.Text = localization.format("Marker_Color_Box");
        container.AddChild(markerColorBox);
        markerColorPicker = new SleekColorPicker();
        markerColorPicker.PositionOffset_X = -120f;
        markerColorPicker.PositionOffset_Y = 140f;
        markerColorPicker.PositionScale_X = 0.75f;
        markerColorPicker.onColorPicked = onPickedMarkerColor;
        container.AddChild(markerColorPicker);
        groupButton = new SleekButtonIcon(null, 20);
        groupButton.PositionOffset_X = -120f;
        groupButton.PositionOffset_Y = 270f;
        groupButton.PositionScale_X = 0.75f;
        groupButton.SizeOffset_X = 240f;
        groupButton.SizeOffset_Y = 30f;
        groupButton.AddLabel(localization.format("Group_Box_Label"), ESleekSide.LEFT);
        groupButton.onClickedButton += onClickedUngroupButton;
        container.AddChild(groupButton);
        groupsBox = Glazier.Get().CreateScrollView();
        groupsBox.PositionOffset_X = -120f;
        groupsBox.PositionOffset_Y = 310f;
        groupsBox.PositionScale_X = 0.75f;
        groupsBox.SizeOffset_X = 270f;
        groupsBox.SizeOffset_Y = -410f;
        groupsBox.SizeScale_Y = 1f;
        groupsBox.ScaleContentToWidth = true;
        groupsBox.ContentSizeOffset = new Vector2(0f, groups.Length * 40 - 10);
        container.AddChild(groupsBox);
        for (int i = 0; i < groups.Length; i++)
        {
            SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(groups[i].icon, 20);
            sleekButtonIcon.PositionOffset_Y = i * 40;
            sleekButtonIcon.SizeOffset_X = 240f;
            sleekButtonIcon.SizeOffset_Y = 30f;
            sleekButtonIcon.text = groups[i].name;
            sleekButtonIcon.onClickedButton += onClickedGroupButton;
            groupsBox.AddChild(sleekButtonIcon);
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
