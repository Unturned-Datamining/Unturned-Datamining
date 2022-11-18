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
        button.sizeScale_X = 1f;
        button.sizeScale_Y = 1f;
        button.tooltipText = PlayerDashboardSkillsUI.localization.format("Boost_" + boost + "_Tooltip");
        button.onClickedButton += onClickedInternalButton;
        button.isClickable = Player.player.skills.experience >= PlayerSkills.BOOST_COST;
        AddChild(button);
        infoLabel = Glazier.Get().CreateLabel();
        infoLabel.positionOffset_X = 5;
        infoLabel.positionOffset_Y = 5;
        infoLabel.sizeOffset_X = -10;
        infoLabel.sizeOffset_Y = -5;
        infoLabel.sizeScale_X = 0.5f;
        infoLabel.sizeScale_Y = 0.5f;
        infoLabel.fontAlignment = TextAnchor.MiddleLeft;
        infoLabel.text = PlayerDashboardSkillsUI.localization.format("Boost_" + boost);
        infoLabel.fontSize = ESleekFontSize.Medium;
        AddChild(infoLabel);
        descriptionLabel = Glazier.Get().CreateLabel();
        descriptionLabel.positionOffset_X = 5;
        descriptionLabel.positionOffset_Y = 5;
        descriptionLabel.positionScale_Y = 0.5f;
        descriptionLabel.sizeOffset_X = -10;
        descriptionLabel.sizeOffset_Y = -5;
        descriptionLabel.sizeScale_X = 0.5f;
        descriptionLabel.sizeScale_Y = 0.5f;
        descriptionLabel.fontAlignment = TextAnchor.MiddleLeft;
        descriptionLabel.text = PlayerDashboardSkillsUI.localization.format("Boost_" + boost + "_Tooltip");
        AddChild(descriptionLabel);
        if (boost > 0)
        {
            ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.positionOffset_X = 5;
            sleekLabel.positionOffset_Y = 5;
            sleekLabel.positionScale_X = 0.25f;
            sleekLabel.sizeOffset_X = -10;
            sleekLabel.sizeOffset_Y = -10;
            sleekLabel.sizeScale_X = 0.5f;
            sleekLabel.sizeScale_Y = 1f;
            sleekLabel.fontAlignment = TextAnchor.MiddleCenter;
            sleekLabel.text = PlayerDashboardSkillsUI.localization.format("Boost_" + boost + "_Bonus");
            AddChild(sleekLabel);
        }
        costLabel = Glazier.Get().CreateLabel();
        costLabel.positionOffset_X = 5;
        costLabel.positionOffset_Y = 5;
        costLabel.positionScale_X = 0.5f;
        costLabel.sizeOffset_X = -10;
        costLabel.sizeOffset_Y = -10;
        costLabel.sizeScale_X = 0.5f;
        costLabel.sizeScale_Y = 1f;
        costLabel.fontAlignment = TextAnchor.MiddleRight;
        costLabel.text = PlayerDashboardSkillsUI.localization.format("Cost", PlayerSkills.BOOST_COST);
        AddChild(costLabel);
    }

    private void onClickedInternalButton(ISleekElement internalButton)
    {
        this.onClickedButton?.Invoke(this);
    }
}
