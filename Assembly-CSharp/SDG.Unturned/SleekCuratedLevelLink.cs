using UnityEngine;

namespace SDG.Unturned;

public class SleekCuratedLevelLink : SleekWrapper
{
    private CuratedMapLink curatedMap;

    private ISleekBox backdrop;

    private ISleekButton viewOnWorkshopButton;

    private ISleekButton manageButton;

    private ISleekImage icon;

    private ISleekLabel nameLabel;

    private void onClickedViewButton(ISleekElement button)
    {
        if (curatedMap != null)
        {
            if (Provider.provider.browserService.canOpenBrowser)
            {
                string url = "http://steamcommunity.com/sharedfiles/filedetails/?id=" + curatedMap.Workshop_File_Id;
                Provider.provider.browserService.open(url);
            }
            else
            {
                MenuUI.alert(MenuDashboardUI.localization.format("Overlay"));
            }
        }
    }

    private bool getSubscribed()
    {
        return Provider.provider.workshopService.getSubscribed(curatedMap.Workshop_File_Id);
    }

    private void setSubscribed(bool subscribe)
    {
        Provider.provider.workshopService.setSubscribed(curatedMap.Workshop_File_Id, subscribe);
        if (subscribe)
        {
            ulong[] required_Workshop_File_Ids = curatedMap.Required_Workshop_File_Ids;
            foreach (ulong fileId in required_Workshop_File_Ids)
            {
                Provider.provider.workshopService.setSubscribed(fileId, subscribe);
            }
        }
    }

    private void onClickedManageButton(ISleekElement button)
    {
        if (curatedMap != null)
        {
            bool subscribed = !getSubscribed();
            updateManageLabel(subscribed);
            setSubscribed(subscribed);
        }
    }

    private void updateManageLabel()
    {
        if (curatedMap != null)
        {
            bool subscribed = getSubscribed();
            updateManageLabel(subscribed);
        }
    }

    private void updateManageLabel(bool subscribed)
    {
        manageButton.text = MenuPlaySingleplayerUI.localization.format(subscribed ? "Retired_Manage_Unsub" : "Retired_Manage_Sub");
    }

    public SleekCuratedLevelLink(CuratedMapLink curatedMap)
    {
        this.curatedMap = curatedMap;
        base.sizeOffset_X = 400;
        base.sizeOffset_Y = 100;
        backdrop = Glazier.Get().CreateBox();
        backdrop.sizeOffset_X = 0;
        backdrop.sizeOffset_Y = 0;
        backdrop.sizeScale_X = 1f;
        backdrop.sizeScale_Y = 1f;
        AddChild(backdrop);
        string path = "/CuratedMapIcons/" + curatedMap.Workshop_File_Id + ".png";
        if (ReadWrite.fileExists(path, useCloud: false, usePath: true))
        {
            icon = Glazier.Get().CreateImage();
            icon.positionOffset_X = 10;
            icon.positionOffset_Y = 10;
            icon.sizeOffset_X = -20;
            icon.sizeOffset_Y = -20;
            icon.sizeScale_X = 1f;
            icon.sizeScale_Y = 1f;
            icon.texture = ReadWrite.readTextureFromFile(path, useBasePath: true);
            icon.shouldDestroyTexture = true;
            backdrop.AddChild(icon);
        }
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.positionOffset_Y = 10;
        nameLabel.sizeScale_X = 1f;
        nameLabel.sizeOffset_Y = 50;
        nameLabel.fontAlignment = TextAnchor.MiddleCenter;
        nameLabel.fontSize = ESleekFontSize.Medium;
        nameLabel.text = curatedMap.Name;
        nameLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        backdrop.AddChild(nameLabel);
        viewOnWorkshopButton = Glazier.Get().CreateButton();
        viewOnWorkshopButton.positionOffset_X = 15;
        viewOnWorkshopButton.positionOffset_Y = -35;
        viewOnWorkshopButton.positionScale_Y = 1f;
        viewOnWorkshopButton.sizeOffset_X = 150;
        viewOnWorkshopButton.sizeOffset_Y = 20;
        viewOnWorkshopButton.fontSize = ESleekFontSize.Small;
        viewOnWorkshopButton.text = MenuPlaySingleplayerUI.localization.format("Retired_View_Label");
        viewOnWorkshopButton.tooltipText = MenuPlaySingleplayerUI.localization.format("Retired_View_Tooltip");
        viewOnWorkshopButton.onClickedButton += onClickedViewButton;
        backdrop.AddChild(viewOnWorkshopButton);
        manageButton = Glazier.Get().CreateButton();
        manageButton.positionOffset_X = -165;
        manageButton.positionOffset_Y = -35;
        manageButton.positionScale_X = 1f;
        manageButton.positionScale_Y = 1f;
        manageButton.sizeOffset_X = 150;
        manageButton.sizeOffset_Y = 20;
        manageButton.fontSize = ESleekFontSize.Small;
        manageButton.tooltipText = MenuPlaySingleplayerUI.localization.format("Retired_Manage_Tooltip");
        updateManageLabel();
        manageButton.onClickedButton += onClickedManageButton;
        backdrop.AddChild(manageButton);
        if (!Provider.statusData.News.isFeatured(curatedMap.Workshop_File_Id) || !LocalNews.isNowWithinFeaturedWorkshopWindow())
        {
            return;
        }
        EMapStatus featured_Workshop_Status = Provider.statusData.News.Featured_Workshop_Status;
        if (featured_Workshop_Status != 0)
        {
            SleekNew sleekNew = new SleekNew(featured_Workshop_Status == EMapStatus.UPDATED);
            if (icon != null)
            {
                icon.AddChild(sleekNew);
            }
            else
            {
                AddChild(sleekNew);
            }
        }
    }
}
