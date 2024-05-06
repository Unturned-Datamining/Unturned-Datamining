using System;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Entry in the MenuPlayServerBookmarksUI list.
/// </summary>
public class SleekServerBookmark : SleekWrapper
{
    private ServerBookmarkDetails bookmarkDetails;

    private ISleekButton button;

    private SleekButtonIcon toggleBookmarkButton;

    private SleekWebImage thumbnail;

    private ISleekLabel nameLabel;

    private ISleekLabel descLabel;

    private ISleekLabel hostLabel;

    internal event Action<ServerBookmarkDetails> OnClickedBookmark;

    private void OnClickedButton(ISleekElement button)
    {
        this.OnClickedBookmark?.Invoke(bookmarkDetails);
    }

    private void OnClickedToggleBookmarkButton(ISleekElement button)
    {
        bookmarkDetails.isBookmarked = !bookmarkDetails.isBookmarked;
        if (bookmarkDetails.isBookmarked)
        {
            ServerBookmarksManager.AddBookmark(bookmarkDetails);
        }
        else
        {
            ServerBookmarksManager.RemoveBookmark(bookmarkDetails.steamId);
        }
        RefreshBookmarkButton();
    }

    private void RefreshBookmarkButton()
    {
        if (bookmarkDetails.isBookmarked)
        {
            button.IsClickable = true;
            toggleBookmarkButton.tooltip = MenuPlayServerInfoUI.localization.format("Bookmark_Off_Button");
            toggleBookmarkButton.icon = MenuPlayUI.serverListUI.icons.load<Texture2D>("Bookmark_Remove");
        }
        else
        {
            button.IsClickable = false;
            toggleBookmarkButton.tooltip = MenuPlayServerInfoUI.localization.format("Bookmark_On_Button");
            toggleBookmarkButton.icon = MenuPlayUI.serverListUI.icons.load<Texture2D>("Bookmark_Add");
        }
    }

    internal SleekServerBookmark(ServerBookmarkDetails bookmarkDetails)
    {
        this.bookmarkDetails = bookmarkDetails;
        button = Glazier.Get().CreateButton();
        button.SizeScale_X = 1f;
        button.SizeScale_Y = 1f;
        button.SizeOffset_X = -40f;
        button.OnClicked += OnClickedButton;
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.PositionOffset_X = 45f;
        nameLabel.SizeScale_X = 1f;
        nameLabel.SizeOffset_X = -45f;
        nameLabel.TextAlignment = TextAnchor.MiddleLeft;
        nameLabel.Text = bookmarkDetails.name;
        button.AddChild(nameLabel);
        if (string.IsNullOrEmpty(bookmarkDetails.description))
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
            descLabel.Text = bookmarkDetails.description;
            button.AddChild(descLabel);
        }
        if (!string.IsNullOrEmpty(bookmarkDetails.thumbnailUrl))
        {
            thumbnail = new SleekWebImage();
            thumbnail.PositionOffset_X = 4f;
            thumbnail.PositionOffset_Y = 4f;
            thumbnail.SizeOffset_X = 32f;
            thumbnail.SizeOffset_Y = 32f;
            thumbnail.Refresh(bookmarkDetails.thumbnailUrl);
            button.AddChild(thumbnail);
        }
        hostLabel = Glazier.Get().CreateLabel();
        hostLabel.PositionOffset_X = 45f;
        hostLabel.SizeScale_X = 1f;
        hostLabel.SizeOffset_X = -50f;
        hostLabel.SizeOffset_Y = 40f;
        hostLabel.TextAlignment = TextAnchor.MiddleRight;
        if (bookmarkDetails.queryPort > 0)
        {
            hostLabel.Text = $"{bookmarkDetails.host}:{bookmarkDetails.queryPort}";
        }
        else
        {
            hostLabel.Text = bookmarkDetails.host;
        }
        hostLabel.TextColor = new SleekColor(ESleekTint.FONT, 0.5f);
        button.AddChild(hostLabel);
        toggleBookmarkButton = new SleekButtonIcon(null, 20);
        toggleBookmarkButton.PositionScale_X = 1f;
        toggleBookmarkButton.PositionOffset_X = -40f;
        toggleBookmarkButton.SizeOffset_X = 40f;
        toggleBookmarkButton.SizeScale_Y = 1f;
        toggleBookmarkButton.iconPositionOffset = 10;
        toggleBookmarkButton.iconColor = ESleekTint.FOREGROUND;
        toggleBookmarkButton.onClickedButton += OnClickedToggleBookmarkButton;
        AddChild(toggleBookmarkButton);
        RefreshBookmarkButton();
        AddChild(button);
    }
}
