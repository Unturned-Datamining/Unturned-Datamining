using UnityEngine;

namespace SDG.Unturned;

public class SleekServer : SleekWrapper
{
    private SteamServerInfo info;

    private SleekButtonIcon favoriteButton;

    private ISleekButton button;

    private ISleekBox mapBox;

    private ISleekBox playersBox;

    private ISleekBox pingBox;

    private SleekWebImage thumbnail;

    private ISleekLabel nameLabel;

    private ISleekLabel descLabel;

    public ClickedServer onClickedServer;

    public bool isCurrentlyFavorited => Provider.GetServerIsFavorited(info.ip, info.queryPort);

    private void onClickedFavoriteOffButton(ISleekElement button)
    {
        Provider.SetServerIsFavorited(info.ip, info.connectionPort, info.queryPort, !isCurrentlyFavorited);
        refreshFavoriteButton();
    }

    private void refreshFavoriteButton()
    {
        if (isCurrentlyFavorited)
        {
            button.isClickable = true;
            favoriteButton.tooltip = MenuPlayServersUI.localization.format("Favorite_Off_Button_Tooltip");
            favoriteButton.icon = MenuPlayServersUI.icons.load<Texture2D>("Favorite_Off");
        }
        else
        {
            button.isClickable = false;
            favoriteButton.tooltip = MenuPlayServersUI.localization.format("Favorite_On_Button_Tooltip");
            favoriteButton.icon = MenuPlayServersUI.icons.load<Texture2D>("Favorite_On");
        }
    }

    private void onClickedButton(ISleekElement button)
    {
        if (onClickedServer != null)
        {
            onClickedServer(this, info);
        }
    }

    public SleekServer(ESteamServerList list, SteamServerInfo newInfo)
    {
        info = newInfo;
        button = Glazier.Get().CreateButton();
        button.sizeOffset_X = -240;
        button.sizeScale_X = 1f;
        button.sizeScale_Y = 1f;
        button.onClickedButton += onClickedButton;
        AddChild(button);
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.positionOffset_X = 45;
        nameLabel.sizeScale_X = 1f;
        nameLabel.sizeOffset_X = -45;
        nameLabel.fontAlignment = TextAnchor.MiddleLeft;
        nameLabel.text = info.name;
        button.AddChild(nameLabel);
        if (string.IsNullOrEmpty(info.descText))
        {
            nameLabel.sizeOffset_Y = 40;
        }
        else
        {
            nameLabel.sizeOffset_Y = 30;
            descLabel = Glazier.Get().CreateLabel();
            descLabel.positionOffset_X = 45;
            descLabel.positionOffset_Y = 15;
            descLabel.sizeScale_X = 1f;
            descLabel.sizeOffset_X = -45;
            descLabel.sizeOffset_Y = 30;
            descLabel.fontSize = ESleekFontSize.Small;
            descLabel.enableRichText = true;
            descLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
            descLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            descLabel.fontAlignment = TextAnchor.MiddleLeft;
            descLabel.text = info.descText;
            button.AddChild(descLabel);
        }
        mapBox = Glazier.Get().CreateBox();
        mapBox.positionOffset_X = 10;
        mapBox.positionScale_X = 1f;
        mapBox.sizeOffset_X = 100;
        mapBox.sizeScale_Y = 1f;
        mapBox.text = info.map;
        button.AddChild(mapBox);
        playersBox = Glazier.Get().CreateBox();
        playersBox.positionOffset_X = 120;
        playersBox.positionScale_X = 1f;
        playersBox.sizeOffset_X = 60;
        playersBox.sizeScale_Y = 1f;
        playersBox.text = MenuPlayServersUI.localization.format("Server_Players", info.players, info.maxPlayers);
        button.AddChild(playersBox);
        pingBox = Glazier.Get().CreateBox();
        pingBox.positionOffset_X = 190;
        pingBox.positionScale_X = 1f;
        pingBox.sizeOffset_X = 50;
        pingBox.sizeScale_Y = 1f;
        pingBox.text = info.ping.ToString();
        button.AddChild(pingBox);
        if (!string.IsNullOrEmpty(info.thumbnailURL))
        {
            thumbnail = new SleekWebImage();
            thumbnail.positionOffset_X = 4;
            thumbnail.positionOffset_Y = 4;
            thumbnail.sizeOffset_X = 32;
            thumbnail.sizeOffset_Y = 32;
            thumbnail.Refresh(info.thumbnailURL);
            button.AddChild(thumbnail);
        }
        if (false)
        {
            if (info.isPassworded)
            {
                ISleekImage sleekImage = Glazier.Get().CreateImage();
                sleekImage.positionOffset_X = 5;
                sleekImage.positionOffset_Y = 5;
                sleekImage.sizeOffset_X = 20;
                sleekImage.sizeOffset_Y = 20;
                sleekImage.texture = MenuPlayServersUI.icons.load<Texture2D>("Lock");
                button.AddChild(sleekImage);
            }
            if (info.isWorkshop)
            {
                ISleekImage sleekImage2 = Glazier.Get().CreateImage();
                sleekImage2.positionOffset_X = 35;
                sleekImage2.positionOffset_Y = 5;
                sleekImage2.sizeOffset_X = 20;
                sleekImage2.sizeOffset_Y = 20;
                sleekImage2.texture = MenuPlayServersUI.icons.load<Texture2D>("Workshop");
                button.AddChild(sleekImage2);
            }
            ISleekImage sleekImage3 = Glazier.Get().CreateImage();
            sleekImage3.positionOffset_X = -145;
            sleekImage3.positionOffset_Y = 5;
            sleekImage3.positionScale_X = 1f;
            sleekImage3.sizeOffset_X = 20;
            sleekImage3.sizeOffset_Y = 20;
            button.AddChild(sleekImage3);
            if (info.mode == EGameMode.EASY)
            {
                sleekImage3.texture = MenuPlayServersUI.icons.load<Texture2D>("Easy");
            }
            else if (info.mode == EGameMode.NORMAL)
            {
                sleekImage3.texture = MenuPlayServersUI.icons.load<Texture2D>("Normal");
            }
            else if (info.mode == EGameMode.HARD)
            {
                sleekImage3.texture = MenuPlayServersUI.icons.load<Texture2D>("Hard");
            }
            if (info.cameraMode == ECameraMode.FIRST)
            {
                ISleekImage sleekImage4 = Glazier.Get().CreateImage();
                sleekImage4.positionOffset_X = -115;
                sleekImage4.positionOffset_Y = 5;
                sleekImage4.positionScale_X = 1f;
                sleekImage4.sizeOffset_X = 20;
                sleekImage4.sizeOffset_Y = 20;
                sleekImage4.texture = MenuPlayServersUI.icons.load<Texture2D>("First");
                button.AddChild(sleekImage4);
            }
            else if (info.cameraMode == ECameraMode.THIRD)
            {
                ISleekImage sleekImage5 = Glazier.Get().CreateImage();
                sleekImage5.positionOffset_X = -115;
                sleekImage5.positionOffset_Y = 5;
                sleekImage5.positionScale_X = 1f;
                sleekImage5.sizeOffset_X = 20;
                sleekImage5.sizeOffset_Y = 20;
                sleekImage5.texture = MenuPlayServersUI.icons.load<Texture2D>("Third");
                button.AddChild(sleekImage5);
            }
            else if (info.cameraMode == ECameraMode.BOTH)
            {
                ISleekImage sleekImage6 = Glazier.Get().CreateImage();
                sleekImage6.positionOffset_X = -115;
                sleekImage6.positionOffset_Y = 5;
                sleekImage6.positionScale_X = 1f;
                sleekImage6.sizeOffset_X = 20;
                sleekImage6.sizeOffset_Y = 20;
                sleekImage6.texture = MenuPlayServersUI.icons.load<Texture2D>("Both");
                button.AddChild(sleekImage6);
            }
            else if (info.cameraMode == ECameraMode.VEHICLE)
            {
                ISleekImage sleekImage7 = Glazier.Get().CreateImage();
                sleekImage7.positionOffset_X = -115;
                sleekImage7.positionOffset_Y = 5;
                sleekImage7.positionScale_X = 1f;
                sleekImage7.sizeOffset_X = 20;
                sleekImage7.sizeOffset_Y = 20;
                sleekImage7.texture = MenuPlayServersUI.icons.load<Texture2D>("Vehicle");
                button.AddChild(sleekImage7);
            }
            if (info.isPvP)
            {
                ISleekImage sleekImage8 = Glazier.Get().CreateImage();
                sleekImage8.positionOffset_X = -85;
                sleekImage8.positionOffset_Y = 5;
                sleekImage8.positionScale_X = 1f;
                sleekImage8.sizeOffset_X = 20;
                sleekImage8.sizeOffset_Y = 20;
                sleekImage8.texture = MenuPlayServersUI.icons.load<Texture2D>("PvP");
                button.AddChild(sleekImage8);
            }
            else
            {
                ISleekImage sleekImage9 = Glazier.Get().CreateImage();
                sleekImage9.positionOffset_X = -85;
                sleekImage9.positionOffset_Y = 5;
                sleekImage9.positionScale_X = 1f;
                sleekImage9.sizeOffset_X = 20;
                sleekImage9.sizeOffset_Y = 20;
                sleekImage9.texture = MenuPlayServersUI.icons.load<Texture2D>("PvE");
                button.AddChild(sleekImage9);
            }
            if (info.IsBattlEyeSecure)
            {
                ISleekImage sleekImage10 = Glazier.Get().CreateImage();
                sleekImage10.positionOffset_X = -55;
                sleekImage10.positionOffset_Y = 5;
                sleekImage10.positionScale_X = 1f;
                sleekImage10.sizeOffset_X = 20;
                sleekImage10.sizeOffset_Y = 20;
                sleekImage10.texture = MenuPlayServersUI.icons.load<Texture2D>("BattlEye");
                button.AddChild(sleekImage10);
            }
            if (info.IsVACSecure)
            {
                ISleekImage sleekImage11 = Glazier.Get().CreateImage();
                sleekImage11.positionOffset_X = -25;
                sleekImage11.positionOffset_Y = 5;
                sleekImage11.positionScale_X = 1f;
                sleekImage11.sizeOffset_X = 20;
                sleekImage11.sizeOffset_Y = 20;
                sleekImage11.texture = MenuPlayServersUI.icons.load<Texture2D>("VAC");
                button.AddChild(sleekImage11);
            }
            if (info.isPro)
            {
                SleekColor backgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
                button.backgroundColor = backgroundColor;
                button.textColor = Palette.PRO;
                button.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
                mapBox.backgroundColor = backgroundColor;
                mapBox.textColor = Palette.PRO;
                mapBox.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
                playersBox.backgroundColor = backgroundColor;
                playersBox.textColor = Palette.PRO;
                playersBox.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
                pingBox.backgroundColor = backgroundColor;
                pingBox.textColor = Palette.PRO;
                pingBox.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            }
        }
        if (list == ESteamServerList.FAVORITES)
        {
            button.positionOffset_X += 40;
            button.sizeOffset_X -= 40;
            favoriteButton = new SleekButtonIcon(null);
            favoriteButton.sizeOffset_X = 40;
            favoriteButton.sizeScale_Y = 1f;
            favoriteButton.iconPositionOffset = 10;
            favoriteButton.iconColor = ESleekTint.FOREGROUND;
            favoriteButton.onClickedButton += onClickedFavoriteOffButton;
            AddChild(favoriteButton);
            refreshFavoriteButton();
        }
    }
}
