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
        skillsScrollBox.ContentSizeOffset = new Vector2(0f, skills.Length * 90 - 10);
        for (byte b = 0; b < skills.Length; b = (byte)(b + 1))
        {
            Skill skill = skills[b];
            SleekSkill sleekSkill = new SleekSkill(specialityIndex, b, skill);
            sleekSkill.PositionOffset_Y = b * 90;
            sleekSkill.SizeOffset_Y = 80f;
            sleekSkill.SizeScale_X = 1f;
            sleekSkill.onClickedButton += onClickedSkillButton;
            skillsScrollBox.AddChild(sleekSkill);
        }
        if (boostButton != null)
        {
            backdropBox.RemoveChild(boostButton);
        }
        boostButton = new SleekBoost((byte)Player.player.skills.boost);
        boostButton.PositionOffset_X = 5f;
        boostButton.PositionOffset_Y = -90f;
        boostButton.PositionScale_X = 0.5f;
        boostButton.PositionScale_Y = 1f;
        boostButton.SizeOffset_X = -15f;
        boostButton.SizeOffset_Y = 80f;
        boostButton.SizeScale_X = 0.5f;
        boostButton.onClickedButton += onClickedBoostButton;
        backdropBox.AddChild(boostButton);
        selectedSpeciality = specialityIndex;
    }

    private static void onClickedSpecialityButton(ISleekElement button)
    {
        updateSelection((byte)((button.PositionOffset_X + 85f) / 60f));
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
        byte b = (byte)(button.PositionOffset_Y / 90f);
        if (skills[b].level < skills[b].GetClampedMaxUnlockableLevel() && Player.player.skills.experience >= Player.player.skills.cost(selectedSpeciality, b))
        {
            Player.player.skills.sendUpgrade(selectedSpeciality, b, InputEx.GetKey(ControlsSettings.other));
        }
    }

    private static void onExperienceUpdated(uint newExperience)
    {
        experienceBox.Text = localization.format("Experience", newExperience.ToString());
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
        container.PositionScale_Y = 1f;
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        selectedSpeciality = byte.MaxValue;
        backdropBox = Glazier.Get().CreateBox();
        backdropBox.PositionOffset_Y = 60f;
        backdropBox.SizeOffset_Y = -60f;
        backdropBox.SizeScale_X = 1f;
        backdropBox.SizeScale_Y = 1f;
        backdropBox.BackgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
        container.AddChild(backdropBox);
        experienceBox = Glazier.Get().CreateBox();
        experienceBox.PositionOffset_X = 10f;
        experienceBox.PositionOffset_Y = -90f;
        experienceBox.PositionScale_Y = 1f;
        experienceBox.SizeOffset_X = -15f;
        experienceBox.SizeOffset_Y = 80f;
        experienceBox.SizeScale_X = 0.5f;
        experienceBox.FontSize = ESleekFontSize.Medium;
        backdropBox.AddChild(experienceBox);
        for (int i = 0; i < PlayerSkills.SPECIALITIES; i++)
        {
            SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(icons.load<Texture2D>("Speciality_" + i));
            sleekButtonIcon.PositionOffset_X = -85 + i * 60;
            sleekButtonIcon.PositionOffset_Y = 10f;
            sleekButtonIcon.PositionScale_X = 0.5f;
            sleekButtonIcon.SizeOffset_X = 50f;
            sleekButtonIcon.SizeOffset_Y = 50f;
            sleekButtonIcon.tooltip = localization.format("Speciality_" + i + "_Tooltip");
            sleekButtonIcon.iconColor = ESleekTint.FOREGROUND;
            sleekButtonIcon.onClickedButton += onClickedSpecialityButton;
            backdropBox.AddChild(sleekButtonIcon);
        }
        skillsScrollBox = Glazier.Get().CreateScrollView();
        skillsScrollBox.PositionOffset_X = 10f;
        skillsScrollBox.PositionOffset_Y = 70f;
        skillsScrollBox.SizeOffset_X = -20f;
        skillsScrollBox.SizeOffset_Y = -170f;
        skillsScrollBox.SizeScale_X = 1f;
        skillsScrollBox.SizeScale_Y = 1f;
        skillsScrollBox.ScaleContentToWidth = true;
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
