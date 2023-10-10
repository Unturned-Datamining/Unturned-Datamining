using UnityEngine;

namespace SDG.Unturned;

public class SleekBoost : SleekWrapper
{
    private ISleekButton button;

    private ISleekLabel infoLabel;

    private ISleekLabel descriptionLabel;

    private ISleekLabel costLabel;

    public event ClickedButton onClickedButton;

    public SleekBoost(byte boost)
    {
        button = Glazier.Get().CreateButton();
        button.SizeScale_X = 1f;
        button.SizeScale_Y = 1f;
        button.TooltipText = PlayerDashboardSkillsUI.localization.format("Boost_" + boost + "_Tooltip");
        button.OnClicked += onClickedInternalButton;
        button.IsClickable = Player.player.skills.experience >= PlayerSkills.BOOST_COST;
        AddChild(button);
        infoLabel = Glazier.Get().CreateLabel();
        infoLabel.PositionOffset_X = 5f;
        infoLabel.PositionOffset_Y = 5f;
        infoLabel.SizeOffset_X = -10f;
        infoLabel.SizeOffset_Y = -5f;
        infoLabel.SizeScale_X = 0.5f;
        infoLabel.SizeScale_Y = 0.5f;
        infoLabel.TextAlignment = TextAnchor.MiddleLeft;
        infoLabel.Text = PlayerDashboardSkillsUI.localization.format("Boost_" + boost);
        infoLabel.FontSize = ESleekFontSize.Medium;
        AddChild(infoLabel);
        descriptionLabel = Glazier.Get().CreateLabel();
        descriptionLabel.PositionOffset_X = 5f;
        descriptionLabel.PositionOffset_Y = 5f;
        descriptionLabel.PositionScale_Y = 0.5f;
        descriptionLabel.SizeOffset_X = -10f;
        descriptionLabel.SizeOffset_Y = -5f;
        descriptionLabel.SizeScale_X = 0.5f;
        descriptionLabel.SizeScale_Y = 0.5f;
        descriptionLabel.TextAlignment = TextAnchor.MiddleLeft;
        descriptionLabel.Text = PlayerDashboardSkillsUI.localization.format("Boost_" + boost + "_Tooltip");
        AddChild(descriptionLabel);
        if (boost > 0)
        {
            ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.PositionOffset_X = 5f;
            sleekLabel.PositionOffset_Y = 5f;
            sleekLabel.PositionScale_X = 0.25f;
            sleekLabel.SizeOffset_X = -10f;
            sleekLabel.SizeOffset_Y = -10f;
            sleekLabel.SizeScale_X = 0.5f;
            sleekLabel.SizeScale_Y = 1f;
            sleekLabel.TextAlignment = TextAnchor.MiddleCenter;
            sleekLabel.Text = PlayerDashboardSkillsUI.localization.format("Boost_" + boost + "_Bonus");
            AddChild(sleekLabel);
        }
        costLabel = Glazier.Get().CreateLabel();
        costLabel.PositionOffset_X = 5f;
        costLabel.PositionOffset_Y = 5f;
        costLabel.PositionScale_X = 0.5f;
        costLabel.SizeOffset_X = -10f;
        costLabel.SizeOffset_Y = -10f;
        costLabel.SizeScale_X = 0.5f;
        costLabel.SizeScale_Y = 1f;
        costLabel.TextAlignment = TextAnchor.MiddleRight;
        costLabel.Text = PlayerDashboardSkillsUI.localization.format("Cost", PlayerSkills.BOOST_COST);
        AddChild(costLabel);
    }

    private void onClickedInternalButton(ISleekElement internalButton)
    {
        this.onClickedButton?.Invoke(this);
    }
}
