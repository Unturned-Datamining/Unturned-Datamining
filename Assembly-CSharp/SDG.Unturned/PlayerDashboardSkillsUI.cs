using System;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerDashboardSkillsUI
{
    public static Local localization;

    public static Bundle icons;

    private static SleekFullscreenBox container;

    public static bool active;

    private static ISleekBox backdropBox;

    private static Skill[] skills;

    private static ISleekScrollView skillsScrollBox;

    private static SleekBoost boostButton;

    private static ISleekBox experienceBox;

    private static byte selectedSpeciality;

    public static void open()
    {
        if (!active)
        {
            active = true;
            updateSelection(selectedSpeciality);
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

    private static void updateSelection(byte specialityIndex)
    {
        skills = Player.player.skills.skills[specialityIndex];
        skillsScrollBox.RemoveAllChildren();
        skillsScrollBox.contentSizeOffset = new Vector2(0f, skills.Length * 90 - 10);
        for (byte b = 0; b < skills.Length; b = (byte)(b + 1))
        {
            Skill skill = skills[b];
            SleekSkill sleekSkill = new SleekSkill(specialityIndex, b, skill);
            sleekSkill.positionOffset_Y = b * 90;
            sleekSkill.sizeOffset_Y = 80;
            sleekSkill.sizeScale_X = 1f;
            sleekSkill.onClickedButton += onClickedSkillButton;
            skillsScrollBox.AddChild(sleekSkill);
        }
        if (boostButton != null)
        {
            backdropBox.RemoveChild(boostButton);
        }
        boostButton = new SleekBoost((byte)Player.player.skills.boost);
        boostButton.positionOffset_X = 5;
        boostButton.positionOffset_Y = -90;
        boostButton.positionScale_X = 0.5f;
        boostButton.positionScale_Y = 1f;
        boostButton.sizeOffset_X = -15;
        boostButton.sizeOffset_Y = 80;
        boostButton.sizeScale_X = 0.5f;
        boostButton.onClickedButton += onClickedBoostButton;
        backdropBox.AddChild(boostButton);
        selectedSpeciality = specialityIndex;
    }

    private static void onClickedSpecialityButton(ISleekElement button)
    {
        updateSelection((byte)((button.positionOffset_X + 85) / 60));
    }

    private static void onClickedBoostButton(ISleekElement button)
    {
        if (Player.player.skills.experience >= PlayerSkills.BOOST_COST)
        {
            Player.player.skills.sendBoost();
        }
    }

    private static void onClickedSkillButton(ISleekElement button)
    {
        byte b = (byte)(button.positionOffset_Y / 90);
        if (skills[b].level < skills[b].GetClampedMaxUnlockableLevel() && Player.player.skills.experience >= Player.player.skills.cost(selectedSpeciality, b))
        {
            Player.player.skills.sendUpgrade(selectedSpeciality, b, InputEx.GetKey(ControlsSettings.other));
        }
    }

    private static void onExperienceUpdated(uint newExperience)
    {
        experienceBox.text = localization.format("Experience", newExperience.ToString());
    }

    private static void onBoostUpdated(EPlayerBoost newBoost)
    {
        if (active && PlayerDashboardUI.active)
        {
            updateSelection(selectedSpeciality);
        }
    }

    private static void onSkillsUpdated()
    {
        if (active && PlayerDashboardUI.active)
        {
            updateSelection(selectedSpeciality);
        }
    }

    public PlayerDashboardSkillsUI()
    {
        if (icons != null)
        {
            icons.unload();
        }
        localization = Localization.read("/Player/PlayerDashboardSkills.dat");
        icons = Bundles.getBundle("/Bundles/Textures/Player/Icons/PlayerDashboardSkills/PlayerDashboardSkills.unity3d");
        container = new SleekFullscreenBox();
        container.positionScale_Y = 1f;
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        selectedSpeciality = byte.MaxValue;
        backdropBox = Glazier.Get().CreateBox();
        backdropBox.positionOffset_Y = 60;
        backdropBox.sizeOffset_Y = -60;
        backdropBox.sizeScale_X = 1f;
        backdropBox.sizeScale_Y = 1f;
        backdropBox.backgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
        container.AddChild(backdropBox);
        experienceBox = Glazier.Get().CreateBox();
        experienceBox.positionOffset_X = 10;
        experienceBox.positionOffset_Y = -90;
        experienceBox.positionScale_Y = 1f;
        experienceBox.sizeOffset_X = -15;
        experienceBox.sizeOffset_Y = 80;
        experienceBox.sizeScale_X = 0.5f;
        experienceBox.fontSize = ESleekFontSize.Medium;
        backdropBox.AddChild(experienceBox);
        for (int i = 0; i < PlayerSkills.SPECIALITIES; i++)
        {
            SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(icons.load<Texture2D>("Speciality_" + i));
            sleekButtonIcon.positionOffset_X = -85 + i * 60;
            sleekButtonIcon.positionOffset_Y = 10;
            sleekButtonIcon.positionScale_X = 0.5f;
            sleekButtonIcon.sizeOffset_X = 50;
            sleekButtonIcon.sizeOffset_Y = 50;
            sleekButtonIcon.tooltip = localization.format("Speciality_" + i + "_Tooltip");
            sleekButtonIcon.iconColor = ESleekTint.FOREGROUND;
            sleekButtonIcon.onClickedButton += onClickedSpecialityButton;
            backdropBox.AddChild(sleekButtonIcon);
        }
        skillsScrollBox = Glazier.Get().CreateScrollView();
        skillsScrollBox.positionOffset_X = 10;
        skillsScrollBox.positionOffset_Y = 70;
        skillsScrollBox.sizeOffset_X = -20;
        skillsScrollBox.sizeOffset_Y = -170;
        skillsScrollBox.sizeScale_X = 1f;
        skillsScrollBox.sizeScale_Y = 1f;
        skillsScrollBox.scaleContentToWidth = true;
        backdropBox.AddChild(skillsScrollBox);
        boostButton = null;
        updateSelection(0);
        PlayerSkills playerSkills = Player.player.skills;
        playerSkills.onExperienceUpdated = (ExperienceUpdated)Delegate.Combine(playerSkills.onExperienceUpdated, new ExperienceUpdated(onExperienceUpdated));
        onExperienceUpdated(Player.player.skills.experience);
        PlayerSkills playerSkills2 = Player.player.skills;
        playerSkills2.onBoostUpdated = (BoostUpdated)Delegate.Combine(playerSkills2.onBoostUpdated, new BoostUpdated(onBoostUpdated));
        PlayerSkills playerSkills3 = Player.player.skills;
        playerSkills3.onSkillsUpdated = (SkillsUpdated)Delegate.Combine(playerSkills3.onSkillsUpdated, new SkillsUpdated(onSkillsUpdated));
    }
}
