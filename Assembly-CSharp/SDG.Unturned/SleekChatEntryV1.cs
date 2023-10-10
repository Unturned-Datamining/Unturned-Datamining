using UnityEngine;

namespace SDG.Unturned;

public class SleekChatEntryV1 : SleekWrapper
{
    public bool shouldFadeOutWithAge;

    protected ReceivedChatMessage _representingChatMessage;

    private ISleekImage avatarImage;

    private SleekWebImage remoteImage;

    private ISleekLabel contentsLabel;

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
            Color color = avatarImage.TintColor;
            color.a = a;
            avatarImage.TintColor = color;
            remoteImage.color = color;
            Color color2 = contentsLabel.TextColor;
            color2.a = a;
            contentsLabel.TextColor = color2;
        }
    }

    public SleekChatEntryV1()
    {
        avatarImage = Glazier.Get().CreateImage();
        avatarImage.PositionOffset_Y = 4f;
        avatarImage.SizeOffset_X = 32f;
        avatarImage.SizeOffset_Y = 32f;
        avatarImage.IsVisible = false;
        AddChild(avatarImage);
        remoteImage = new SleekWebImage();
        remoteImage.PositionOffset_Y = 4f;
        remoteImage.SizeOffset_X = 32f;
        remoteImage.SizeOffset_Y = 32f;
        remoteImage.IsVisible = false;
        AddChild(remoteImage);
        contentsLabel = Glazier.Get().CreateLabel();
        contentsLabel.PositionOffset_X = 40f;
        contentsLabel.PositionOffset_Y = -4f;
        contentsLabel.SizeOffset_X = -40f;
        contentsLabel.SizeOffset_Y = 48f;
        contentsLabel.SizeScale_X = 1f;
        contentsLabel.FontSize = ESleekFontSize.Medium;
        contentsLabel.TextAlignment = TextAnchor.MiddleLeft;
        contentsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        AddChild(contentsLabel);
    }
}
