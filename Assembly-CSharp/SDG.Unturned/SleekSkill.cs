using UnityEngine;

namespace SDG.Unturned;

public class SleekSkill : SleekWrapper
{
    private ISleekButton button;

    public event ClickedButton onClickedButton;

    public SleekSkill(byte speciality, byte index, Skill skill)
    {
        uint num = Player.player.skills.cost(speciality, index);
        button = Glazier.Get().CreateButton();
        button.sizeScale_X = 1f;
        button.sizeScale_Y = 1f;
        button.tooltipText = PlayerDashboardSkillsUI.localization.format("Speciality_" + speciality + "_Skill_" + index + "_Tooltip");
        button.onClickedButton += onClickedInternalButton;
        button.isClickable = Player.player.skills.experience >= num && skill.level < skill.GetClampedMaxUnlockableLevel();
        AddChild(button);
        for (byte b = 0; b < skill.GetClampedMaxUnlockableLevel(); b = (byte)(b + 1))
        {
            ISleekImage sleekImage = Glazier.Get().CreateImage();
            sleekImage.positionOffset_X = -20 - b * 20;
            sleekImage.positionOffset_Y = 10;
            sleekImage.positionScale_X = 1f;
            sleekImage.sizeOffset_X = 10;
            sleekImage.sizeOffset_Y = -10;
            sleekImage.sizeScale_Y = 0.5f;
            if (b < skill.level)
            {
                sleekImage.texture = PlayerDashboardSkillsUI.icons.load<Texture2D>("Unlocked");
            }
            else
            {
                sleekImage.texture = PlayerDashboardSkillsUI.icons.load<Texture2D>("Locked");
            }
            AddChild(sleekImage);
        }
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.positionOffset_X = 5;
        sleekLabel.positionOffset_Y = 5;
        sleekLabel.sizeOffset_X = -10;
        sleekLabel.sizeOffset_Y = 30;
        sleekLabel.sizeScale_X = 0.5f;
        sleekLabel.fontAlignment = TextAnchor.UpperLeft;
        sleekLabel.text = PlayerDashboardSkillsUI.localization.format("Skill", PlayerDashboardSkillsUI.localization.format("Speciality_" + speciality + "_Skill_" + index), PlayerDashboardSkillsUI.localization.format("Level_" + skill.level));
        sleekLabel.fontSize = ESleekFontSize.Medium;
        AddChild(sleekLabel);
        ISleekImage sleekImage2 = Glazier.Get().CreateImage();
        sleekImage2.positionOffset_X = 10;
        sleekImage2.positionOffset_Y = -10;
        sleekImage2.positionScale_Y = 0.5f;
        sleekImage2.sizeOffset_X = 20;
        sleekImage2.sizeOffset_Y = 20;
        sleekImage2.color = ESleekTint.FOREGROUND;
        for (byte b2 = 0; b2 < PlayerSkills.SKILLSETS.Length; b2 = (byte)(b2 + 1))
        {
            for (byte b3 = 0; b3 < PlayerSkills.SKILLSETS[b2].Length; b3 = (byte)(b3 + 1))
            {
                SpecialitySkillPair specialitySkillPair = PlayerSkills.SKILLSETS[b2][b3];
                if (speciality == specialitySkillPair.speciality && index == specialitySkillPair.skill)
                {
                    sleekImage2.texture = MenuSurvivorsCharacterUI.icons.load<Texture2D>("Skillset_" + b2);
                    break;
                }
            }
        }
        AddChild(sleekImage2);
        ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
        sleekLabel2.positionOffset_X = 5;
        sleekLabel2.positionOffset_Y = -35;
        sleekLabel2.positionScale_Y = 1f;
        sleekLabel2.sizeOffset_X = -10;
        sleekLabel2.sizeOffset_Y = 30;
        sleekLabel2.sizeScale_X = 0.5f;
        sleekLabel2.fontAlignment = TextAnchor.LowerLeft;
        sleekLabel2.text = PlayerDashboardSkillsUI.localization.format("Speciality_" + speciality + "_Skill_" + index + "_Tooltip");
        AddChild(sleekLabel2);
        if (skill.level > 0)
        {
            ISleekLabel sleekLabel3 = Glazier.Get().CreateLabel();
            sleekLabel3.positionOffset_X = 5;
            sleekLabel3.positionOffset_Y = 5;
            sleekLabel3.positionScale_X = 0.25f;
            sleekLabel3.sizeOffset_X = -10;
            sleekLabel3.sizeOffset_Y = -10;
            sleekLabel3.sizeScale_X = 0.5f;
            sleekLabel3.sizeScale_Y = 0.5f;
            sleekLabel3.fontAlignment = TextAnchor.MiddleCenter;
            sleekLabel3.text = PlayerDashboardSkillsUI.localization.format("Bonus_Current", PlayerDashboardSkillsUI.localization.format("Speciality_" + speciality + "_Skill_" + index + "_Level_" + skill.level));
            AddChild(sleekLabel3);
        }
        if (skill.level < skill.GetClampedMaxUnlockableLevel())
        {
            ISleekLabel sleekLabel4 = Glazier.Get().CreateLabel();
            sleekLabel4.positionOffset_X = 5;
            sleekLabel4.positionOffset_Y = 5;
            sleekLabel4.positionScale_X = 0.25f;
            sleekLabel4.positionScale_Y = 0.5f;
            sleekLabel4.sizeOffset_X = -10;
            sleekLabel4.sizeOffset_Y = -10;
            sleekLabel4.sizeScale_X = 0.5f;
            sleekLabel4.sizeScale_Y = 0.5f;
            sleekLabel4.fontAlignment = TextAnchor.MiddleCenter;
            sleekLabel4.text = PlayerDashboardSkillsUI.localization.format("Bonus_Next", PlayerDashboardSkillsUI.localization.format("Speciality_" + speciality + "_Skill_" + index + "_Level_" + (skill.level + 1)));
            AddChild(sleekLabel4);
        }
        ISleekLabel sleekLabel5 = Glazier.Get().CreateLabel();
        sleekLabel5.positionOffset_X = 5;
        sleekLabel5.positionOffset_Y = -35;
        sleekLabel5.positionScale_X = 0.5f;
        sleekLabel5.positionScale_Y = 1f;
        sleekLabel5.sizeOffset_X = -10;
        sleekLabel5.sizeOffset_Y = 30;
        sleekLabel5.sizeScale_X = 0.5f;
        sleekLabel5.fontAlignment = TextAnchor.LowerRight;
        if (skill.level < skill.GetClampedMaxUnlockableLevel())
        {
            sleekLabel5.text = PlayerDashboardSkillsUI.localization.format("Cost", num);
        }
        else
        {
            sleekLabel5.text = PlayerDashboardSkillsUI.localization.format("Full");
        }
        AddChild(sleekLabel5);
    }

    private void onClickedInternalButton(ISleekElement internalButton)
    {
        this.onClickedButton?.Invoke(this);
    }
}
