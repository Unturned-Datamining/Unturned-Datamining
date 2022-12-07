using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SDG.Provider;
using Steamworks;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class MenuPlayServerInfoUI
{
    public enum EServerInfoOpenContext
    {
        CONNECT,
        SERVERS,
        MATCHMAKING
    }

    private class ServerInfoViewWorkshopButton : SleekWrapper
    {
        public PublishedFileId_t fileId;

        public ServerInfoViewWorkshopButton(PublishedFileId_t fileId, string name)
        {
            this.fileId = fileId;
            base.sizeOffset_X = 20;
            base.sizeOffset_Y = 20;
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.sizeScale_X = 1f;
            sleekButton.sizeScale_Y = 1f;
            sleekButton.onClickedButton += onClickedButton;
            sleekButton.tooltipText = MenuWorkshopSubscriptionsUI.localization.format("View_Tooltip", name);
            AddChild(sleekButton);
            ISleekSprite sleekSprite = Glazier.Get().CreateSprite();
            sleekSprite.positionOffset_X = 5;
            sleekSprite.positionOffset_Y = 5;
            sleekSprite.sizeOffset_X = 10;
            sleekSprite.sizeOffset_Y = 10;
            sleekSprite.sprite = MenuDashboardUI.icons.load<Sprite>("External_Link_Sprite");
            sleekSprite.drawMethod = ESleekSpriteType.Regular;
            sleekButton.AddChild(sleekSprite);
        }

        private void onClickedButton(ISleekElement button)
        {
            PublishedFileId_t publishedFileId_t = fileId;
            string url = "http://steamcommunity.com/sharedfiles/filedetails/?id=" + publishedFileId_t.ToString();
            Provider.provider.browserService.open(url);
        }
    }

    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static ISleekElement infoContainer;

    private static ISleekElement playersContainer;

    private static ISleekElement detailsContainer;

    private static ISleekElement mapContainer;

    private static ISleekElement buttonsContainer;

    private static ISleekBox titleBox;

    private static SleekWebImage titleIconImage;

    private static ISleekLabel titleNameLabel;

    private static ISleekLabel titleDescriptionLabel;

    private static ISleekBox playerCountBox;

    private static ISleekScrollView playersScrollBox;

    private static ISleekBox detailsBox;

    private static ISleekScrollView detailsScrollBox;

    private static ISleekButton hostBanWarningButton;

    private static ISleekButton notLoggedInWarningButton;

    private static ISleekElement linksFrame;

    private static ISleekBox serverTitle;

    private static ISleekBox serverBox;

    private static ISleekLabel serverWorkshopLabel;

    private static ISleekLabel serverCombatLabel;

    private static ISleekLabel serverPerspectiveLabel;

    private static ISleekLabel serverSecurityLabel;

    private static ISleekLabel serverModeLabel;

    private static ISleekLabel serverCheatsLabel;

    private static ISleekLabel serverMonetizationLabel;

    private static ISleekBox ugcTitle;

    private static ISleekBox ugcBox;

    private static ISleekBox configTitle;

    private static ISleekBox configBox;

    private static ISleekBox rocketTitle;

    private static ISleekBox rocketBox;

    private static ISleekBox mapNameBox;

    private static ISleekBox mapPreviewBox;

    private static ISleekImage mapPreviewImage;

    private static ISleekBox mapDescriptionBox;

    private static ISleekBox serverDescriptionBox;

    private static ISleekButton joinButton;

    private static ISleekBox joinDisabledBox;

    private static ISleekButton favoriteButton;

    private static ISleekButton refreshButton;

    private static ISleekButton cancelButton;

    private static SteamServerInfo serverInfo;

    private static string serverPassword;

    private static bool serverFavorited;

    private static List<PublishedFileId_t> expectedWorkshopItems;

    private static List<string> linkUrls;

    private static int playersOffset;

    private static int playerCount;

    private static UGCQueryHandle_t detailsHandle;

    private static CallResult<SteamUGCQueryCompleted_t> ugcQueryCompleted;

    private static MenuServerPasswordUI passwordUI;

    public static EServerInfoOpenContext openContext { get; private set; }

    private static void onUGCQueryCompleted(SteamUGCQueryCompleted_t callback, bool io)
    {
        if (callback.m_eResult != EResult.k_EResultOK || io)
        {
            return;
        }
        for (uint num = 0u; num < callback.m_unNumResultsReturned; num++)
        {
            if (TempSteamworksWorkshop.cacheDetails(callback.m_handle, num, out var cachedDetails))
            {
                string title = cachedDetails.GetTitle();
                ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
                sleekLabel.positionOffset_X = 5;
                sleekLabel.positionOffset_Y = (int)(num * 20);
                sleekLabel.sizeOffset_Y = 30;
                sleekLabel.sizeScale_X = 1f;
                sleekLabel.fontAlignment = TextAnchor.MiddleLeft;
                sleekLabel.text = title;
                sleekLabel.textColor = (cachedDetails.isBannedOrPrivate ? ESleekTint.BAD : ESleekTint.FONT);
                ugcBox.AddChild(sleekLabel);
                ServerInfoViewWorkshopButton serverInfoViewWorkshopButton = new ServerInfoViewWorkshopButton(cachedDetails.fileId, title);
                serverInfoViewWorkshopButton.positionOffset_X = -45;
                serverInfoViewWorkshopButton.positionOffset_Y = sleekLabel.positionOffset_Y + 5;
                serverInfoViewWorkshopButton.positionScale_X = 1f;
                ugcBox.AddChild(serverInfoViewWorkshopButton);
                SleekWorkshopSubscriptionButton sleekWorkshopSubscriptionButton = new SleekWorkshopSubscriptionButton();
                sleekWorkshopSubscriptionButton.positionOffset_X = -25;
                sleekWorkshopSubscriptionButton.positionOffset_Y = sleekLabel.positionOffset_Y + 5;
                sleekWorkshopSubscriptionButton.positionScale_X = 1f;
                sleekWorkshopSubscriptionButton.sizeOffset_X = 20;
                sleekWorkshopSubscriptionButton.sizeOffset_Y = 20;
                sleekWorkshopSubscriptionButton.subscribeText = localization.format("Subscribe");
                sleekWorkshopSubscriptionButton.unsubscribeText = localization.format("Unsubscribe");
                sleekWorkshopSubscriptionButton.subscribeTooltip = localization.format("Subscribe_Tooltip", title);
                sleekWorkshopSubscriptionButton.unsubscribeTooltip = localization.format("Unsubscribe_Tooltip", title);
                sleekWorkshopSubscriptionButton.fileId = cachedDetails.fileId;
                sleekWorkshopSubscriptionButton.synchronizeText();
                ugcBox.AddChild(sleekWorkshopSubscriptionButton);
            }
        }
        ugcBox.sizeOffset_Y = (int)(callback.m_unNumResultsReturned * 20 + 10);
        ugcTitle.isVisible = true;
        ugcBox.isVisible = true;
        updateDetails();
    }

    public static string GetClipboardData()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("Name: " + serverInfo.name);
        stringBuilder.AppendLine("Description: " + serverInfo.descText);
        stringBuilder.AppendLine("Thumbnail: " + serverInfo.thumbnailURL);
        stringBuilder.AppendLine($"Address: {Parser.getIPFromUInt32(serverInfo.ip)}:{serverInfo.queryPort}");
        stringBuilder.AppendLine($"SteamId: {serverInfo.steamID} ({serverInfo.steamID.GetEAccountType()})");
        stringBuilder.AppendLine($"Ping: {serverInfo.ping}ms");
        if (expectedWorkshopItems == null)
        {
            stringBuilder.AppendLine("Workshop files unknown");
        }
        else
        {
            stringBuilder.AppendLine($"{expectedWorkshopItems.Count} workshop file(s):");
            for (int i = 0; i < expectedWorkshopItems.Count; i++)
            {
                stringBuilder.AppendLine($"{i}: {expectedWorkshopItems[i]}");
            }
        }
        return stringBuilder.ToString();
    }

    public static void OpenWithoutRefresh()
    {
        if (!active)
        {
            active = true;
            container.AnimateIntoView();
        }
    }

    public static void open(SteamServerInfo newServerInfo, string newServerPassword, EServerInfoOpenContext newOpenContext)
    {
        if (!active)
        {
            active = true;
            openContext = newOpenContext;
            serverInfo = newServerInfo;
            serverPassword = newServerPassword;
            expectedWorkshopItems = null;
            linkUrls = null;
            bool flag = false;
            new IPv4Address(serverInfo.ip);
            bool flag2 = !serverInfo.steamID.BPersistentGameServerAccount() && new IPv4Address(serverInfo.ip).IsWideAreaNetwork;
            if (flag2)
            {
                UnturnedLog.info($"{serverInfo.name} is not logged in ({serverInfo.steamID}) and IP ({new IPv4Address(serverInfo.ip)}) is WAN");
            }
            notLoggedInWarningButton.isVisible = flag2;
            if (flag2)
            {
                joinButton.isVisible = false;
                joinDisabledBox.isVisible = true;
                joinDisabledBox.text = localization.format("NotLoggedInBlock_Label");
                joinDisabledBox.tooltipText = localization.format("NotLoggedInBlock_Tooltip");
            }
            else if (flag)
            {
                joinButton.isVisible = false;
                joinDisabledBox.isVisible = true;
                joinDisabledBox.text = localization.format("ServerBlacklisted_Label");
                joinDisabledBox.tooltipText = localization.format("ServerBlacklisted_Tooltip");
            }
            else
            {
                joinButton.isVisible = true;
                joinDisabledBox.isVisible = false;
            }
            reset();
            serverFavorited = Provider.GetServerIsFavorited(serverInfo.ip, serverInfo.queryPort);
            updateFavorite();
            updatePlayers();
            Provider.provider.matchmakingService.refreshPlayers(serverInfo.ip, serverInfo.queryPort);
            Provider.provider.matchmakingService.refreshPlayers(serverInfo.ip, serverInfo.queryPort);
            updateRules();
            Provider.provider.matchmakingService.refreshRules(serverInfo.ip, serverInfo.queryPort);
            updateServerInfo();
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

    private static void onClickedJoinButton(ISleekElement button)
    {
        if (serverInfo.isPassworded && string.IsNullOrEmpty(serverPassword))
        {
            MenuServerPasswordUI.open(serverInfo, expectedWorkshopItems);
            close();
        }
        else
        {
            Provider.connect(serverInfo, serverPassword, expectedWorkshopItems);
        }
    }

    private static void onClickedFavoriteButton(ISleekElement button)
    {
        serverFavorited = !serverFavorited;
        Provider.SetServerIsFavorited(serverInfo.ip, serverInfo.connectionPort, serverInfo.queryPort, serverFavorited);
        updateFavorite();
    }

    private static void onClickedRefreshButton(ISleekElement button)
    {
        updatePlayers();
        Provider.provider.matchmakingService.refreshPlayers(serverInfo.ip, serverInfo.queryPort);
    }

    private static void onClickedCancelButton(ISleekElement button)
    {
        switch (openContext)
        {
        case EServerInfoOpenContext.CONNECT:
            MenuPlayConnectUI.open();
            break;
        case EServerInfoOpenContext.SERVERS:
            MenuPlayServersUI.open();
            break;
        case EServerInfoOpenContext.MATCHMAKING:
            MenuPlayMatchmakingUI.open();
            break;
        }
        close();
    }

    private static void onMasterServerQueryRefreshed(SteamServerInfo server)
    {
        serverInfo = server;
        updateServerInfo();
    }

    private static void reset()
    {
        titleDescriptionLabel.text = "";
        titleIconImage.Clear();
        serverDescriptionBox.text = "";
    }

    private static void updateServerInfo()
    {
        titleNameLabel.textColor = (serverInfo.isPro ? new SleekColor(Palette.PRO) : new SleekColor(ESleekTint.FONT));
        titleNameLabel.text = serverInfo.name;
        int num = 0;
        serverWorkshopLabel.text = localization.format("Workshop", localization.format(serverInfo.isWorkshop ? "Yes" : "No"));
        num += 20;
        serverCombatLabel.text = localization.format("Combat", localization.format(serverInfo.isPvP ? "PvP" : "PvE"));
        num += 20;
        string text = serverInfo.cameraMode switch
        {
            ECameraMode.FIRST => localization.format("First"), 
            ECameraMode.THIRD => localization.format("Third"), 
            ECameraMode.BOTH => localization.format("Both"), 
            ECameraMode.VEHICLE => localization.format("Vehicle"), 
            _ => string.Empty, 
        };
        serverPerspectiveLabel.text = localization.format("Perspective", text);
        serverPerspectiveLabel.isVisible = !string.IsNullOrEmpty(text);
        num += (serverPerspectiveLabel.isVisible ? 20 : 0);
        string text2 = ((!serverInfo.IsVACSecure) ? localization.format("VAC_Insecure") : localization.format("VAC_Secure"));
        text2 = ((!serverInfo.IsBattlEyeSecure) ? (text2 + " + " + localization.format("BattlEye_Insecure")) : (text2 + " + " + localization.format("BattlEye_Secure")));
        serverSecurityLabel.positionOffset_Y = num;
        serverSecurityLabel.text = localization.format("Security", text2);
        num += 20;
        string arg = serverInfo.mode switch
        {
            EGameMode.EASY => localization.format("Easy"), 
            EGameMode.NORMAL => localization.format("Normal"), 
            EGameMode.HARD => localization.format("Hard"), 
            _ => string.Empty, 
        };
        serverModeLabel.positionOffset_Y = num;
        serverModeLabel.text = localization.format("Mode", arg);
        num += 20;
        serverCheatsLabel.positionOffset_Y = num;
        serverCheatsLabel.text = localization.format("Cheats", localization.format(serverInfo.hasCheats ? "Yes" : "No"));
        num += 20;
        if (serverInfo.monetization != 0)
        {
            serverMonetizationLabel.isVisible = true;
            serverMonetizationLabel.positionOffset_Y = num;
            switch (serverInfo.monetization)
            {
            case EServerMonetizationTag.None:
                serverMonetizationLabel.text = localization.format("Monetization_None");
                break;
            case EServerMonetizationTag.NonGameplay:
                serverMonetizationLabel.text = localization.format("Monetization_NonGameplay");
                break;
            default:
                serverMonetizationLabel.text = "unknown: " + serverInfo.monetization;
                break;
            }
            num += 20;
        }
        else
        {
            serverMonetizationLabel.isVisible = false;
        }
        serverBox.sizeOffset_Y = num + 10;
        updateDetails();
        LevelInfo level = Level.getLevel(serverInfo.map);
        if (level != null)
        {
            Local local = level.getLocalization();
            if (local != null)
            {
                string desc = local.format("Description");
                desc = ItemTool.filterRarityRichText(desc);
                RichTextUtil.replaceNewlineMarkup(ref desc);
                mapDescriptionBox.text = desc;
            }
            if (local != null && local.has("Name"))
            {
                mapNameBox.text = localization.format("Map", local.format("Name"));
            }
            else
            {
                mapNameBox.text = localization.format("Map", serverInfo.map);
            }
            string previewImageFilePath = level.GetPreviewImageFilePath();
            if (!string.IsNullOrEmpty(previewImageFilePath))
            {
                mapPreviewImage.setTextureAndShouldDestroy(ReadWrite.readTextureFromFile(previewImageFilePath), shouldDestroyTexture: true);
            }
        }
        else
        {
            mapDescriptionBox.text = string.Empty;
            mapNameBox.text = serverInfo.map;
            mapPreviewImage.setTextureAndShouldDestroy(null, shouldDestroyTexture: true);
        }
    }

    private static void updateFavorite()
    {
        if (serverFavorited)
        {
            favoriteButton.text = localization.format("Favorite_Off_Button");
        }
        else
        {
            favoriteButton.text = localization.format("Favorite_On_Button");
        }
    }

    private static void updatePlayers()
    {
        playersScrollBox.RemoveAllChildren();
        playersOffset = 0;
        playerCount = 0;
        playerCountBox.text = localization.format("Players", playerCount, serverInfo.maxPlayers);
    }

    private static void onPlayersQueryRefreshed(string name, int score, float time)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        string text = string.Empty;
        if (timeSpan.Days > 0)
        {
            text = text + " " + timeSpan.Days + "d";
        }
        if (timeSpan.Hours > 0)
        {
            text = text + " " + timeSpan.Hours + "h";
        }
        if (timeSpan.Minutes > 0)
        {
            text = text + " " + timeSpan.Minutes + "m";
        }
        if (timeSpan.Seconds > 0)
        {
            text = text + " " + timeSpan.Seconds + "s";
        }
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        sleekBox.positionOffset_Y = playersOffset;
        sleekBox.sizeOffset_Y = 30;
        sleekBox.sizeScale_X = 1f;
        playersScrollBox.AddChild(sleekBox);
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.positionOffset_X = 5;
        sleekLabel.sizeOffset_X = -10;
        sleekLabel.sizeScale_X = 1f;
        sleekLabel.sizeScale_Y = 1f;
        sleekLabel.fontAlignment = TextAnchor.MiddleLeft;
        sleekLabel.text = name;
        sleekBox.AddChild(sleekLabel);
        ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
        sleekLabel2.positionOffset_X = -5;
        sleekLabel2.sizeOffset_X = -10;
        sleekLabel2.sizeScale_X = 1f;
        sleekLabel2.sizeScale_Y = 1f;
        sleekLabel2.fontAlignment = TextAnchor.MiddleRight;
        sleekLabel2.text = text;
        sleekBox.AddChild(sleekLabel2);
        playersOffset += 40;
        playersScrollBox.contentSizeOffset = new Vector2(0f, playersOffset - 10);
        playerCount++;
        playerCountBox.text = localization.format("Players", playerCount, serverInfo.maxPlayers);
    }

    private static void updateRules()
    {
        linksFrame.RemoveAllChildren();
        linksFrame.isVisible = false;
        ugcTitle.isVisible = false;
        ugcBox.RemoveAllChildren();
        ugcBox.isVisible = false;
        configTitle.isVisible = false;
        configBox.RemoveAllChildren();
        configBox.isVisible = false;
        rocketTitle.isVisible = false;
        rocketBox.RemoveAllChildren();
        rocketBox.isVisible = false;
        updateDetails();
    }

    private static void onRulesQueryRefreshed(Dictionary<string, string> rulesMap)
    {
        if (rulesMap == null)
        {
            return;
        }
        if (rulesMap.TryGetValue("Browser_Icon", out var value) && !string.IsNullOrEmpty(value))
        {
            titleIconImage.Refresh(value);
        }
        if (rulesMap.TryGetValue("Browser_Desc_Hint", out var value2) && !string.IsNullOrEmpty(value2))
        {
            if (OptionsSettings.filter)
            {
                ProfanityFilter.filter(ref value2);
            }
            titleDescriptionLabel.text = value2;
        }
        if (rulesMap.TryGetValue("Browser_Desc_Full_Count", out var value3) && int.TryParse(value3, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) && result > 0)
        {
            string text = string.Empty;
            for (int i = 0; i < result; i++)
            {
                if (rulesMap.TryGetValue("Browser_Desc_Full_Line_" + i, out var value4))
                {
                    text += value4;
                }
            }
            if (ConvertEx.TryDecodeBase64AsUtf8String(text, out var output))
            {
                if (!string.IsNullOrEmpty(output))
                {
                    if (OptionsSettings.filter)
                    {
                        ProfanityFilter.filter(ref output);
                    }
                    RichTextUtil.replaceNewlineMarkup(ref output);
                    serverDescriptionBox.text = output;
                }
            }
            else
            {
                UnturnedLog.error("Unable to convert server browser Base64 string: \"" + text + "\"");
            }
        }
        linkUrls = new List<string>();
        if (rulesMap.TryGetValue("Custom_Links_Count", out var value5) && int.TryParse(value5, NumberStyles.Any, CultureInfo.InvariantCulture, out var result2) && result2 > 0)
        {
            int num = 0;
            for (int j = 0; j < result2; j++)
            {
                if (!rulesMap.TryGetValue("Custom_Link_Message_" + j, out var value6))
                {
                    UnturnedLog.warn("Skipping link index {0} because message is missing", j);
                    continue;
                }
                if (string.IsNullOrEmpty(value6))
                {
                    UnturnedLog.warn("Skipping link index {0} because message is empty", j);
                    continue;
                }
                if (!rulesMap.TryGetValue("Custom_Link_Url_" + j, out var value7))
                {
                    UnturnedLog.warn("Skipping link index {0} because url is missing", j);
                    continue;
                }
                if (string.IsNullOrEmpty(value7))
                {
                    UnturnedLog.warn("Skipping link index {0} because url is empty", j);
                    continue;
                }
                if (!ConvertEx.TryDecodeBase64AsUtf8String(value6, out var output2))
                {
                    UnturnedLog.warn("Skipping link index {0} because unable to decode message Base64: \"{1}\"", j, value6);
                    continue;
                }
                if (!ConvertEx.TryDecodeBase64AsUtf8String(value7, out var output3))
                {
                    UnturnedLog.warn("Skipping link index {0} because unable to decode url Base64: \"{1}\"", j, value7);
                    continue;
                }
                if (!WebUtils.ParseThirdPartyUrl(output3, out var result3))
                {
                    UnturnedLog.warn("Ignoring potentially unsafe link index {0} url {1}", j, output3);
                    continue;
                }
                if (OptionsSettings.filter)
                {
                    ProfanityFilter.filter(ref output2);
                }
                linkUrls.Add(result3);
                ISleekButton sleekButton = Glazier.Get().CreateButton();
                sleekButton.positionOffset_Y += num;
                sleekButton.sizeScale_X = 1f;
                sleekButton.sizeOffset_Y = 30;
                sleekButton.enableRichText = true;
                sleekButton.text = output2;
                sleekButton.tooltipText = result3;
                sleekButton.textColor = ESleekTint.RICH_TEXT_DEFAULT;
                sleekButton.onClickedButton += OnClickedLinkButton;
                linksFrame.AddChild(sleekButton);
                num += 30;
            }
            if (num > 0)
            {
                linksFrame.sizeOffset_Y = num;
                linksFrame.isVisible = true;
            }
        }
        if (rulesMap.TryGetValue("rocketplugins", out var value8) && !string.IsNullOrEmpty(value8))
        {
            string[] array = value8.Split(',');
            rocketBox.sizeOffset_Y = array.Length * 20 + 10;
            for (int k = 0; k < array.Length; k++)
            {
                ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
                sleekLabel.positionOffset_X = 5;
                sleekLabel.positionOffset_Y = k * 20;
                sleekLabel.sizeOffset_Y = 30;
                sleekLabel.sizeScale_X = 1f;
                sleekLabel.fontAlignment = TextAnchor.MiddleLeft;
                sleekLabel.text = array[k];
                rocketBox.AddChild(sleekLabel);
            }
            if (serverInfo.pluginFramework == SteamServerInfo.EPluginFramework.Rocket)
            {
                rocketTitle.text = localization.format("Plugins_Rocket");
            }
            else if (serverInfo.pluginFramework == SteamServerInfo.EPluginFramework.OpenMod)
            {
                rocketTitle.text = localization.format("Plugins_OpenMod");
            }
            else
            {
                rocketTitle.text = localization.format("Plugins_Unknown");
            }
            rocketTitle.isVisible = true;
            rocketBox.isVisible = true;
        }
        expectedWorkshopItems = new List<PublishedFileId_t>(0);
        if (rulesMap.TryGetValue("Mod_Count", out var value9) && int.TryParse(value9, NumberStyles.Any, CultureInfo.InvariantCulture, out var result4) && result4 > 0)
        {
            string text2 = string.Empty;
            for (int l = 0; l < result4; l++)
            {
                if (rulesMap.TryGetValue("Mod_" + l, out var value10))
                {
                    text2 += value10;
                }
            }
            string[] array2 = text2.Split(',');
            expectedWorkshopItems = new List<PublishedFileId_t>(array2.Length);
            for (int m = 0; m < array2.Length; m++)
            {
                if (ulong.TryParse(array2[m], NumberStyles.Any, CultureInfo.InvariantCulture, out var result5))
                {
                    expectedWorkshopItems.Add(new PublishedFileId_t(result5));
                }
            }
            detailsHandle = SteamUGC.CreateQueryUGCDetailsRequest(expectedWorkshopItems.ToArray(), (uint)expectedWorkshopItems.Count);
            SteamUGC.SetAllowCachedResponse(detailsHandle, 60u);
            SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(detailsHandle);
            ugcQueryCompleted.Set(hAPICall);
        }
        if (rulesMap.TryGetValue("Cfg_Count", out var value11) && int.TryParse(value11, NumberStyles.Any, CultureInfo.InvariantCulture, out var result6) && result6 > 0)
        {
            int num2 = 0;
            for (int n = 0; n < result6; n++)
            {
                if (!rulesMap.TryGetValue("Cfg_" + n.ToString(CultureInfo.InvariantCulture), out var value12))
                {
                    continue;
                }
                int num3 = value12.IndexOf('.');
                int num4 = value12.IndexOf('=', num3 + 1);
                if (num3 >= 0 && num4 >= 0)
                {
                    string fieldName = value12.Substring(0, num3);
                    string fieldName2 = value12.Substring(num3 + 1, num4 - num3 - 1);
                    string text3 = value12.Substring(num4 + 1);
                    string text4 = null;
                    float result7;
                    int result8;
                    if (text3 == "T")
                    {
                        text4 = localization.format("Yes");
                    }
                    else if (text3 == "F")
                    {
                        text4 = localization.format("No");
                    }
                    else if (float.TryParse(text3, NumberStyles.Any, CultureInfo.InvariantCulture, out result7))
                    {
                        text4 = result7.ToString();
                    }
                    else if (int.TryParse(text3, NumberStyles.Any, CultureInfo.InvariantCulture, out result8))
                    {
                        text4 = result8.ToString();
                    }
                    if (string.IsNullOrEmpty(text4))
                    {
                        ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
                        sleekLabel2.positionOffset_X = 5;
                        sleekLabel2.positionOffset_Y = num2;
                        sleekLabel2.sizeOffset_X = -10;
                        sleekLabel2.sizeOffset_Y = 30;
                        sleekLabel2.sizeScale_X = 1f;
                        sleekLabel2.fontAlignment = TextAnchor.MiddleLeft;
                        sleekLabel2.textColor = Color.red;
                        sleekLabel2.text = value12;
                        configBox.AddChild(sleekLabel2);
                    }
                    else
                    {
                        ISleekLabel sleekLabel3 = Glazier.Get().CreateLabel();
                        sleekLabel3.positionOffset_X = 5;
                        sleekLabel3.positionOffset_Y = num2;
                        sleekLabel3.sizeOffset_X = -5;
                        sleekLabel3.sizeOffset_Y = 30;
                        sleekLabel3.sizeScale_X = 0.25f;
                        sleekLabel3.fontAlignment = TextAnchor.MiddleRight;
                        sleekLabel3.text = MenuPlayConfigUI.sanitizeName(fieldName);
                        sleekLabel3.textColor = new SleekColor(ESleekTint.FONT, 0.5f);
                        configBox.AddChild(sleekLabel3);
                        ISleekLabel sleekLabel4 = Glazier.Get().CreateLabel();
                        sleekLabel4.positionOffset_X = 5;
                        sleekLabel4.positionOffset_Y = num2;
                        sleekLabel4.positionScale_X = 0.25f;
                        sleekLabel4.sizeOffset_X = -5;
                        sleekLabel4.sizeOffset_Y = 30;
                        sleekLabel4.sizeScale_X = 0.75f;
                        sleekLabel4.fontAlignment = TextAnchor.MiddleLeft;
                        sleekLabel4.text = localization.format("Rule", MenuPlayConfigUI.sanitizeName(fieldName2), text4);
                        configBox.AddChild(sleekLabel4);
                    }
                    num2 += 20;
                }
            }
            configBox.sizeOffset_Y = num2 + 10;
            if (num2 > 0)
            {
                configTitle.isVisible = true;
                configBox.isVisible = true;
            }
        }
        if (!rulesMap.TryGetValue("GameVersion", out var value13))
        {
            value13 = "3.21.7.0";
        }
        if (Parser.TryGetUInt32FromIP(value13, out var value14) && Provider.APP_VERSION_PACKED != value14)
        {
            joinButton.isVisible = false;
            joinDisabledBox.isVisible = true;
            if (value14 > Provider.APP_VERSION_PACKED)
            {
                joinDisabledBox.text = localization.format("ServerNewerVersion_Label", value13);
                joinDisabledBox.tooltipText = localization.format("ServerNewerVersion_Tooltip");
            }
            else
            {
                joinDisabledBox.text = localization.format("ServerOlderVersion_Label", value13);
                joinDisabledBox.tooltipText = localization.format("ServerOlderVersion_Tooltip");
            }
        }
        updateDetails();
    }

    private static void updateDetails()
    {
        int num = 0;
        if (hostBanWarningButton.isVisible)
        {
            hostBanWarningButton.positionOffset_X = num;
            num += hostBanWarningButton.sizeOffset_Y + 10;
        }
        if (notLoggedInWarningButton.isVisible)
        {
            notLoggedInWarningButton.positionOffset_X = num;
            num += notLoggedInWarningButton.sizeOffset_Y + 10;
        }
        if (linksFrame.isVisible)
        {
            linksFrame.positionOffset_Y = num;
            num += linksFrame.sizeOffset_Y + 10;
        }
        if (serverTitle.isVisible)
        {
            serverTitle.positionOffset_Y = num;
            num += 40;
        }
        if (serverBox.isVisible)
        {
            serverBox.positionOffset_Y = num;
            num += serverBox.sizeOffset_Y + 10;
        }
        if (ugcTitle.isVisible)
        {
            ugcTitle.positionOffset_Y = num;
            num += ugcTitle.sizeOffset_Y + 10;
        }
        if (ugcBox.isVisible)
        {
            ugcBox.positionOffset_Y = num;
            num += ugcBox.sizeOffset_Y + 10;
        }
        if (configTitle.isVisible)
        {
            configTitle.positionOffset_Y = num;
            num += configTitle.sizeOffset_Y + 10;
        }
        if (configBox.isVisible)
        {
            configBox.positionOffset_Y = num;
            num += configBox.sizeOffset_Y + 10;
        }
        if (rocketTitle.isVisible)
        {
            rocketTitle.positionOffset_Y = num;
            num += rocketTitle.sizeOffset_Y + 10;
        }
        if (rocketBox.isVisible)
        {
            rocketBox.positionOffset_Y = num;
            num += rocketBox.sizeOffset_Y + 10;
        }
        detailsScrollBox.contentSizeOffset = new Vector2(0f, num - 10);
    }

    private static void OnClickedLinkButton(ISleekElement button)
    {
        int index = linksFrame.FindIndexOfChild(button);
        Provider.openURL(linkUrls[index]);
    }

    private static void OnClickedHostBanWarning(ISleekElement button)
    {
        Provider.openURL("https://github.com/SmartlyDressedGames/U3-Docs/blob/master/ServerHostingRules.md");
    }

    private static void OnClickedNotLoggedInWarning(ISleekElement button)
    {
        Provider.openURL("https://github.com/SmartlyDressedGames/U3-Docs/blob/master/GameServerLoginTokens.md");
    }

    public void OnDestroy()
    {
        TempSteamworksMatchmaking matchmakingService = Provider.provider.matchmakingService;
        matchmakingService.onMasterServerQueryRefreshed = (TempSteamworksMatchmaking.MasterServerQueryRefreshed)Delegate.Remove(matchmakingService.onMasterServerQueryRefreshed, new TempSteamworksMatchmaking.MasterServerQueryRefreshed(onMasterServerQueryRefreshed));
        TempSteamworksMatchmaking matchmakingService2 = Provider.provider.matchmakingService;
        matchmakingService2.onPlayersQueryRefreshed = (TempSteamworksMatchmaking.PlayersQueryRefreshed)Delegate.Remove(matchmakingService2.onPlayersQueryRefreshed, new TempSteamworksMatchmaking.PlayersQueryRefreshed(onPlayersQueryRefreshed));
        TempSteamworksMatchmaking matchmakingService3 = Provider.provider.matchmakingService;
        matchmakingService3.onRulesQueryRefreshed = (TempSteamworksMatchmaking.RulesQueryRefreshed)Delegate.Remove(matchmakingService3.onRulesQueryRefreshed, new TempSteamworksMatchmaking.RulesQueryRefreshed(onRulesQueryRefreshed));
        if (ugcQueryCompleted != null)
        {
            ugcQueryCompleted.Dispose();
            ugcQueryCompleted = null;
        }
    }

    public MenuPlayServerInfoUI()
    {
        localization = Localization.read("/Menu/Play/MenuPlayServerInfo.dat");
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_Y = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        infoContainer = Glazier.Get().CreateFrame();
        infoContainer.positionOffset_Y = 94;
        infoContainer.sizeOffset_Y = -154;
        infoContainer.sizeScale_X = 1f;
        infoContainer.sizeScale_Y = 1f;
        container.AddChild(infoContainer);
        buttonsContainer = Glazier.Get().CreateFrame();
        buttonsContainer.positionOffset_Y = -50;
        buttonsContainer.positionScale_Y = 1f;
        buttonsContainer.sizeOffset_Y = 50;
        buttonsContainer.sizeScale_X = 1f;
        container.AddChild(buttonsContainer);
        titleBox = Glazier.Get().CreateBox();
        titleBox.sizeOffset_Y = 84;
        titleBox.sizeScale_X = 1f;
        container.AddChild(titleBox);
        titleIconImage = new SleekWebImage();
        titleIconImage.positionOffset_X = 10;
        titleIconImage.positionOffset_Y = 10;
        titleIconImage.sizeOffset_X = 64;
        titleIconImage.sizeOffset_Y = 64;
        titleBox.AddChild(titleIconImage);
        titleNameLabel = Glazier.Get().CreateLabel();
        titleNameLabel.positionOffset_X = 79;
        titleNameLabel.positionOffset_Y = 5;
        titleNameLabel.sizeOffset_X = -84;
        titleNameLabel.sizeOffset_Y = 40;
        titleNameLabel.sizeScale_X = 1f;
        titleNameLabel.fontSize = ESleekFontSize.Large;
        titleBox.AddChild(titleNameLabel);
        titleDescriptionLabel = Glazier.Get().CreateLabel();
        titleDescriptionLabel.positionOffset_X = 79;
        titleDescriptionLabel.positionOffset_Y = 45;
        titleDescriptionLabel.sizeOffset_X = -84;
        titleDescriptionLabel.sizeOffset_Y = 34;
        titleDescriptionLabel.sizeScale_X = 1f;
        titleDescriptionLabel.enableRichText = true;
        titleDescriptionLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        titleDescriptionLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        titleBox.AddChild(titleDescriptionLabel);
        playersContainer = Glazier.Get().CreateFrame();
        playersContainer.sizeOffset_X = 280;
        playersContainer.sizeScale_Y = 1f;
        infoContainer.AddChild(playersContainer);
        playerCountBox = Glazier.Get().CreateBox();
        playerCountBox.sizeScale_X = 1f;
        playerCountBox.sizeOffset_Y = 30;
        playersContainer.AddChild(playerCountBox);
        playersScrollBox = Glazier.Get().CreateScrollView();
        playersScrollBox.positionOffset_Y = 40;
        playersScrollBox.sizeScale_X = 1f;
        playersScrollBox.sizeOffset_Y = -40;
        playersScrollBox.sizeScale_Y = 1f;
        playersScrollBox.scaleContentToWidth = true;
        playersContainer.AddChild(playersScrollBox);
        detailsContainer = Glazier.Get().CreateFrame();
        detailsContainer.positionOffset_X = 290;
        detailsContainer.sizeOffset_X = -detailsContainer.positionOffset_X - 350;
        detailsContainer.sizeScale_X = 1f;
        detailsContainer.sizeScale_Y = 1f;
        infoContainer.AddChild(detailsContainer);
        detailsBox = Glazier.Get().CreateBox();
        detailsBox.sizeScale_X = 1f;
        detailsBox.sizeOffset_Y = 30;
        detailsBox.text = localization.format("Details");
        detailsContainer.AddChild(detailsBox);
        detailsScrollBox = Glazier.Get().CreateScrollView();
        detailsScrollBox.positionOffset_Y = 40;
        detailsScrollBox.sizeScale_X = 1f;
        detailsScrollBox.sizeOffset_Y = -40;
        detailsScrollBox.sizeScale_Y = 1f;
        detailsScrollBox.scaleContentToWidth = true;
        detailsContainer.AddChild(detailsScrollBox);
        hostBanWarningButton = Glazier.Get().CreateButton();
        hostBanWarningButton.sizeOffset_Y = 60;
        hostBanWarningButton.sizeScale_X = 1f;
        hostBanWarningButton.isVisible = false;
        hostBanWarningButton.onClickedButton += OnClickedHostBanWarning;
        detailsScrollBox.AddChild(hostBanWarningButton);
        notLoggedInWarningButton = Glazier.Get().CreateButton();
        notLoggedInWarningButton.sizeOffset_Y = 60;
        notLoggedInWarningButton.sizeScale_X = 1f;
        notLoggedInWarningButton.isVisible = false;
        notLoggedInWarningButton.onClickedButton += OnClickedNotLoggedInWarning;
        notLoggedInWarningButton.text += localization.format("NotLoggedInMessage");
        notLoggedInWarningButton.textColor = ESleekTint.BAD;
        detailsScrollBox.AddChild(notLoggedInWarningButton);
        notLoggedInWarningButton.isVisible = false;
        linksFrame = Glazier.Get().CreateFrame();
        linksFrame.positionOffset_Y = 40;
        linksFrame.sizeScale_X = 1f;
        detailsScrollBox.AddChild(linksFrame);
        serverTitle = Glazier.Get().CreateBox();
        serverTitle.sizeOffset_Y = 30;
        serverTitle.sizeScale_X = 1f;
        serverTitle.text = localization.format("Server");
        detailsScrollBox.AddChild(serverTitle);
        serverBox = Glazier.Get().CreateBox();
        serverBox.positionOffset_Y = 40;
        serverBox.sizeScale_X = 1f;
        serverBox.sizeOffset_Y = 130;
        detailsScrollBox.AddChild(serverBox);
        serverWorkshopLabel = Glazier.Get().CreateLabel();
        serverWorkshopLabel.positionOffset_X = 5;
        serverWorkshopLabel.positionOffset_Y = 0;
        serverWorkshopLabel.sizeOffset_Y = 30;
        serverWorkshopLabel.sizeScale_X = 1f;
        serverWorkshopLabel.fontAlignment = TextAnchor.MiddleLeft;
        serverBox.AddChild(serverWorkshopLabel);
        serverCombatLabel = Glazier.Get().CreateLabel();
        serverCombatLabel.positionOffset_X = 5;
        serverCombatLabel.positionOffset_Y = 20;
        serverCombatLabel.sizeOffset_Y = 30;
        serverCombatLabel.sizeScale_X = 1f;
        serverCombatLabel.fontAlignment = TextAnchor.MiddleLeft;
        serverBox.AddChild(serverCombatLabel);
        serverPerspectiveLabel = Glazier.Get().CreateLabel();
        serverPerspectiveLabel.positionOffset_X = 5;
        serverPerspectiveLabel.positionOffset_Y = 40;
        serverPerspectiveLabel.sizeOffset_Y = 30;
        serverPerspectiveLabel.sizeScale_X = 1f;
        serverPerspectiveLabel.fontAlignment = TextAnchor.MiddleLeft;
        serverBox.AddChild(serverPerspectiveLabel);
        serverSecurityLabel = Glazier.Get().CreateLabel();
        serverSecurityLabel.positionOffset_X = 5;
        serverSecurityLabel.positionOffset_Y = 60;
        serverSecurityLabel.sizeOffset_Y = 30;
        serverSecurityLabel.sizeScale_X = 1f;
        serverSecurityLabel.fontAlignment = TextAnchor.MiddleLeft;
        serverBox.AddChild(serverSecurityLabel);
        serverModeLabel = Glazier.Get().CreateLabel();
        serverModeLabel.positionOffset_X = 5;
        serverModeLabel.positionOffset_Y = 80;
        serverModeLabel.sizeOffset_Y = 30;
        serverModeLabel.sizeScale_X = 1f;
        serverModeLabel.fontAlignment = TextAnchor.MiddleLeft;
        serverBox.AddChild(serverModeLabel);
        serverCheatsLabel = Glazier.Get().CreateLabel();
        serverCheatsLabel.positionOffset_X = 5;
        serverCheatsLabel.positionOffset_Y = 100;
        serverCheatsLabel.sizeOffset_Y = 30;
        serverCheatsLabel.sizeScale_X = 1f;
        serverCheatsLabel.fontAlignment = TextAnchor.MiddleLeft;
        serverBox.AddChild(serverCheatsLabel);
        serverMonetizationLabel = Glazier.Get().CreateLabel();
        serverMonetizationLabel.positionOffset_X = 5;
        serverMonetizationLabel.positionOffset_Y = 100;
        serverMonetizationLabel.sizeOffset_Y = 30;
        serverMonetizationLabel.sizeScale_X = 1f;
        serverMonetizationLabel.fontAlignment = TextAnchor.MiddleLeft;
        serverBox.AddChild(serverMonetizationLabel);
        ugcTitle = Glazier.Get().CreateBox();
        ugcTitle.sizeOffset_Y = 30;
        ugcTitle.sizeScale_X = 1f;
        ugcTitle.text = localization.format("UGC");
        detailsScrollBox.AddChild(ugcTitle);
        ugcTitle.isVisible = false;
        ugcBox = Glazier.Get().CreateBox();
        ugcBox.sizeScale_X = 1f;
        detailsScrollBox.AddChild(ugcBox);
        ugcBox.isVisible = false;
        configTitle = Glazier.Get().CreateBox();
        configTitle.sizeOffset_Y = 30;
        configTitle.sizeScale_X = 1f;
        configTitle.text = localization.format("Config");
        detailsScrollBox.AddChild(configTitle);
        configTitle.isVisible = false;
        configBox = Glazier.Get().CreateBox();
        configBox.sizeScale_X = 1f;
        detailsScrollBox.AddChild(configBox);
        configBox.isVisible = false;
        rocketTitle = Glazier.Get().CreateBox();
        rocketTitle.sizeOffset_Y = 30;
        rocketTitle.sizeScale_X = 1f;
        detailsScrollBox.AddChild(rocketTitle);
        rocketTitle.isVisible = false;
        rocketBox = Glazier.Get().CreateBox();
        rocketBox.sizeScale_X = 1f;
        detailsScrollBox.AddChild(rocketBox);
        rocketBox.isVisible = false;
        mapContainer = Glazier.Get().CreateFrame();
        mapContainer.positionOffset_X = -340;
        mapContainer.positionScale_X = 1f;
        mapContainer.sizeOffset_X = 340;
        mapContainer.sizeScale_Y = 1f;
        infoContainer.AddChild(mapContainer);
        mapNameBox = Glazier.Get().CreateBox();
        mapNameBox.sizeOffset_X = 340;
        mapNameBox.sizeOffset_Y = 30;
        mapContainer.AddChild(mapNameBox);
        mapPreviewBox = Glazier.Get().CreateBox();
        mapPreviewBox.positionOffset_Y = 40;
        mapPreviewBox.sizeOffset_X = 340;
        mapPreviewBox.sizeOffset_Y = 200;
        mapContainer.AddChild(mapPreviewBox);
        mapPreviewImage = Glazier.Get().CreateImage();
        mapPreviewImage.positionOffset_X = 10;
        mapPreviewImage.positionOffset_Y = 10;
        mapPreviewImage.sizeOffset_X = -20;
        mapPreviewImage.sizeOffset_Y = -20;
        mapPreviewImage.sizeScale_X = 1f;
        mapPreviewImage.sizeScale_Y = 1f;
        mapPreviewBox.AddChild(mapPreviewImage);
        mapDescriptionBox = Glazier.Get().CreateBox();
        mapDescriptionBox.positionOffset_Y = 250;
        mapDescriptionBox.sizeOffset_X = 340;
        mapDescriptionBox.sizeOffset_Y = 140;
        mapDescriptionBox.fontAlignment = TextAnchor.UpperCenter;
        mapDescriptionBox.enableRichText = true;
        mapDescriptionBox.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        mapContainer.AddChild(mapDescriptionBox);
        serverDescriptionBox = Glazier.Get().CreateBox();
        serverDescriptionBox.positionOffset_Y = 400;
        serverDescriptionBox.sizeOffset_X = 340;
        serverDescriptionBox.sizeOffset_Y = -400;
        serverDescriptionBox.sizeScale_Y = 1f;
        serverDescriptionBox.fontAlignment = TextAnchor.UpperCenter;
        serverDescriptionBox.enableRichText = true;
        serverDescriptionBox.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        serverDescriptionBox.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        mapContainer.AddChild(serverDescriptionBox);
        joinButton = Glazier.Get().CreateButton();
        joinButton.sizeOffset_X = -5;
        joinButton.sizeScale_X = 0.25f;
        joinButton.sizeScale_Y = 1f;
        joinButton.text = localization.format("Join_Button");
        joinButton.tooltipText = localization.format("Join_Button_Tooltip");
        joinButton.onClickedButton += onClickedJoinButton;
        joinButton.fontSize = ESleekFontSize.Medium;
        buttonsContainer.AddChild(joinButton);
        joinDisabledBox = Glazier.Get().CreateBox();
        joinDisabledBox.sizeOffset_X = -5;
        joinDisabledBox.sizeScale_X = 0.25f;
        joinDisabledBox.sizeScale_Y = 1f;
        joinDisabledBox.textColor = ESleekTint.BAD;
        buttonsContainer.AddChild(joinDisabledBox);
        joinDisabledBox.isVisible = false;
        favoriteButton = Glazier.Get().CreateButton();
        favoriteButton.positionOffset_X = 5;
        favoriteButton.positionScale_X = 0.25f;
        favoriteButton.sizeOffset_X = -10;
        favoriteButton.sizeScale_X = 0.25f;
        favoriteButton.sizeScale_Y = 1f;
        favoriteButton.tooltipText = localization.format("Favorite_Button_Tooltip");
        favoriteButton.onClickedButton += onClickedFavoriteButton;
        favoriteButton.fontSize = ESleekFontSize.Medium;
        buttonsContainer.AddChild(favoriteButton);
        refreshButton = Glazier.Get().CreateButton();
        refreshButton.positionOffset_X = 5;
        refreshButton.positionScale_X = 0.5f;
        refreshButton.sizeOffset_X = -10;
        refreshButton.sizeScale_X = 0.25f;
        refreshButton.sizeScale_Y = 1f;
        refreshButton.text = localization.format("Refresh_Button");
        refreshButton.tooltipText = localization.format("Refresh_Button_Tooltip");
        refreshButton.onClickedButton += onClickedRefreshButton;
        refreshButton.fontSize = ESleekFontSize.Medium;
        buttonsContainer.AddChild(refreshButton);
        cancelButton = Glazier.Get().CreateButton();
        cancelButton.positionOffset_X = 5;
        cancelButton.positionScale_X = 0.75f;
        cancelButton.sizeOffset_X = -5;
        cancelButton.sizeScale_X = 0.25f;
        cancelButton.sizeScale_Y = 1f;
        cancelButton.text = localization.format("Cancel_Button");
        cancelButton.tooltipText = localization.format("Cancel_Button_Tooltip");
        cancelButton.onClickedButton += onClickedCancelButton;
        cancelButton.fontSize = ESleekFontSize.Medium;
        buttonsContainer.AddChild(cancelButton);
        TempSteamworksMatchmaking matchmakingService = Provider.provider.matchmakingService;
        matchmakingService.onMasterServerQueryRefreshed = (TempSteamworksMatchmaking.MasterServerQueryRefreshed)Delegate.Combine(matchmakingService.onMasterServerQueryRefreshed, new TempSteamworksMatchmaking.MasterServerQueryRefreshed(onMasterServerQueryRefreshed));
        TempSteamworksMatchmaking matchmakingService2 = Provider.provider.matchmakingService;
        matchmakingService2.onPlayersQueryRefreshed = (TempSteamworksMatchmaking.PlayersQueryRefreshed)Delegate.Combine(matchmakingService2.onPlayersQueryRefreshed, new TempSteamworksMatchmaking.PlayersQueryRefreshed(onPlayersQueryRefreshed));
        TempSteamworksMatchmaking matchmakingService3 = Provider.provider.matchmakingService;
        matchmakingService3.onRulesQueryRefreshed = (TempSteamworksMatchmaking.RulesQueryRefreshed)Delegate.Combine(matchmakingService3.onRulesQueryRefreshed, new TempSteamworksMatchmaking.RulesQueryRefreshed(onRulesQueryRefreshed));
        if (ugcQueryCompleted == null)
        {
            ugcQueryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create(onUGCQueryCompleted);
        }
        passwordUI = new MenuServerPasswordUI();
    }
}
