using SDG.HostBans;
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
            button.IsClickable = true;
            favoriteButton.tooltip = MenuPlayServersUI.localization.format("Favorite_Off_Button_Tooltip");
            favoriteButton.icon = MenuPlayServersUI.icons.load<Texture2D>("Favorite_Off");
        }
        else
        {
            button.IsClickable = false;
            favoriteButton.tooltip = MenuPlayServersUI.localization.format("Favorite_On_Button_Tooltip");
            favoriteButton.icon = MenuPlayServersUI.icons.load<Texture2D>("Favorite_On");
        }
    }

    private void onClickedButton(ISleekElement button)
    {
        onClickedServer?.Invoke(this, info);
    }

    public SleekServer(ESteamServerList list, SteamServerInfo newInfo)
    {
        info = newInfo;
        button = Glazier.Get().CreateButton();
        button.SizeOffset_X = -240f;
        button.SizeScale_X = 1f;
        button.SizeScale_Y = 1f;
        button.OnClicked += onClickedButton;
        AddChild(button);
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.PositionOffset_X = 45f;
        nameLabel.SizeScale_X = 1f;
        nameLabel.SizeOffset_X = -45f;
        nameLabel.TextAlignment = TextAnchor.MiddleLeft;
        nameLabel.Text = info.name;
        button.AddChild(nameLabel);
        if (string.IsNullOrEmpty(info.descText))
        {
            nameLabel.SizeOffset_Y = 40f;
        }
        else
        {
            nameLabel.SizeOffset_Y = 30f;
            descLabel = Glazier.Get().CreateLabel();
            descLabel.PositionOffset_X = 45f;
            descLabel.PositionOffset_Y = 15f;
            descLabel.SizeScale_X = 1f;
            descLabel.SizeOffset_X = -45f;
            descLabel.SizeOffset_Y = 30f;
            descLabel.FontSize = ESleekFontSize.Small;
            descLabel.AllowRichText = true;
            descLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
            descLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            descLabel.TextAlignment = TextAnchor.MiddleLeft;
            descLabel.Text = info.descText;
            button.AddChild(descLabel);
        }
        mapBox = Glazier.Get().CreateBox();
        mapBox.PositionOffset_X = 10f;
        mapBox.PositionScale_X = 1f;
        mapBox.SizeOffset_X = 100f;
        mapBox.SizeScale_Y = 1f;
        mapBox.Text = info.map;
        button.AddChild(mapBox);
        playersBox = Glazier.Get().CreateBox();
        playersBox.PositionOffset_X = 120f;
        playersBox.PositionScale_X = 1f;
        playersBox.SizeOffset_X = 60f;
        playersBox.SizeScale_Y = 1f;
        playersBox.Text = MenuPlayServersUI.localization.format("Server_Players", info.players, info.maxPlayers);
        button.AddChild(playersBox);
        pingBox = Glazier.Get().CreateBox();
        pingBox.PositionOffset_X = 190f;
        pingBox.PositionScale_X = 1f;
        pingBox.SizeOffset_X = 50f;
        pingBox.SizeScale_Y = 1f;
        pingBox.Text = info.ping.ToString();
        if (info.hostBanFlags.HasFlag(EHostBanFlags.QueryPingWarning))
        {
            pingBox.TextColor = ESleekTint.BAD;
            if (MenuPlayServerInfoUI.localization != null)
            {
                pingBox.TooltipText = MenuPlayServerInfoUI.localization.format("HostBan_QueryPingWarning");
            }
        }
        button.AddChild(pingBox);
        if (!string.IsNullOrEmpty(info.thumbnailURL))
        {
            thumbnail = new SleekWebImage();
            thumbnail.PositionOffset_X = 4f;
            thumbnail.PositionOffset_Y = 4f;
            thumbnail.SizeOffset_X = 32f;
            thumbnail.SizeOffset_Y = 32f;
            thumbnail.Refresh(info.thumbnailURL);
            button.AddChild(thumbnail);
        }
        if (false)
        {
            if (info.isPassworded)
            {
                ISleekImage sleekImage = Glazier.Get().CreateImage();
                sleekImage.PositionOffset_X = 5f;
                sleekImage.PositionOffset_Y = 5f;
                sleekImage.SizeOffset_X = 20f;
                sleekImage.SizeOffset_Y = 20f;
                sleekImage.Texture = MenuPlayServersUI.icons.load<Texture2D>("Lock");
                button.AddChild(sleekImage);
            }
            if (info.isWorkshop)
            {
                ISleekImage sleekImage2 = Glazier.Get().CreateImage();
                sleekImage2.PositionOffset_X = 35f;
                sleekImage2.PositionOffset_Y = 5f;
                sleekImage2.SizeOffset_X = 20f;
                sleekImage2.SizeOffset_Y = 20f;
                sleekImage2.Texture = MenuPlayServersUI.icons.load<Texture2D>("Workshop");
                button.AddChild(sleekImage2);
            }
            ISleekImage sleekImage3 = Glazier.Get().CreateImage();
            sleekImage3.PositionOffset_X = -145f;
            sleekImage3.PositionOffset_Y = 5f;
            sleekImage3.PositionScale_X = 1f;
            sleekImage3.SizeOffset_X = 20f;
            sleekImage3.SizeOffset_Y = 20f;
            button.AddChild(sleekImage3);
            if (info.mode == EGameMode.EASY)
            {
                sleekImage3.Texture = MenuPlayServersUI.icons.load<Texture2D>("Easy");
            }
            else if (info.mode == EGameMode.NORMAL)
            {
                sleekImage3.Texture = MenuPlayServersUI.icons.load<Texture2D>("Normal");
            }
            else if (info.mode == EGameMode.HARD)
            {
                sleekImage3.Texture = MenuPlayServersUI.icons.load<Texture2D>("Hard");
            }
            if (info.cameraMode == ECameraMode.FIRST)
            {
                ISleekImage sleekImage4 = Glazier.Get().CreateImage();
                sleekImage4.PositionOffset_X = -115f;
                sleekImage4.PositionOffset_Y = 5f;
                sleekImage4.PositionScale_X = 1f;
                sleekImage4.SizeOffset_X = 20f;
                sleekImage4.SizeOffset_Y = 20f;
                sleekImage4.Texture = MenuPlayServersUI.icons.load<Texture2D>("First");
                button.AddChild(sleekImage4);
            }
            else if (info.cameraMode == ECameraMode.THIRD)
            {
                ISleekImage sleekImage5 = Glazier.Get().CreateImage();
                sleekImage5.PositionOffset_X = -115f;
                sleekImage5.PositionOffset_Y = 5f;
                sleekImage5.PositionScale_X = 1f;
                sleekImage5.SizeOffset_X = 20f;
                sleekImage5.SizeOffset_Y = 20f;
                sleekImage5.Texture = MenuPlayServersUI.icons.load<Texture2D>("Third");
                button.AddChild(sleekImage5);
            }
            else if (info.cameraMode == ECameraMode.BOTH)
            {
                ISleekImage sleekImage6 = Glazier.Get().CreateImage();
                sleekImage6.PositionOffset_X = -115f;
                sleekImage6.PositionOffset_Y = 5f;
                sleekImage6.PositionScale_X = 1f;
                sleekImage6.SizeOffset_X = 20f;
                sleekImage6.SizeOffset_Y = 20f;
                sleekImage6.Texture = MenuPlayServersUI.icons.load<Texture2D>("Both");
                button.AddChild(sleekImage6);
            }
            else if (info.cameraMode == ECameraMode.VEHICLE)
            {
                ISleekImage sleekImage7 = Glazier.Get().CreateImage();
                sleekImage7.PositionOffset_X = -115f;
                sleekImage7.PositionOffset_Y = 5f;
                sleekImage7.PositionScale_X = 1f;
                sleekImage7.SizeOffset_X = 20f;
                sleekImage7.SizeOffset_Y = 20f;
                sleekImage7.Texture = MenuPlayServersUI.icons.load<Texture2D>("Vehicle");
                button.AddChild(sleekImage7);
            }
            if (info.isPvP)
            {
                ISleekImage sleekImage8 = Glazier.Get().CreateImage();
                sleekImage8.PositionOffset_X = -85f;
                sleekImage8.PositionOffset_Y = 5f;
                sleekImage8.PositionScale_X = 1f;
                sleekImage8.SizeOffset_X = 20f;
                sleekImage8.SizeOffset_Y = 20f;
                sleekImage8.Texture = MenuPlayServersUI.icons.load<Texture2D>("PvP");
                button.AddChild(sleekImage8);
            }
            else
            {
                ISleekImage sleekImage9 = Glazier.Get().CreateImage();
                sleekImage9.PositionOffset_X = -85f;
                sleekImage9.PositionOffset_Y = 5f;
                sleekImage9.PositionScale_X = 1f;
                sleekImage9.SizeOffset_X = 20f;
                sleekImage9.SizeOffset_Y = 20f;
                sleekImage9.Texture = MenuPlayServersUI.icons.load<Texture2D>("PvE");
                button.AddChild(sleekImage9);
            }
            if (info.IsBattlEyeSecure)
            {
                ISleekImage sleekImage10 = Glazier.Get().CreateImage();
                sleekImage10.PositionOffset_X = -55f;
                sleekImage10.PositionOffset_Y = 5f;
                sleekImage10.PositionScale_X = 1f;
                sleekImage10.SizeOffset_X = 20f;
                sleekImage10.SizeOffset_Y = 20f;
                sleekImage10.Texture = MenuPlayServersUI.icons.load<Texture2D>("BattlEye");
                button.AddChild(sleekImage10);
            }
            if (info.IsVACSecure)
            {
                ISleekImage sleekImage11 = Glazier.Get().CreateImage();
                sleekImage11.PositionOffset_X = -25f;
                sleekImage11.PositionOffset_Y = 5f;
                sleekImage11.PositionScale_X = 1f;
                sleekImage11.SizeOffset_X = 20f;
                sleekImage11.SizeOffset_Y = 20f;
                sleekImage11.Texture = MenuPlayServersUI.icons.load<Texture2D>("VAC");
                button.AddChild(sleekImage11);
            }
            if (info.isPro)
            {
                SleekColor backgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
                button.BackgroundColor = backgroundColor;
                button.TextColor = Palette.PRO;
                button.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
                mapBox.BackgroundColor = backgroundColor;
                mapBox.TextColor = Palette.PRO;
                mapBox.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
                playersBox.BackgroundColor = backgroundColor;
                playersBox.TextColor = Palette.PRO;
                playersBox.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
                pingBox.BackgroundColor = backgroundColor;
                pingBox.TextColor = Palette.PRO;
                pingBox.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            }
        }
        if (list == ESteamServerList.FAVORITES)
        {
            button.PositionOffset_X += 40f;
            button.SizeOffset_X -= 40f;
            favoriteButton = new SleekButtonIcon(null);
            favoriteButton.SizeOffset_X = 40f;
            favoriteButton.SizeScale_Y = 1f;
            favoriteButton.iconPositionOffset = 10;
            favoriteButton.iconColor = ESleekTint.FOREGROUND;
            favoriteButton.onClickedButton += onClickedFavoriteOffButton;
            AddChild(favoriteButton);
            refreshFavoriteButton();
        }
    }
}
