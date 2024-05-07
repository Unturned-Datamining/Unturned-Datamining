using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class MenuPlayServerBookmarksUI : SleekFullscreenBox
{
    public bool active;

    private List<ServerBookmarkDetails> sortedBookmarks;

    private SleekList<ServerBookmarkDetails> list;

    private ISleekLabel tutorialBox;

    private SleekButtonIcon backButton;

    public void open()
    {
        if (!active)
        {
            active = true;
            SynchronizeSortedBookmarks();
            AnimateIntoView();
        }
    }

    public void close()
    {
        if (active)
        {
            active = false;
            AnimateOutOfView(0f, 1f);
        }
    }

    private void SynchronizeSortedBookmarks()
    {
        sortedBookmarks.Clear();
        foreach (ServerBookmarkDetails item in ServerBookmarksManager.GetList())
        {
            sortedBookmarks.Add(item);
        }
        sortedBookmarks.Sort(new ServerBookmarkComparer_NameAscending());
        list.NotifyDataChanged();
        tutorialBox.IsVisible = sortedBookmarks.Count < 1;
    }

    private void OnClickedBackButton(ISleekElement button)
    {
        MenuPlayUI.open();
        close();
    }

    private void OnClickedBookmark(ServerBookmarkDetails bookmarkDetails)
    {
        string host = bookmarkDetails.host;
        if (!MenuPlayConnectUI.TryParseHostString(host, out var address, out var steamId, out var queryPortOverride))
        {
            UnturnedLog.info("Unable to join bookmark because failed to resolve host \"" + host + "\"");
            return;
        }
        if (steamId != default(CSteamID))
        {
            UnturnedLog.info("Unable to join bookmark because it returned a server code (prefer to use A2S system for now)");
            return;
        }
        ushort newPort = ((queryPortOverride > 0) ? queryPortOverride : bookmarkDetails.queryPort);
        SteamConnectionInfo info = new SteamConnectionInfo(address.value, newPort, string.Empty);
        close();
        MenuPlayConnectUI.open();
        MenuPlayConnectUI.connect(info, shouldAutoJoin: false, MenuPlayServerInfoUI.EServerInfoOpenContext.BOOKMARKS);
    }

    private ISleekElement OnCreateBookmarkElement(ServerBookmarkDetails bookmarkDetails)
    {
        SleekServerBookmark sleekServerBookmark = new SleekServerBookmark(bookmarkDetails);
        sleekServerBookmark.OnClickedBookmark += OnClickedBookmark;
        sleekServerBookmark.SizeOffset_X = -30f;
        return sleekServerBookmark;
    }

    public MenuPlayServerBookmarksUI()
    {
        active = false;
        Local local = Localization.read("/Menu/Play/MenuPlayServerBookmarks.dat");
        sortedBookmarks = new List<ServerBookmarkDetails>();
        list = new SleekList<ServerBookmarkDetails>();
        list.SizeOffset_Y = -60f;
        list.SizeScale_X = 1f;
        list.SizeScale_Y = 1f;
        list.itemHeight = 40;
        list.scrollView.ReduceWidthWhenScrollbarVisible = false;
        list.onCreateElement = OnCreateBookmarkElement;
        list.SetData(sortedBookmarks);
        AddChild(list);
        tutorialBox = Glazier.Get().CreateBox();
        tutorialBox.SizeOffset_Y = 60f;
        tutorialBox.SizeScale_X = 1f;
        tutorialBox.PositionScale_Y = 0.5f;
        tutorialBox.PositionOffset_Y = -30f;
        tutorialBox.Text = local.format("Tutorial");
        tutorialBox.FontSize = ESleekFontSize.Medium;
        tutorialBox.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        AddChild(tutorialBox);
        tutorialBox.IsVisible = false;
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.PositionOffset_Y = -50f;
        backButton.PositionScale_Y = 1f;
        backButton.SizeOffset_X = 200f;
        backButton.SizeOffset_Y = 50f;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += OnClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        AddChild(backButton);
    }
}
