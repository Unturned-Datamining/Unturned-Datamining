using SDG.Provider;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class SleekManageWorkshopEntry : SleekWrapper
{
    public PublishedFileId_t fileId;

    private string installPath;

    public SleekManageWorkshopEntry(PublishedFileId_t fileId)
    {
        this.fileId = fileId;
        TempSteamworksWorkshop.getCachedDetails(fileId, out var cachedDetails);
        string title = cachedDetails.GetTitle();
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        sleekBox.sizeScale_X = 1f;
        sleekBox.sizeScale_Y = 1f;
        AddChild(sleekBox);
        ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
        sleekToggle.positionOffset_Y = -20;
        sleekToggle.positionScale_Y = 0.5f;
        sleekToggle.sizeOffset_X = 40;
        sleekToggle.sizeOffset_Y = 40;
        sleekToggle.onToggled += onToggledEnabled;
        sleekToggle.state = getEnabled();
        AddChild(sleekToggle);
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.positionOffset_X = 40;
        sleekLabel.positionOffset_Y = -15;
        sleekLabel.positionScale_Y = 0.5f;
        sleekLabel.sizeOffset_X = -40;
        sleekLabel.sizeOffset_Y = 30;
        sleekLabel.sizeScale_X = 1f;
        sleekLabel.fontSize = ESleekFontSize.Medium;
        sleekLabel.fontAlignment = TextAnchor.MiddleLeft;
        sleekLabel.text = title;
        sleekLabel.textColor = (cachedDetails.isBannedOrPrivate ? ESleekTint.BAD : ESleekTint.FONT);
        AddChild(sleekLabel);
        int num = -5;
        SleekWorkshopSubscriptionButton sleekWorkshopSubscriptionButton = new SleekWorkshopSubscriptionButton
        {
            positionOffset_Y = -15,
            positionScale_X = 1f,
            positionScale_Y = 0.5f,
            sizeOffset_X = 100,
            sizeOffset_Y = 30
        };
        num = (sleekWorkshopSubscriptionButton.positionOffset_X = num - sleekWorkshopSubscriptionButton.sizeOffset_X);
        sleekWorkshopSubscriptionButton.subscribeText = MenuWorkshopSubscriptionsUI.localization.format("Subscribe_Label");
        sleekWorkshopSubscriptionButton.unsubscribeText = MenuWorkshopSubscriptionsUI.localization.format("Unsubscribe_Label");
        sleekWorkshopSubscriptionButton.subscribeTooltip = MenuWorkshopSubscriptionsUI.localization.format("Subscribe_Tooltip", title);
        sleekWorkshopSubscriptionButton.unsubscribeTooltip = MenuWorkshopSubscriptionsUI.localization.format("Unsubscribe_Tooltip", title);
        sleekWorkshopSubscriptionButton.fileId = fileId;
        sleekWorkshopSubscriptionButton.synchronizeText();
        AddChild(sleekWorkshopSubscriptionButton);
        num -= 5;
        ISleekButton sleekButton = Glazier.Get().CreateButton();
        sleekButton.positionOffset_Y = -15;
        sleekButton.positionScale_X = 1f;
        sleekButton.positionScale_Y = 0.5f;
        sleekButton.sizeOffset_X = 100;
        sleekButton.sizeOffset_Y = 30;
        num = (sleekButton.positionOffset_X = num - sleekButton.sizeOffset_X);
        sleekButton.text = MenuWorkshopSubscriptionsUI.localization.format("View_Label");
        sleekButton.tooltipText = MenuWorkshopSubscriptionsUI.localization.format("View_Tooltip", title);
        sleekButton.fontAlignment = TextAnchor.MiddleCenter;
        sleekButton.onClickedButton += onClickedViewButton;
        AddChild(sleekButton);
        num -= 5;
        if (ReadWrite.SupportsOpeningFileBrowser)
        {
            if (SteamUGC.GetItemInstallInfo(fileId, out var _, out installPath, 1024u, out var punTimeStamp))
            {
                ISleekButton sleekButton2 = Glazier.Get().CreateButton();
                sleekButton2.positionOffset_Y = -15;
                sleekButton2.positionScale_X = 1f;
                sleekButton2.positionScale_Y = 0.5f;
                sleekButton2.sizeOffset_X = 100;
                sleekButton2.sizeOffset_Y = 30;
                num = (sleekButton2.positionOffset_X = num - sleekButton2.sizeOffset_X);
                sleekButton2.text = MenuWorkshopSubscriptionsUI.localization.format("BrowseFiles_Label");
                sleekButton2.tooltipText = MenuWorkshopSubscriptionsUI.localization.format("BrowseFiles_Tooltip", title);
                sleekButton2.onClickedButton += OnClickedBrowseFilesButton;
                AddChild(sleekButton2);
                num -= 5;
            }
            else
            {
                ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
                sleekLabel2.positionOffset_Y = -15;
                sleekLabel2.positionScale_X = 1f;
                sleekLabel2.positionScale_Y = 0.5f;
                sleekLabel2.sizeOffset_X = 100;
                sleekLabel2.sizeOffset_Y = 30;
                num = (sleekLabel2.positionOffset_X = num - sleekLabel2.sizeOffset_X);
                sleekLabel2.text = MenuWorkshopSubscriptionsUI.localization.format("NotInstalledLabel");
                sleekLabel2.textColor = ESleekTint.BAD;
                AddChild(sleekLabel2);
                num -= 5;
            }
            ISleekLabel sleekLabel3 = Glazier.Get().CreateLabel();
            sleekLabel3.positionScale_X = 1f;
            sleekLabel3.sizeOffset_X = 150;
            sleekLabel3.sizeScale_Y = 1f;
            num = (sleekLabel3.positionOffset_X = num - sleekLabel3.sizeOffset_X);
            sleekLabel3.text = MenuWorkshopSubscriptionsUI.localization.format("LocalTimestampLabel") + "\n" + DateTimeEx.FromUtcUnixTimeSeconds(punTimeStamp).ToLocalTime().ToString();
            sleekLabel3.fontSize = ESleekFontSize.Small;
            AddChild(sleekLabel3);
            num -= 5;
        }
        ISleekLabel sleekLabel4 = Glazier.Get().CreateLabel();
        sleekLabel4.positionScale_X = 1f;
        sleekLabel4.sizeOffset_X = 150;
        sleekLabel4.sizeScale_Y = 1f;
        num = (sleekLabel4.positionOffset_X = num - sleekLabel4.sizeOffset_X);
        sleekLabel4.text = MenuWorkshopSubscriptionsUI.localization.format("RemoteTimestampLabel") + "\n" + DateTimeEx.FromUtcUnixTimeSeconds(cachedDetails.updateTimestamp).ToLocalTime().ToString();
        sleekLabel4.fontSize = ESleekFontSize.Small;
        AddChild(sleekLabel4);
        num -= 5;
        if ((SteamUGC.GetItemState(fileId) & 8) == 8)
        {
            ISleekLabel sleekLabel5 = Glazier.Get().CreateLabel();
            sleekLabel5.positionOffset_Y = -15;
            sleekLabel5.positionScale_X = 1f;
            sleekLabel5.positionScale_Y = 0.5f;
            sleekLabel5.sizeOffset_X = 100;
            sleekLabel5.sizeOffset_Y = 30;
            num = (sleekLabel5.positionOffset_X = num - sleekLabel5.sizeOffset_X);
            sleekLabel5.text = MenuWorkshopSubscriptionsUI.localization.format("ItemState_NeedsUpdate");
            sleekLabel5.textColor = ESleekTint.BAD;
            AddChild(sleekLabel5);
        }
    }

    protected bool getEnabled()
    {
        return LocalWorkshopSettings.get().getEnabled(fileId);
    }

    protected void setEnabled(bool newEnabled)
    {
        LocalWorkshopSettings.get().setEnabled(fileId, newEnabled);
    }

    protected void onToggledEnabled(ISleekToggle toggle, bool state)
    {
        setEnabled(state);
    }

    protected void OnClickedBrowseFilesButton(ISleekElement button)
    {
        ReadWrite.OpenFileBrowser(installPath);
    }

    protected void onClickedViewButton(ISleekElement viewButton)
    {
        if (Provider.provider.browserService.canOpenBrowser)
        {
            PublishedFileId_t publishedFileId_t = fileId;
            string url = "http://steamcommunity.com/sharedfiles/filedetails/?id=" + publishedFileId_t.ToString();
            Provider.provider.browserService.open(url);
        }
        else
        {
            MenuUI.alert(MenuDashboardUI.localization.format("Overlay"));
        }
    }
}
