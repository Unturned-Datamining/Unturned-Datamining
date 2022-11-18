using UnityEngine;

namespace SDG.Unturned;

public class SleekChat : SleekWrapper
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
                avatarImage.texture = texture;
                avatarImage.isVisible = true;
                remoteImage.isVisible = false;
            }
            else
            {
                remoteImage.Refresh(_representingChatMessage.iconURL);
                avatarImage.isVisible = false;
                remoteImage.isVisible = true;
            }
            contentsLabel.textColor = _representingChatMessage.color;
            contentsLabel.enableRichText = _representingChatMessage.useRichTextFormatting;
            contentsLabel.text = _representingChatMessage.contents;
        }
    }

    public override void OnUpdate()
    {
        if (shouldFadeOutWithAge)
        {
            float value = representingChatMessage.age - Provider.preferenceData.Chat.Fade_Delay;
            value = Mathf.Clamp01(value);
            float a = 1f - value;
            Color color = avatarImage.color;
            color.a = a;
            avatarImage.color = color;
            remoteImage.color = color;
            Color color2 = contentsLabel.textColor;
            color2.a = a;
            contentsLabel.textColor = color2;
        }
    }

    public SleekChat()
    {
        avatarImage = Glazier.Get().CreateImage();
        avatarImage.positionOffset_Y = 4;
        avatarImage.sizeOffset_X = 32;
        avatarImage.sizeOffset_Y = 32;
        avatarImage.isVisible = false;
        AddChild(avatarImage);
        remoteImage = new SleekWebImage();
        remoteImage.positionOffset_Y = 4;
        remoteImage.sizeOffset_X = 32;
        remoteImage.sizeOffset_Y = 32;
        remoteImage.isVisible = false;
        AddChild(remoteImage);
        contentsLabel = Glazier.Get().CreateLabel();
        contentsLabel.positionOffset_X = 40;
        contentsLabel.positionOffset_Y = -4;
        contentsLabel.sizeOffset_X = -40;
        contentsLabel.sizeOffset_Y = 48;
        contentsLabel.sizeScale_X = 1f;
        contentsLabel.fontSize = ESleekFontSize.Medium;
        contentsLabel.fontAlignment = TextAnchor.MiddleLeft;
        contentsLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        AddChild(contentsLabel);
    }
}
