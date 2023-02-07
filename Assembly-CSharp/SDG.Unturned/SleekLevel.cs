using UnityEngine;
using Unturned.LiveConfig;

namespace SDG.Unturned;

public class SleekLevel : SleekWrapper
{
    public ClickedLevel onClickedLevel;

    private bool hasCreatedStatusLabel;

    private LevelInfo level;

    private ISleekButton button;

    private ISleekImage icon;

    private ISleekLabel nameLabel;

    private ISleekLabel infoLabel;

    private void onClickedButton(ISleekElement button)
    {
        onClickedLevel?.Invoke(this, (byte)(base.positionOffset_Y / 110));
    }

    private void OnLiveConfigRefreshed()
    {
        if (hasCreatedStatusLabel)
        {
            return;
        }
        MainMenuWorkshopFeaturedLiveConfig featured = LiveConfig.Get().MainMenuWorkshop.Featured;
        if (featured.Status != 0 && featured.IsFeatured(level.publishedFileId))
        {
            SleekNew sleekNew = new SleekNew(featured.Status == EMapStatus.Updated);
            if (icon != null)
            {
                icon.AddChild(sleekNew);
            }
            else
            {
                AddChild(sleekNew);
            }
            hasCreatedStatusLabel = true;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        LiveConfig.OnRefreshed -= OnLiveConfigRefreshed;
    }

    public SleekLevel(LevelInfo level, bool isEditor)
    {
        this.level = level;
        base.sizeOffset_X = 400;
        base.sizeOffset_Y = 100;
        button = Glazier.Get().CreateButton();
        button.sizeOffset_X = 0;
        button.sizeOffset_Y = 0;
        button.sizeScale_X = 1f;
        button.sizeScale_Y = 1f;
        if (level.isEditable || !isEditor)
        {
            button.onClickedButton += onClickedButton;
        }
        AddChild(button);
        string text = level.path + "/Icon.png";
        if (ReadWrite.fileExists(text, useCloud: false, usePath: false))
        {
            icon = Glazier.Get().CreateImage();
            icon.positionOffset_X = 10;
            icon.positionOffset_Y = 10;
            icon.sizeOffset_X = -20;
            icon.sizeOffset_Y = -20;
            icon.sizeScale_X = 1f;
            icon.sizeScale_Y = 1f;
            icon.texture = ReadWrite.readTextureFromFile(text);
            icon.shouldDestroyTexture = true;
            button.AddChild(icon);
        }
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.positionOffset_Y = 10;
        nameLabel.sizeScale_X = 1f;
        nameLabel.sizeOffset_Y = 50;
        nameLabel.fontAlignment = TextAnchor.MiddleCenter;
        nameLabel.fontSize = ESleekFontSize.Medium;
        nameLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        button.AddChild(nameLabel);
        Local localization = level.getLocalization();
        if (localization != null && localization.has("Name"))
        {
            nameLabel.text = localization.format("Name");
        }
        else
        {
            nameLabel.text = level.name;
        }
        infoLabel = Glazier.Get().CreateLabel();
        infoLabel.positionOffset_Y = 60;
        infoLabel.sizeScale_X = 1f;
        infoLabel.sizeOffset_Y = 30;
        infoLabel.fontAlignment = TextAnchor.MiddleCenter;
        infoLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        string arg = "#SIZE";
        if (level.size == ELevelSize.TINY)
        {
            arg = MenuPlaySingleplayerUI.localization.format("Tiny");
        }
        else if (level.size == ELevelSize.SMALL)
        {
            arg = MenuPlaySingleplayerUI.localization.format("Small");
        }
        else if (level.size == ELevelSize.MEDIUM)
        {
            arg = MenuPlaySingleplayerUI.localization.format("Medium");
        }
        else if (level.size == ELevelSize.LARGE)
        {
            arg = MenuPlaySingleplayerUI.localization.format("Large");
        }
        else if (level.size == ELevelSize.INSANE)
        {
            arg = MenuPlaySingleplayerUI.localization.format("Insane");
        }
        string arg2 = "#TYPE";
        if (localization != null && localization.has("GameModeLabel"))
        {
            arg2 = localization.format("GameModeLabel");
        }
        else if (level.type == ELevelType.SURVIVAL)
        {
            arg2 = MenuPlaySingleplayerUI.localization.format("Survival");
        }
        else if (level.type == ELevelType.HORDE)
        {
            arg2 = MenuPlaySingleplayerUI.localization.format("Horde");
        }
        else if (level.type == ELevelType.ARENA)
        {
            arg2 = MenuPlaySingleplayerUI.localization.format("Arena");
        }
        infoLabel.text = MenuPlaySingleplayerUI.localization.format("Info_WithVersion", arg, arg2, level.configData.Version);
        infoLabel.textColor = new SleekColor(ESleekTint.FONT, 0.75f);
        button.AddChild(infoLabel);
        if (!level.isEditable && isEditor)
        {
            Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Workshop/MenuWorkshopEditor/MenuWorkshopEditor.unity3d");
            ISleekImage sleekImage = Glazier.Get().CreateImage();
            sleekImage.positionOffset_X = 20;
            sleekImage.positionOffset_Y = -20;
            sleekImage.positionScale_Y = 0.5f;
            sleekImage.sizeOffset_X = 40;
            sleekImage.sizeOffset_Y = 40;
            sleekImage.texture = bundle.load<Texture2D>("Lock");
            sleekImage.color = ESleekTint.FOREGROUND;
            button.AddChild(sleekImage);
            bundle.unload();
        }
        hasCreatedStatusLabel = false;
        LiveConfig.OnRefreshed -= OnLiveConfigRefreshed;
        OnLiveConfigRefreshed();
    }
}
