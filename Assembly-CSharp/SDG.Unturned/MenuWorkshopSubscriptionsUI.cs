using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class MenuWorkshopSubscriptionsUI
{
    private SleekFullscreenBox container;

    public static bool active;

    private SleekButtonIcon backButton;

    private ISleekBox headerBox;

    private ISleekScrollView moduleBox;

    private List<SleekManageWorkshopEntry> entryWidgets;

    private ISleekBox emptyBox;

    public static MenuWorkshopSubscriptionsUI instance { get; private set; }

    public static Local localization { get; private set; }

    public void open()
    {
        if (!active)
        {
            active = true;
            container.AnimateIntoView();
            synchronizeEntries();
        }
    }

    public void close()
    {
        if (active)
        {
            active = false;
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private bool hasEntry(PublishedFileId_t fileId)
    {
        return entryWidgets.FindIndex((SleekManageWorkshopEntry x) => x.fileId == fileId) >= 0;
    }

    private void addEntry(PublishedFileId_t fileId)
    {
        SleekManageWorkshopEntry sleekManageWorkshopEntry = new SleekManageWorkshopEntry(fileId);
        sleekManageWorkshopEntry.positionOffset_Y = entryWidgets.Count * 50;
        sleekManageWorkshopEntry.sizeOffset_Y = 40;
        sleekManageWorkshopEntry.sizeScale_X = 1f;
        moduleBox.AddChild(sleekManageWorkshopEntry);
        entryWidgets.Add(sleekManageWorkshopEntry);
    }

    private void synchronizeEntries()
    {
        if (entryWidgets == null)
        {
            entryWidgets = new List<SleekManageWorkshopEntry>();
        }
        List<SteamContent> ugc = Provider.provider.workshopService.ugc;
        if (ugc != null && entryWidgets.Count != ugc.Count)
        {
            foreach (SteamContent item in ugc)
            {
                PublishedFileId_t publishedFileID = item.publishedFileID;
                if (!hasEntry(publishedFileID))
                {
                    addEntry(publishedFileID);
                }
            }
        }
        if (entryWidgets.Count > 0)
        {
            if (emptyBox != null)
            {
                container.RemoveChild(emptyBox);
                emptyBox = null;
            }
        }
        else
        {
            emptyBox = Glazier.Get().CreateBox();
            emptyBox.positionOffset_Y = 60;
            emptyBox.sizeOffset_Y = 50;
            emptyBox.sizeScale_X = 1f;
            emptyBox.fontSize = ESleekFontSize.Medium;
            emptyBox.text = localization.format("No_Subscriptions");
            container.AddChild(emptyBox);
        }
        moduleBox.contentSizeOffset = new Vector2(0f, entryWidgets.Count * 50 - 10);
    }

    private void onClickedManageInOverlayButton(ISleekElement button)
    {
        if (Provider.provider.browserService.canOpenBrowser)
        {
            string url = $"https://steamcommunity.com/my/myworkshopfiles/?appid={Provider.APP_ID}&browsefilter=mysubscriptions";
            Provider.provider.browserService.open(url);
        }
        else
        {
            MenuUI.alert(MenuDashboardUI.localization.format("Overlay"));
        }
    }

    private void onClickedBackButton(ISleekElement button)
    {
        MenuWorkshopUI.open();
        close();
    }

    public MenuWorkshopSubscriptionsUI()
    {
        instance = this;
        localization = Localization.read("/Menu/Workshop/MenuWorkshopSubscriptions.dat");
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
        headerBox = Glazier.Get().CreateBox();
        headerBox.sizeOffset_Y = 50;
        headerBox.sizeScale_X = 1f;
        container.AddChild(headerBox);
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.positionOffset_X = 10;
        sleekLabel.sizeOffset_X = -210;
        sleekLabel.sizeOffset_Y = 30;
        sleekLabel.sizeScale_X = 1f;
        sleekLabel.fontSize = ESleekFontSize.Medium;
        sleekLabel.text = localization.format("Header");
        headerBox.AddChild(sleekLabel);
        ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
        sleekLabel2.positionOffset_X = 10;
        sleekLabel2.positionOffset_Y = -30;
        sleekLabel2.positionScale_Y = 1f;
        sleekLabel2.sizeOffset_X = -210;
        sleekLabel2.sizeOffset_Y = 30;
        sleekLabel2.sizeScale_X = 1f;
        sleekLabel2.text = localization.format("Enable_Warning");
        sleekLabel2.fontStyle = FontStyle.Italic;
        headerBox.AddChild(sleekLabel2);
        ISleekButton sleekButton = Glazier.Get().CreateButton();
        sleekButton.positionOffset_X = -210;
        sleekButton.positionOffset_Y = -15;
        sleekButton.positionScale_X = 1f;
        sleekButton.positionScale_Y = 0.5f;
        sleekButton.sizeOffset_X = 200;
        sleekButton.sizeOffset_Y = 30;
        sleekButton.text = localization.format("Manage_Label");
        sleekButton.tooltipText = localization.format("Manage_Tooltip");
        sleekButton.onClickedButton += onClickedManageInOverlayButton;
        headerBox.AddChild(sleekButton);
        moduleBox = Glazier.Get().CreateScrollView();
        moduleBox.positionOffset_Y = 60;
        moduleBox.sizeOffset_Y = -120;
        moduleBox.sizeScale_X = 1f;
        moduleBox.sizeScale_Y = 1f;
        moduleBox.scaleContentToWidth = true;
        container.AddChild(moduleBox);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.positionOffset_Y = -50;
        backButton.positionScale_Y = 1f;
        backButton.sizeOffset_X = 200;
        backButton.sizeOffset_Y = 50;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
    }
}
