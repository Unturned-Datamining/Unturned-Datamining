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
            addButton.Text = localization.format("Add_Button", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
            removeButton.Text = localization.format("Remove_Button", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
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
        base.PositionScale_Y = 1f;
        base.PositionOffset_X = 10f;
        base.PositionOffset_Y = 10f;
        base.SizeOffset_X = -20f;
        base.SizeOffset_Y = -20f;
        base.SizeScale_X = 1f;
        base.SizeScale_Y = 1f;
        active = false;
        mannequin = null;
        cosmeticsButton = Glazier.Get().CreateButton();
        cosmeticsButton.PositionOffset_X = -100f;
        cosmeticsButton.PositionOffset_Y = -135f;
        cosmeticsButton.PositionScale_X = 0.5f;
        cosmeticsButton.PositionScale_Y = 0.5f;
        cosmeticsButton.SizeOffset_X = 200f;
        cosmeticsButton.SizeOffset_Y = 30f;
        cosmeticsButton.Text = localization.format("Cosmetics_Button");
        cosmeticsButton.TooltipText = localization.format("Cosmetics_Button_Tooltip");
        cosmeticsButton.OnClicked += onClickedCosmeticsButton;
        AddChild(cosmeticsButton);
        addButton = Glazier.Get().CreateButton();
        addButton.PositionOffset_X = -100f;
        addButton.PositionOffset_Y = -95f;
        addButton.PositionScale_X = 0.5f;
        addButton.PositionScale_Y = 0.5f;
        addButton.SizeOffset_X = 200f;
        addButton.SizeOffset_Y = 30f;
        addButton.Text = localization.format("Add_Button");
        addButton.TooltipText = localization.format("Add_Button_Tooltip");
        addButton.OnClicked += onClickedAddButton;
        AddChild(addButton);
        removeButton = Glazier.Get().CreateButton();
        removeButton.PositionOffset_X = -100f;
        removeButton.PositionOffset_Y = -55f;
        removeButton.PositionScale_X = 0.5f;
        removeButton.PositionScale_Y = 0.5f;
        removeButton.SizeOffset_X = 200f;
        removeButton.SizeOffset_Y = 30f;
        removeButton.TooltipText = localization.format("Remove_Button_Tooltip");
        removeButton.OnClicked += onClickedRemoveButton;
        AddChild(removeButton);
        swapButton = Glazier.Get().CreateButton();
        swapButton.PositionOffset_X = -100f;
        swapButton.PositionOffset_Y = -15f;
        swapButton.PositionScale_X = 0.5f;
        swapButton.PositionScale_Y = 0.5f;
        swapButton.SizeOffset_X = 200f;
        swapButton.SizeOffset_Y = 30f;
        swapButton.Text = localization.format("Swap_Button");
        swapButton.TooltipText = localization.format("Swap_Button_Tooltip");
        swapButton.OnClicked += onClickedSwapButton;
        AddChild(swapButton);
        poseButton = new SleekButtonState(new GUIContent(localization.format("T")), new GUIContent(localization.format("Classic")), new GUIContent(localization.format("Lie")));
        poseButton.PositionOffset_X = -100f;
        poseButton.PositionOffset_Y = 25f;
        poseButton.PositionScale_X = 0.5f;
        poseButton.PositionScale_Y = 0.5f;
        poseButton.SizeOffset_X = 200f;
        poseButton.SizeOffset_Y = 30f;
        poseButton.tooltip = localization.format("Pose_Button_Tooltip");
        poseButton.onSwappedState = onSwappedPoseState;
        AddChild(poseButton);
        mirrorButton = Glazier.Get().CreateButton();
        mirrorButton.PositionOffset_X = -100f;
        mirrorButton.PositionOffset_Y = 65f;
        mirrorButton.PositionScale_X = 0.5f;
        mirrorButton.PositionScale_Y = 0.5f;
        mirrorButton.SizeOffset_X = 200f;
        mirrorButton.SizeOffset_Y = 30f;
        mirrorButton.Text = localization.format("Mirror_Button");
        mirrorButton.TooltipText = localization.format("Mirror_Button_Tooltip");
        mirrorButton.OnClicked += onClickedMirrorButton;
        AddChild(mirrorButton);
        cancelButton = Glazier.Get().CreateButton();
        cancelButton.PositionOffset_X = -100f;
        cancelButton.PositionOffset_Y = 105f;
        cancelButton.PositionScale_X = 0.5f;
        cancelButton.PositionScale_Y = 0.5f;
        cancelButton.SizeOffset_X = 200f;
        cancelButton.SizeOffset_Y = 30f;
        cancelButton.Text = localization.format("Cancel_Button");
        cancelButton.TooltipText = localization.format("Cancel_Button_Tooltip");
        cancelButton.OnClicked += onClickedCancelButton;
        AddChild(cancelButton);
    }
}
