using UnityEngine;

namespace SDG.Unturned;

public class PlayerBarricadeMannequinUI : SleekFullscreenBox
{
    private Local localization;

    public bool active;

    private InteractableMannequin mannequin;

    private ISleekButton cosmeticsButton;

    private ISleekButton addButton;

    private ISleekButton removeButton;

    private ISleekButton swapButton;

    private SleekButtonState poseButton;

    private ISleekButton mirrorButton;

    private ISleekButton cancelButton;

    public void open(InteractableMannequin newMannequin)
    {
        if (!active)
        {
            active = true;
            mannequin = newMannequin;
            addButton.text = localization.format("Add_Button", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
            removeButton.text = localization.format("Remove_Button", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
            if (mannequin != null)
            {
                poseButton.state = mannequin.pose;
            }
            AnimateIntoView();
        }
    }

    public void close()
    {
        if (active)
        {
            active = false;
            mannequin = null;
            AnimateOutOfView(0f, 1f);
        }
    }

    private void onClickedCosmeticsButton(ISleekElement button)
    {
        if (mannequin != null)
        {
            mannequin.ClientRequestUpdate(EMannequinUpdateMode.COSMETICS);
        }
        PlayerLifeUI.open();
        close();
    }

    private void onClickedAddButton(ISleekElement button)
    {
        if (mannequin != null)
        {
            mannequin.ClientRequestUpdate(EMannequinUpdateMode.ADD);
        }
        PlayerLifeUI.open();
        close();
    }

    private void onClickedRemoveButton(ISleekElement button)
    {
        if (mannequin != null)
        {
            mannequin.ClientRequestUpdate(EMannequinUpdateMode.REMOVE);
        }
        PlayerLifeUI.open();
        close();
    }

    private void onClickedSwapButton(ISleekElement button)
    {
        if (mannequin != null)
        {
            mannequin.ClientRequestUpdate(EMannequinUpdateMode.SWAP);
        }
        PlayerLifeUI.open();
        close();
    }

    private void onSwappedPoseState(SleekButtonState button, int index)
    {
        if (mannequin != null)
        {
            poseButton.state = mannequin.pose;
            byte comp = mannequin.getComp(mannequin.mirror, (byte)index);
            mannequin.ClientSetPose(comp);
        }
    }

    private void onClickedMirrorButton(ISleekElement button)
    {
        if (mannequin != null)
        {
            bool mirror = mannequin.mirror;
            mirror = !mirror;
            byte comp = mannequin.getComp(mirror, mannequin.pose);
            mannequin.ClientSetPose(comp);
        }
    }

    private void onClickedCancelButton(ISleekElement button)
    {
        PlayerLifeUI.open();
        close();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (mannequin != null)
        {
            poseButton.state = mannequin.pose;
        }
    }

    public PlayerBarricadeMannequinUI()
    {
        localization = Localization.read("/Player/PlayerBarricadeMannequin.dat");
        base.positionScale_Y = 1f;
        base.positionOffset_X = 10;
        base.positionOffset_Y = 10;
        base.sizeOffset_X = -20;
        base.sizeOffset_Y = -20;
        base.sizeScale_X = 1f;
        base.sizeScale_Y = 1f;
        active = false;
        mannequin = null;
        cosmeticsButton = Glazier.Get().CreateButton();
        cosmeticsButton.positionOffset_X = -100;
        cosmeticsButton.positionOffset_Y = -135;
        cosmeticsButton.positionScale_X = 0.5f;
        cosmeticsButton.positionScale_Y = 0.5f;
        cosmeticsButton.sizeOffset_X = 200;
        cosmeticsButton.sizeOffset_Y = 30;
        cosmeticsButton.text = localization.format("Cosmetics_Button");
        cosmeticsButton.tooltipText = localization.format("Cosmetics_Button_Tooltip");
        cosmeticsButton.onClickedButton += onClickedCosmeticsButton;
        AddChild(cosmeticsButton);
        addButton = Glazier.Get().CreateButton();
        addButton.positionOffset_X = -100;
        addButton.positionOffset_Y = -95;
        addButton.positionScale_X = 0.5f;
        addButton.positionScale_Y = 0.5f;
        addButton.sizeOffset_X = 200;
        addButton.sizeOffset_Y = 30;
        addButton.text = localization.format("Add_Button");
        addButton.tooltipText = localization.format("Add_Button_Tooltip");
        addButton.onClickedButton += onClickedAddButton;
        AddChild(addButton);
        removeButton = Glazier.Get().CreateButton();
        removeButton.positionOffset_X = -100;
        removeButton.positionOffset_Y = -55;
        removeButton.positionScale_X = 0.5f;
        removeButton.positionScale_Y = 0.5f;
        removeButton.sizeOffset_X = 200;
        removeButton.sizeOffset_Y = 30;
        removeButton.tooltipText = localization.format("Remove_Button_Tooltip");
        removeButton.onClickedButton += onClickedRemoveButton;
        AddChild(removeButton);
        swapButton = Glazier.Get().CreateButton();
        swapButton.positionOffset_X = -100;
        swapButton.positionOffset_Y = -15;
        swapButton.positionScale_X = 0.5f;
        swapButton.positionScale_Y = 0.5f;
        swapButton.sizeOffset_X = 200;
        swapButton.sizeOffset_Y = 30;
        swapButton.text = localization.format("Swap_Button");
        swapButton.tooltipText = localization.format("Swap_Button_Tooltip");
        swapButton.onClickedButton += onClickedSwapButton;
        AddChild(swapButton);
        poseButton = new SleekButtonState(new GUIContent(localization.format("T")), new GUIContent(localization.format("Classic")), new GUIContent(localization.format("Lie")));
        poseButton.positionOffset_X = -100;
        poseButton.positionOffset_Y = 25;
        poseButton.positionScale_X = 0.5f;
        poseButton.positionScale_Y = 0.5f;
        poseButton.sizeOffset_X = 200;
        poseButton.sizeOffset_Y = 30;
        poseButton.tooltip = localization.format("Pose_Button_Tooltip");
        poseButton.onSwappedState = onSwappedPoseState;
        AddChild(poseButton);
        mirrorButton = Glazier.Get().CreateButton();
        mirrorButton.positionOffset_X = -100;
        mirrorButton.positionOffset_Y = 65;
        mirrorButton.positionScale_X = 0.5f;
        mirrorButton.positionScale_Y = 0.5f;
        mirrorButton.sizeOffset_X = 200;
        mirrorButton.sizeOffset_Y = 30;
        mirrorButton.text = localization.format("Mirror_Button");
        mirrorButton.tooltipText = localization.format("Mirror_Button_Tooltip");
        mirrorButton.onClickedButton += onClickedMirrorButton;
        AddChild(mirrorButton);
        cancelButton = Glazier.Get().CreateButton();
        cancelButton.positionOffset_X = -100;
        cancelButton.positionOffset_Y = 105;
        cancelButton.positionScale_X = 0.5f;
        cancelButton.positionScale_Y = 0.5f;
        cancelButton.sizeOffset_X = 200;
        cancelButton.sizeOffset_Y = 30;
        cancelButton.text = localization.format("Cancel_Button");
        cancelButton.tooltipText = localization.format("Cancel_Button_Tooltip");
        cancelButton.onClickedButton += onClickedCancelButton;
        AddChild(cancelButton);
    }
}
