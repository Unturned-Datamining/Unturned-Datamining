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
        sleekManageWorkshopEntry.PositionOffset_Y = entryWidgets.Count * 50;
        sleekManageWorkshopEntry.SizeOffset_Y = 40f;
        sleekManageWorkshopEntry.SizeScale_X = 1f;
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
            emptyBox.PositionOffset_Y = 60f;
            emptyBox.SizeOffset_Y = 50f;
            emptyBox.SizeScale_X = 1f;
            emptyBox.FontSize = ESleekFontSize.Medium;
            emptyBox.Text = localization.format("No_Subscriptions");
            container.AddChild(emptyBox);
        }
        moduleBox.ContentSizeOffset = new Vector2(0f, entryWidgets.Count * 50 - 10);
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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        headerBox = Glazier.Get().CreateBox();
        headerBox.SizeOffset_Y = 50f;
        headerBox.SizeScale_X = 1f;
        container.AddChild(headerBox);
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.PositionOffset_X = 10f;
        sleekLabel.SizeOffset_X = -210f;
        sleekLabel.SizeOffset_Y = 30f;
        sleekLabel.SizeScale_X = 1f;
        sleekLabel.FontSize = ESleekFontSize.Medium;
        sleekLabel.Text = localization.format("Header");
        headerBox.AddChild(sleekLabel);
        ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
        sleekLabel2.PositionOffset_X = 10f;
        sleekLabel2.PositionOffset_Y = -30f;
        sleekLabel2.PositionScale_Y = 1f;
        sleekLabel2.SizeOffset_X = -210f;
        sleekLabel2.SizeOffset_Y = 30f;
        sleekLabel2.SizeScale_X = 1f;
        sleekLabel2.Text = localization.format("Enable_Warning");
        sleekLabel2.FontStyle = FontStyle.Italic;
        headerBox.AddChild(sleekLabel2);
        ISleekButton sleekButton = Glazier.Get().CreateButton();
        sleekButton.PositionOffset_X = -210f;
        sleekButton.PositionOffset_Y = -15f;
        sleekButton.PositionScale_X = 1f;
        sleekButton.PositionScale_Y = 0.5f;
        sleekButton.SizeOffset_X = 200f;
        sleekButton.SizeOffset_Y = 30f;
        sleekButton.Text = localization.format("Manage_Label");
        sleekButton.TooltipText = localization.format("Manage_Tooltip");
        sleekButton.OnClicked += onClickedManageInOverlayButton;
        headerBox.AddChild(sleekButton);
        moduleBox = Glazier.Get().CreateScrollView();
        moduleBox.PositionOffset_Y = 60f;
        moduleBox.SizeOffset_Y = -120f;
        moduleBox.SizeScale_X = 1f;
        moduleBox.SizeScale_Y = 1f;
        moduleBox.ScaleContentToWidth = true;
        container.AddChild(moduleBox);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.PositionOffset_Y = -50f;
        backButton.PositionScale_Y = 1f;
        backButton.SizeOffset_X = 200f;
        backButton.SizeOffset_Y = 50f;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
    }
}
