using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

namespace SDG.Unturned;

public class MenuUI : MonoBehaviour
{
    public static SleekWindow window;

    public static SleekFullscreenBox container;

    private static ISleekBox alertBox;

    private static ISleekLabel originLabel;

    private static ISleekButton dismissNotificationButton;

    internal static SleekButtonIcon copyNotificationButton;

    private static SleekInventory[] itemAlerts;

    private static bool isAlerting;

    private Transform title;

    private Transform play;

    private Transform survivors;

    private Transform configuration;

    private Transform workshop;

    private Transform target;

    private static bool hasPanned;

    private static bool hasTitled;

    internal static MenuUI instance;

    private MenuDashboardUI dashboard;

    private static void removeItemAlerts()
    {
        if (itemAlerts != null)
        {
            SleekInventory[] array = itemAlerts;
            foreach (SleekInventory child in array)
            {
                alertBox.RemoveChild(child);
            }
            itemAlerts = null;
        }
    }

    private static void alertText()
    {
        if (alertBox != null && originLabel != null)
        {
            alertBox.positionOffset_Y = -50;
            alertBox.sizeOffset_Y = 100;
            copyNotificationButton.isVisible = true;
            originLabel.isVisible = false;
            removeItemAlerts();
        }
    }

    private static void alertItem()
    {
        if (alertBox != null && originLabel != null)
        {
            alertBox.text = "";
            alertBox.positionOffset_Y = -150;
            alertBox.sizeOffset_Y = 300;
            copyNotificationButton.isVisible = false;
            originLabel.isVisible = true;
        }
    }

    private static void internalOpenAlert()
    {
        if (alertBox != null)
        {
            alertBox.lerpPositionScale(0f, 0.5f, ESleekLerp.EXPONENTIAL, 20f);
        }
        if (container != null)
        {
            container.AnimateOutOfView(-1f, 0f);
        }
        MenuDashboardUI.setCanvasActive(isActive: false);
    }

    private static void updateDismissButton(bool canBeDismissed)
    {
        if (dismissNotificationButton != null)
        {
            if (Provider.provider.matchmakingService.isAttemptingServerQuery)
            {
                dismissNotificationButton.text = MenuPlayConnectUI.localization.format("Cancel_Attempt_Label");
                dismissNotificationButton.tooltipText = MenuPlayConnectUI.localization.format("Cancel_Attempt_Tooltip");
            }
            else
            {
                dismissNotificationButton.text = MenuDashboardUI.localization.format("Dismiss_Notification_Label");
                dismissNotificationButton.tooltipText = MenuDashboardUI.localization.format("Dismiss_Notification_Tooltip");
            }
            dismissNotificationButton.isVisible = canBeDismissed;
        }
    }

    public static void openAlert(string message, bool canBeDismissed = true)
    {
        alertText();
        if (alertBox != null)
        {
            alertBox.text = message;
        }
        updateDismissButton(canBeDismissed);
        internalOpenAlert();
    }

    public static void closeAlert()
    {
        removeItemAlerts();
        if (alertBox != null)
        {
            alertBox.lerpPositionScale(1f, 0.5f, ESleekLerp.EXPONENTIAL, 20f);
        }
        if (container != null)
        {
            container.AnimateIntoView();
        }
        MenuDashboardUI.setCanvasActive(isActive: true);
    }

    public static void alert(string message)
    {
        openAlert(message);
        isAlerting = true;
    }

    public static void alert(string origin, ulong instanceId, int itemDefId, ushort quantity)
    {
        SteamItemDetails_t item = default(SteamItemDetails_t);
        item.m_itemId.m_SteamItemInstanceID = instanceId;
        item.m_iDefinition.m_SteamItemDef = itemDefId;
        item.m_unQuantity = quantity;
        alertNewItems(origin, new List<SteamItemDetails_t> { item });
    }

    public static void alertNewItems(string origin, List<SteamItemDetails_t> grantedItems)
    {
        if (originLabel != null)
        {
            originLabel.text = origin;
            originLabel.textColor = Provider.provider.economyService.getInventoryColor(grantedItems[0].m_iDefinition.m_SteamItemDef);
        }
        removeItemAlerts();
        itemAlerts = new SleekInventory[grantedItems.Count];
        int num = -100;
        for (int i = 0; i < grantedItems.Count; i++)
        {
            SteamItemDetails_t steamItemDetails_t = grantedItems[i];
            bool flag = i == 0;
            int num2 = (flag ? 200 : 100);
            SleekInventory sleekInventory = new SleekInventory();
            sleekInventory.positionOffset_X = num;
            sleekInventory.positionOffset_Y = (flag ? 75 : 125);
            sleekInventory.positionScale_X = 0.5f;
            sleekInventory.sizeOffset_X = num2;
            sleekInventory.sizeOffset_Y = num2;
            alertBox.AddChild(sleekInventory);
            sleekInventory.updateInventory(steamItemDetails_t.m_itemId.m_SteamItemInstanceID, steamItemDetails_t.m_iDefinition.m_SteamItemDef, steamItemDetails_t.m_unQuantity, isClickable: false, flag);
            itemAlerts[i] = sleekInventory;
            num += num2 + 5;
        }
        alertItem();
        updateDismissButton(canBeDismissed: true);
        internalOpenAlert();
        isAlerting = true;
    }

    public static void alertPurchasedItems(string origin, List<SteamItemDetails_t> grantedItems)
    {
        if (originLabel != null)
        {
            originLabel.text = origin;
            originLabel.textColor = ItemStore.PremiumColor;
        }
        removeItemAlerts();
        itemAlerts = new SleekInventory[grantedItems.Count];
        int num = grantedItems.Count * -100;
        for (int i = 0; i < grantedItems.Count; i++)
        {
            SteamItemDetails_t steamItemDetails_t = grantedItems[i];
            SleekInventory sleekInventory = new SleekInventory();
            sleekInventory.positionOffset_X = num;
            sleekInventory.positionOffset_Y = 75;
            sleekInventory.positionScale_X = 0.5f;
            sleekInventory.sizeOffset_X = 200;
            sleekInventory.sizeOffset_Y = 200;
            alertBox.AddChild(sleekInventory);
            sleekInventory.updateInventory(steamItemDetails_t.m_itemId.m_SteamItemInstanceID, steamItemDetails_t.m_iDefinition.m_SteamItemDef, steamItemDetails_t.m_unQuantity, isClickable: false, isLarge: true);
            itemAlerts[i] = sleekInventory;
            num += 200;
        }
        alertItem();
        updateDismissButton(canBeDismissed: true);
        internalOpenAlert();
        isAlerting = true;
    }

    private static void onClickedDismissNotification(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.isAttemptingServerQuery)
        {
            Provider.provider.matchmakingService.cancel();
            closeAlert();
            isAlerting = false;
        }
        else if (!MenuSurvivorsClothingUI.isCrafting)
        {
            closeAlert();
            isAlerting = false;
        }
    }

    private static void OnClickedCopyNotification(ISleekElement button)
    {
        GUIUtility.systemCopyBuffer = alertBox.text;
    }

    public static void closeAll()
    {
        MenuPauseUI.close();
        MenuCreditsUI.close();
        MenuTitleUI.close();
        MenuDashboardUI.close();
        MenuPlayUI.close();
        MenuPlaySingleplayerUI.close();
        MenuPlayMatchmakingUI.close();
        MenuPlayLobbiesUI.close();
        MenuPlayConnectUI.close();
        MenuPlayServersUI.close();
        MenuPlayServerInfoUI.close();
        MenuServerPasswordUI.close();
        MenuPlayConfigUI.close();
        MenuSurvivorsUI.close();
        ItemStoreDetailsMenu.instance.Close();
        ItemStoreCartMenu.instance.Close();
        ItemStoreMenu.instance.Close();
        MenuSurvivorsCharacterUI.close();
        MenuSurvivorsAppearanceUI.close();
        MenuSurvivorsClothingUI.close();
        MenuSurvivorsGroupUI.close();
        MenuSurvivorsClothingBoxUI.close();
        MenuSurvivorsClothingDeleteUI.close();
        MenuSurvivorsClothingInspectUI.close();
        MenuSurvivorsClothingItemUI.close();
        MenuConfigurationUI.close();
        MenuConfigurationOptionsUI.close();
        MenuConfigurationDisplayUI.close();
        MenuConfigurationGraphicsUI.close();
        MenuConfigurationControlsUI.close();
        MenuWorkshopUI.close();
        MenuWorkshopEditorUI.close();
        MenuWorkshopSubmitUI.close();
    }

    private void OnEnable()
    {
        instance = this;
        base.useGUILayout = false;
    }

    internal void Menu_OnGUI()
    {
        if (window != null)
        {
            Glazier.Get().Root = window;
        }
    }

    private void OnGUI()
    {
        MenuConfigurationControlsUI.bindOnGUI();
    }

    private void escapeMenu()
    {
        if (Provider.provider.matchmakingService.isAttemptingServerQuery)
        {
            Provider.provider.matchmakingService.cancel();
            closeAlert();
            isAlerting = false;
        }
        else
        {
            if (MenuSurvivorsClothingUI.isCrafting)
            {
                return;
            }
            if (isAlerting)
            {
                closeAlert();
                isAlerting = false;
            }
            else if (MenuPauseUI.active)
            {
                MenuPauseUI.close();
                MenuDashboardUI.open();
                MenuTitleUI.open();
            }
            else if (MenuCreditsUI.active)
            {
                MenuCreditsUI.close();
                MenuPauseUI.open();
            }
            else if (MenuTitleUI.active)
            {
                MenuPauseUI.open();
                MenuDashboardUI.close();
                MenuTitleUI.close();
            }
            else if (MenuPlayConfigUI.active)
            {
                MenuPlayConfigUI.close();
                MenuPlaySingleplayerUI.open();
            }
            else if (MenuServerPasswordUI.isActive)
            {
                MenuServerPasswordUI.close();
                MenuPlayServerInfoUI.OpenWithoutRefresh();
            }
            else if (MenuPlayServerInfoUI.active)
            {
                MenuPlayServerInfoUI.close();
                switch (MenuPlayServerInfoUI.openContext)
                {
                case MenuPlayServerInfoUI.EServerInfoOpenContext.CONNECT:
                    MenuPlayConnectUI.open();
                    break;
                case MenuPlayServerInfoUI.EServerInfoOpenContext.SERVERS:
                    MenuPlayServersUI.open();
                    break;
                case MenuPlayServerInfoUI.EServerInfoOpenContext.MATCHMAKING:
                    MenuPlayMatchmakingUI.open();
                    break;
                default:
                    UnturnedLog.info("Unknown server info open context: {0}", MenuPlayServerInfoUI.openContext);
                    break;
                }
            }
            else if (MenuPlayConnectUI.active || MenuPlayServersUI.active || MenuPlaySingleplayerUI.active || MenuPlayMatchmakingUI.active || MenuPlayLobbiesUI.active)
            {
                MenuPlayConnectUI.close();
                MenuPlayServersUI.close();
                MenuPlaySingleplayerUI.close();
                MenuPlayMatchmakingUI.close();
                MenuPlayLobbiesUI.close();
                MenuPlayUI.open();
            }
            else if (ItemStoreCartMenu.instance.IsOpen)
            {
                ItemStoreCartMenu.instance.Close();
                ItemStoreMenu.instance.Open();
            }
            else if (ItemStoreDetailsMenu.instance.IsOpen)
            {
                ItemStoreDetailsMenu.instance.Close();
                ItemStoreMenu.instance.Open();
            }
            else if (ItemStoreMenu.instance.IsOpen)
            {
                ItemStoreMenu.instance.Close();
                MenuSurvivorsClothingUI.open();
            }
            else if (MenuSurvivorsClothingItemUI.active)
            {
                MenuSurvivorsClothingItemUI.close();
                MenuSurvivorsClothingUI.open();
            }
            else if (MenuSurvivorsClothingBoxUI.active)
            {
                if (MenuSurvivorsClothingBoxUI.isUnboxing)
                {
                    MenuSurvivorsClothingBoxUI.skipAnimation();
                    return;
                }
                MenuSurvivorsClothingBoxUI.close();
                MenuSurvivorsClothingItemUI.open();
            }
            else if (MenuSurvivorsClothingInspectUI.active || MenuSurvivorsClothingDeleteUI.active)
            {
                MenuSurvivorsClothingInspectUI.close();
                MenuSurvivorsClothingDeleteUI.close();
                MenuSurvivorsClothingItemUI.open();
            }
            else if (MenuSurvivorsCharacterUI.active || MenuSurvivorsAppearanceUI.active || MenuSurvivorsGroupUI.active || MenuSurvivorsClothingUI.active)
            {
                MenuSurvivorsCharacterUI.close();
                MenuSurvivorsAppearanceUI.close();
                MenuSurvivorsGroupUI.close();
                MenuSurvivorsClothingUI.close();
                MenuSurvivorsUI.open();
            }
            else if (MenuConfigurationOptionsUI.active || MenuConfigurationControlsUI.active || MenuConfigurationGraphicsUI.active || MenuConfigurationDisplayUI.active)
            {
                MenuConfigurationOptionsUI.close();
                MenuConfigurationControlsUI.close();
                MenuConfigurationGraphicsUI.close();
                MenuConfigurationDisplayUI.close();
                MenuConfigurationUI.open();
            }
            else if (MenuWorkshopSubmitUI.active || MenuWorkshopEditorUI.active || MenuWorkshopErrorUI.active || MenuWorkshopLocalizationUI.active || MenuWorkshopSpawnsUI.active || MenuWorkshopSubscriptionsUI.active)
            {
                MenuWorkshopSubmitUI.close();
                MenuWorkshopEditorUI.close();
                MenuWorkshopErrorUI.close();
                MenuWorkshopLocalizationUI.close();
                MenuWorkshopSpawnsUI.close();
                MenuWorkshopSubscriptionsUI.instance.close();
                MenuWorkshopUI.open();
            }
            else
            {
                MenuPlayUI.close();
                MenuSurvivorsUI.close();
                MenuConfigurationUI.close();
                MenuWorkshopUI.close();
                MenuDashboardUI.open();
                MenuTitleUI.open();
            }
        }
    }

    private void tickInput()
    {
        if (MenuConfigurationControlsUI.binding != byte.MaxValue)
        {
            return;
        }
        if (InputEx.GetKeyDown(KeyCode.F1))
        {
            MenuWorkshopUI.toggleIconTools();
        }
        if (InputEx.ConsumeKeyDown(KeyCode.Escape))
        {
            escapeMenu();
        }
        if (window != null)
        {
            if (InputEx.GetKeyDown(ControlsSettings.screenshot))
            {
                Provider.RequestScreenshot();
            }
            if (InputEx.GetKeyDown(ControlsSettings.hud))
            {
                window.isEnabled = !window.isEnabled;
                window.drawCursorWhileDisabled = false;
            }
            InputEx.GetKeyDown(ControlsSettings.terminal);
        }
        if (InputEx.GetKeyDown(ControlsSettings.refreshAssets))
        {
            Assets.refresh();
        }
        if (!InputEx.GetKeyDown(ControlsSettings.clipboardDebug))
        {
            return;
        }
        if (MenuSurvivorsAppearanceUI.active)
        {
            string empty = string.Empty;
            empty = empty + "Face " + Characters.active.face;
            empty = empty + "\nHair " + Characters.active.hair;
            empty = empty + "\nBeard " + Characters.active.beard;
            empty = empty + "\nColor_Skin " + Palette.hex(Characters.active.skin);
            empty = empty + "\nColor_Hair " + Palette.hex(Characters.active.color);
            if (Characters.active.hand)
            {
                empty += "\nBackward";
            }
            GUIUtility.systemCopyBuffer = empty;
        }
        else if (MenuPlayServerInfoUI.active)
        {
            GUIUtility.systemCopyBuffer = MenuPlayServerInfoUI.GetClipboardData();
        }
    }

    private void Update()
    {
        if (window == null)
        {
            return;
        }
        MenuConfigurationControlsUI.bindUpdate();
        MenuSurvivorsClothingBoxUI.update();
        tickInput();
        window.showCursor = true;
        if (MenuPlayUI.active || MenuPlayConnectUI.active || MenuPlayServersUI.active || MenuPlayServerInfoUI.active || MenuServerPasswordUI.isActive || MenuPlaySingleplayerUI.active || MenuPlayMatchmakingUI.active || MenuPlayLobbiesUI.active || MenuPlayConfigUI.active)
        {
            target = play;
        }
        else if (MenuSurvivorsUI.active || MenuSurvivorsCharacterUI.active || MenuSurvivorsAppearanceUI.active || MenuSurvivorsGroupUI.active || MenuSurvivorsClothingUI.active || MenuSurvivorsClothingItemUI.active || MenuSurvivorsClothingInspectUI.active || MenuSurvivorsClothingDeleteUI.active || MenuSurvivorsClothingBoxUI.active || ItemStoreMenu.instance.IsOpen || ItemStoreCartMenu.instance.IsOpen || ItemStoreDetailsMenu.instance.IsOpen)
        {
            target = survivors;
        }
        else if (MenuConfigurationUI.active || MenuConfigurationOptionsUI.active || MenuConfigurationControlsUI.active || MenuConfigurationGraphicsUI.active || MenuConfigurationDisplayUI.active)
        {
            target = configuration;
        }
        else if (MenuWorkshopUI.active || MenuWorkshopSubmitUI.active || MenuWorkshopEditorUI.active || MenuWorkshopErrorUI.active || MenuWorkshopLocalizationUI.active || MenuWorkshopSpawnsUI.active || MenuWorkshopSubscriptionsUI.active)
        {
            target = workshop;
        }
        else
        {
            target = title;
        }
        if (target == title)
        {
            if (hasTitled)
            {
                base.transform.position = Vector3.Lerp(base.transform.position, target.position, Time.deltaTime * 4f);
                base.transform.rotation = Quaternion.Lerp(base.transform.rotation, target.rotation, Time.deltaTime * 4f);
            }
            else
            {
                base.transform.position = Vector3.Lerp(base.transform.position, target.position, Time.deltaTime);
                base.transform.rotation = Quaternion.Lerp(base.transform.rotation, target.rotation, Time.deltaTime);
            }
        }
        else
        {
            hasTitled = true;
            base.transform.position = Vector3.Lerp(base.transform.position, target.position, Time.deltaTime * 4f);
            base.transform.rotation = Quaternion.Lerp(base.transform.rotation, target.rotation, Time.deltaTime * 4f);
        }
    }

    internal IEnumerator requestSteamNews()
    {
        int num = Provider.statusData.News.Announcements_Count;
        if (num < 1)
        {
            UnturnedLog.warn("Not requesting Steam community announcements because count is zero");
            yield break;
        }
        if (num > 10)
        {
            num = 10;
            UnturnedLog.warn("Clamping Steam community announcements to {0}", num);
        }
        if (!Provider.allowWebRequests)
        {
            UnturnedLog.warn("Not requesting Steam community announcements because web requests are disabled");
            yield break;
        }
        string format = "http://api.steampowered.com/ISteamNews/GetNewsForApp/v0002?appid=304930&count={0}&feeds=steam_community_announcements";
        format = string.Format(format, num.ToString("D"));
        UnityWebRequest request = UnityWebRequest.Get(format);
        request.timeout = 15;
        UnturnedLog.info("Requesting {0} Steam community announcements", num);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            UnturnedLog.warn("Error requesting news: {0}", request.error);
            yield break;
        }
        try
        {
            UnturnedLog.info("Received Steam community announcements");
            MenuDashboardUI.receiveSteamNews(request.downloadHandler.text);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "News web query handled improperly!");
        }
    }

    internal IEnumerator CheckForUpdates(Action<string, bool> callback)
    {
        if (Application.isEditor)
        {
            yield break;
        }
        if (!Provider.allowWebRequests)
        {
            UnturnedLog.warn("Not checking for updates because web requests are disabled");
            yield break;
        }
        if (!SteamApps.GetCurrentBetaName(out var pchName, 64) || string.IsNullOrWhiteSpace(pchName))
        {
            UnturnedLog.warn("Unable to get current Steam beta name, defaulting to \"public\"");
            pchName = "public";
        }
        UnturnedLog.info("Checking for updates on Steam beta branch \"" + pchName + "\"...");
        string uri = "https://smartlydressedgames.com/unturned-steam-versions/" + pchName + ".txt";
        UnityWebRequest request = UnityWebRequest.Get(uri);
        request.timeout = 30;
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            string text = request.downloadHandler.text;
            if (Parser.TryGetUInt32FromIP(text, out var value))
            {
                if (value != Provider.APP_VERSION_PACKED)
                {
                    if (value > Provider.APP_VERSION_PACKED)
                    {
                        UnturnedLog.info("Detected newer game version: " + text);
                    }
                    else
                    {
                        UnturnedLog.info("Detected rollback to older game version: " + text);
                    }
                    bool arg = value < Provider.APP_VERSION_PACKED;
                    callback(text, arg);
                }
                else
                {
                    UnturnedLog.info("Game version is up-to-date");
                }
            }
            else
            {
                UnturnedLog.info("Unable to parse newest game version \"" + text + "\"");
            }
        }
        else
        {
            UnturnedLog.warn("Network error checking for updates: \"" + request.error + "\"");
        }
    }

    internal void customStart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void OnDestroy()
    {
        if (window != null)
        {
            if (dashboard != null)
            {
                dashboard.OnDestroy();
            }
            if (!Provider.isApplicationQuitting)
            {
                window.destroy();
            }
            window = null;
        }
    }
}
