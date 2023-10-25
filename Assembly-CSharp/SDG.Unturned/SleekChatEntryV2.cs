using UnityEngine;

namespace SDG.Unturned;

public class SleekChatEntryV2 : SleekWrapper
{
    /// <summary>
    /// Does this label fade out as the chat message gets older?
    /// </summary>
    public bool shouldFadeOutWithAge;

    public bool forceVisibleWhileBrowsingChatHistory;

    protected ReceivedChatMessage _representingChatMessage;

    private ISleekImage avatarImage;

    private SleekWebImage remoteImage;

    private ISleekLabel contentsLabel;

    /// <summary>
    /// Chat message values to show.
    /// </summary>
    public ReceivedChatMessage representingChatMessage
    {
        get
        {
            return _representingChatMessage;
        }
        set
        {
            _representingChatMessage = value;
            if (string.IsNullOrEmpty(_representingChatMessage.iconURL))
            {
                Texture2D texture = ((!OptionsSettings.streamer && _representingChatMessage.speaker != null) ? Provider.provider.communityService.getIcon(_representingChatMessage.speaker.playerID.steamID, shouldCache: true) : null);
                avatarImage.Texture = texture;
                avatarImage.IsVisible = true;
                remoteImage.IsVisible = false;
            }
            else
            {
                remoteImage.Refresh(_representingChatMessage.iconURL);
                avatarImage.IsVisible = false;
                remoteImage.IsVisible = true;
            }
            contentsLabel.TextColor = _representingChatMessage.color;
            contentsLabel.AllowRichText = _representingChatMessage.useRichTextFormatting;
            contentsLabel.Text = _representingChatMessage.contents;
        }
    }

    public override void OnUpdate()
    {
        if (shouldFadeOutWithAge)
        {
            float value = representingChatMessage.age - Provider.preferenceData.Chat.Fade_Delay;
            value = Mathf.Clamp01(value);
            float a = 1f - value;
            if (forceVisibleWhileBrowsingChatHistory)
            {
                a = 1f;
            }
            Color color = avatarImage.TintColor;
            color.a = a;
            avatarImage.TintColor = color;
            remoteImage.color = color;
            Color color2 = contentsLabel.TextColor;
            color2.a = a;
            contentsLabel.TextColor = color2;
        }
    }

    public SleekChatEntryV2()
    {
        UseManualLayout = false;
        base.UseChildAutoLayout = ESleekChildLayout.Horizontal;
        base.ChildPerpendicularAlignment = ESleekChildPerpendicularAlignment.Top;
        ISleekElement sleekElement = Glazier.Get().CreateFrame();
        sleekElement.UseManualLayout = false;
        sleekElement.UseWidthLayoutOverride = true;
        sleekElement.UseHeightLayoutOverride = true;
        sleekElement.SizeOffset_X = 40f;
        sleekElement.SizeOffset_Y = 40f;
        AddChild(sleekElement);
        avatarImage = Glazier.Get().CreateImage();
        avatarImage.PositionOffset_X = 4f;
        avatarImage.PositionOffset_Y = 4f;
        avatarImage.SizeOffset_X = 32f;
        avatarImage.SizeOffset_Y = 32f;
        avatarImage.IsVisible = false;
        sleekElement.AddChild(avatarImage);
        remoteImage = new SleekWebImage();
        remoteImage.PositionOffset_X = 4f;
        remoteImage.PositionOffset_Y = 4f;
        remoteImage.SizeOffset_X = 32f;
        remoteImage.SizeOffset_Y = 32f;
        remoteImage.IsVisible = false;
        sleekElement.AddChild(remoteImage);
        ISleekElement sleekElement2 = Glazier.Get().CreateFrame();
        sleekElement2.UseManualLayout = false;
        sleekElement2.UseChildAutoLayout = ESleekChildLayout.Vertical;
        sleekElement2.ExpandChildren = true;
        AddChild(sleekElement2);
        contentsLabel = Glazier.Get().CreateLabel();
        contentsLabel.UseManualLayout = false;
        contentsLabel.FontSize = ESleekFontSize.Medium;
        contentsLabel.TextAlignment = TextAnchor.MiddleLeft;
        contentsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        sleekElement2.AddChild(contentsLabel);
    }
}
