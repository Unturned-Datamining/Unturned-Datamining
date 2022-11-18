using System;
using UnityEngine;

namespace SDG.Unturned;

internal class VolumeTypeButton : SleekWrapper
{
    public EditorVolumesUI owner;

    public VolumeManagerBase volumeType;

    private SleekButtonState visibilityButton;

    private ISleekButton nameButton;

    public VolumeTypeButton(EditorVolumesUI owner, VolumeManagerBase volumeType)
    {
        this.owner = owner;
        this.volumeType = volumeType;
        visibilityButton = new SleekButtonState(new GUIContent("H", owner.localization.format("Visibility_Hidden")), new GUIContent("W", owner.localization.format("Visibility_Wireframe")), new GUIContent("S", owner.localization.format("Visibility_Solid")));
        visibilityButton.sizeOffset_X = 50;
        visibilityButton.sizeOffset_Y = 30;
        visibilityButton.useContentTooltip = true;
        SleekButtonState sleekButtonState = visibilityButton;
        sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedVisibility));
        RefreshVisibility();
        AddChild(visibilityButton);
        nameButton = Glazier.Get().CreateButton();
        nameButton.positionOffset_X = 50;
        nameButton.sizeScale_X = 1f;
        nameButton.sizeScale_Y = 1f;
        nameButton.sizeOffset_X = -nameButton.positionOffset_X;
        nameButton.text = volumeType.FriendlyName;
        nameButton.onClickedButton += OnTypeClicked;
        AddChild(nameButton);
    }

    public void RefreshVisibility()
    {
        visibilityButton.state = (int)volumeType.Visibility;
    }

    private void OnSwappedVisibility(SleekButtonState button, int state)
    {
        volumeType.Visibility = (ELevelVolumeVisibility)state;
        owner.RefreshSelectedVisibility();
    }

    private void OnTypeClicked(ISleekElement element)
    {
        owner.SetSelectedType(volumeType);
    }
}
