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
        visibilityButton.SizeOffset_X = 50f;
        visibilityButton.SizeOffset_Y = 30f;
        visibilityButton.UseContentTooltip = true;
        SleekButtonState sleekButtonState = visibilityButton;
        sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedVisibility));
        RefreshVisibility();
        AddChild(visibilityButton);
        nameButton = Glazier.Get().CreateButton();
        nameButton.PositionOffset_X = 50f;
        nameButton.SizeScale_X = 1f;
        nameButton.SizeScale_Y = 1f;
        nameButton.SizeOffset_X = 0f - nameButton.PositionOffset_X;
        nameButton.Text = volumeType.FriendlyName;
        nameButton.OnClicked += OnTypeClicked;
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
