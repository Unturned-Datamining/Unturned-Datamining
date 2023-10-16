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
        CachedUGCDetails cachedDetails;
        bool cachedDetails2 = TempSteamworksWorkshop.getCachedDetails(fileId, out cachedDetails);
        string text = (cachedDetails2 ? cachedDetails.GetTitle() : fileId.ToString());
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        sleekBox.SizeScale_X = 1f;
        sleekBox.SizeScale_Y = 1f;
        AddChild(sleekBox);
        ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
        sleekToggle.PositionOffset_Y = -20f;
        sleekToggle.PositionScale_Y = 0.5f;
        sleekToggle.SizeOffset_X = 40f;
        sleekToggle.SizeOffset_Y = 40f;
        sleekToggle.OnValueChanged += onToggledEnabled;
        sleekToggle.Value = getEnabled();
        AddChild(sleekToggle);
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.PositionOffset_X = 40f;
        sleekLabel.PositionOffset_Y = -15f;
        sleekLabel.PositionScale_Y = 0.5f;
        sleekLabel.SizeOffset_X = -40f;
        sleekLabel.SizeOffset_Y = 30f;
        sleekLabel.SizeScale_X = 1f;
        sleekLabel.FontSize = ESleekFontSize.Medium;
        sleekLabel.TextAlignment = TextAnchor.MiddleLeft;
        sleekLabel.Text = text;
        sleekLabel.TextColor = (cachedDetails.isBannedOrPrivate ? ESleekTint.BAD : ESleekTint.FONT);
        AddChild(sleekLabel);
        float num = -5f;
        SleekWorkshopSubscriptionButton sleekWorkshopSubscriptionButton = new SleekWorkshopSubscriptionButton
        {
            PositionOffset_Y = -15f,
            PositionScale_X = 1f,
            PositionScale_Y = 0.5f,
            SizeOffset_X = 100f,
            SizeOffset_Y = 30f
        };
        num = (sleekWorkshopSubscriptionButton.PositionOffset_X = num - sleekWorkshopSubscriptionButton.SizeOffset_X);
        sleekWorkshopSubscriptionButton.subscribeText = MenuWorkshopSubscriptionsUI.localization.format("Subscribe_Label");
        sleekWorkshopSubscriptionButton.unsubscribeText = MenuWorkshopSubscriptionsUI.localization.format("Unsubscribe_Label");
        sleekWorkshopSubscriptionButton.subscribeTooltip = MenuWorkshopSubscriptionsUI.localization.format("Subscribe_Tooltip", text);
        sleekWorkshopSubscriptionButton.unsubscribeTooltip = MenuWorkshopSubscriptionsUI.localization.format("Unsubscribe_Tooltip", text);
        sleekWorkshopSubscriptionButton.fileId = fileId;
        sleekWorkshopSubscriptionButton.synchronizeText();
        AddChild(sleekWorkshopSubscriptionButton);
        num -= 5f;
        ISleekButton sleekButton = Glazier.Get().CreateButton();
        sleekButton.PositionOffset_Y = -15f;
        sleekButton.PositionScale_X = 1f;
        sleekButton.PositionScale_Y = 0.5f;
        sleekButton.SizeOffset_X = 100f;
        sleekButton.SizeOffset_Y = 30f;
        num = (sleekButton.PositionOffset_X = num - sleekButton.SizeOffset_X);
        sleekButton.Text = MenuWorkshopSubscriptionsUI.localization.format("View_Label");
        sleekButton.TooltipText = MenuWorkshopSubscriptionsUI.localization.format("View_Tooltip", text);
        sleekButton.TextAlignment = TextAnchor.MiddleCenter;
        sleekButton.OnClicked += onClickedViewButton;
        AddChild(sleekButton);
        num -= 5f;
        if (ReadWrite.SupportsOpeningFileBrowser)
        {
            if (SteamUGC.GetItemInstallInfo(fileId, out var _, out installPath, 1024u, out var punTimeStamp))
            {
                ISleekButton sleekButton2 = Glazier.Get().CreateButton();
                sleekButton2.PositionOffset_Y = -15f;
                sleekButton2.PositionScale_X = 1f;
                sleekButton2.PositionScale_Y = 0.5f;
                sleekButton2.SizeOffset_X = 100f;
                sleekButton2.SizeOffset_Y = 30f;
                num = (sleekButton2.PositionOffset_X = num - sleekButton2.SizeOffset_X);
                sleekButton2.Text = MenuWorkshopSubscriptionsUI.localization.format("BrowseFiles_Label");
                sleekButton2.TooltipText = MenuWorkshopSubscriptionsUI.localization.format("BrowseFiles_Tooltip", text);
                sleekButton2.OnClicked += OnClickedBrowseFilesButton;
                AddChild(sleekButton2);
                num -= 5f;
            }
            else
            {
                ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
                sleekLabel2.PositionOffset_Y = -15f;
                sleekLabel2.PositionScale_X = 1f;
                sleekLabel2.PositionScale_Y = 0.5f;
                sleekLabel2.SizeOffset_X = 100f;
                sleekLabel2.SizeOffset_Y = 30f;
                num = (sleekLabel2.PositionOffset_X = num - sleekLabel2.SizeOffset_X);
                sleekLabel2.Text = MenuWorkshopSubscriptionsUI.localization.format("NotInstalledLabel");
                sleekLabel2.TextColor = ESleekTint.BAD;
                AddChild(sleekLabel2);
                num -= 5f;
            }
            ISleekLabel sleekLabel3 = Glazier.Get().CreateLabel();
            sleekLabel3.PositionScale_X = 1f;
            sleekLabel3.SizeOffset_X = 150f;
            sleekLabel3.SizeScale_Y = 1f;
            num = (sleekLabel3.PositionOffset_X = num - sleekLabel3.SizeOffset_X);
            sleekLabel3.Text = MenuWorkshopSubscriptionsUI.localization.format("LocalTimestampLabel") + "\n" + DateTimeEx.FromUtcUnixTimeSeconds(punTimeStamp).ToLocalTime();
            sleekLabel3.FontSize = ESleekFontSize.Small;
            AddChild(sleekLabel3);
            num -= 5f;
        }
        if (cachedDetails2)
        {
            ISleekLabel sleekLabel4 = Glazier.Get().CreateLabel();
            sleekLabel4.PositionScale_X = 1f;
            sleekLabel4.SizeOffset_X = 150f;
            sleekLabel4.SizeScale_Y = 1f;
            num = (sleekLabel4.PositionOffset_X = num - sleekLabel4.SizeOffset_X);
            sleekLabel4.Text = MenuWorkshopSubscriptionsUI.localization.format("RemoteTimestampLabel") + "\n" + DateTimeEx.FromUtcUnixTimeSeconds(cachedDetails.updateTimestamp).ToLocalTime();
            sleekLabel4.FontSize = ESleekFontSize.Small;
            AddChild(sleekLabel4);
            num -= 5f;
        }
        if ((SteamUGC.GetItemState(fileId) & 8) == 8)
        {
            ISleekLabel sleekLabel5 = Glazier.Get().CreateLabel();
            sleekLabel5.PositionOffset_Y = -15f;
            sleekLabel5.PositionScale_X = 1f;
            sleekLabel5.PositionScale_Y = 0.5f;
            sleekLabel5.SizeOffset_X = 100f;
            sleekLabel5.SizeOffset_Y = 30f;
            num -= sleekLabel5.SizeOffset_X;
            sleekLabel5.PositionOffset_X = num;
            sleekLabel5.Text = MenuWorkshopSubscriptionsUI.localization.format("ItemState_NeedsUpdate");
            sleekLabel5.TextColor = ESleekTint.BAD;
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
