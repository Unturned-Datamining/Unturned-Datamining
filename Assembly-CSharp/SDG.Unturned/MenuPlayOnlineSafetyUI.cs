using UnityEngine;

namespace SDG.Unturned;

public class MenuPlayOnlineSafetyUI : SleekFullscreenBox
{
    public Local localization;

    public Bundle icons;

    public bool active;

    private EOnlineSafetyDestination destination;

    private ISleekToggle profanityFilterToggle;

    private ISleekLabel profanityFilter_Header;

    private ISleekToggle inboundVoiceChatToggle;

    private ISleekLabel inboundVoiceChat_Header;

    private ISleekToggle outboundVoiceChatToggle;

    private ISleekLabel outboundVoiceChat_Header;

    private ISleekLabel outboundVoiceChat_Description;

    private ISleekToggle streamerModeToggle;

    private ISleekLabel streamerMode_Header;

    private ISleekToggle dontShowAgainToggle;

    public void OpenIfNecessary(EOnlineSafetyDestination destination)
    {
        if (OptionsSettings.ShouldShowOnlineSafetyMenu)
        {
            open(destination);
            return;
        }
        this.destination = destination;
        ProceedToDestination();
    }

    public void open(EOnlineSafetyDestination destination)
    {
        if (!active)
        {
            active = true;
            this.destination = destination;
            SynchronizeValues();
            AnimateIntoView();
        }
    }

    public void close()
    {
        if (active)
        {
            MenuSettings.SaveOptionsIfLoaded();
            active = false;
            AnimateOutOfView(0f, 1f);
        }
    }

    private void ProceedToDestination()
    {
        switch (destination)
        {
        case EOnlineSafetyDestination.Connect:
            MenuPlayConnectUI.open();
            break;
        case EOnlineSafetyDestination.ServerList:
            MenuPlayUI.serverListUI.open(shouldRefresh: true);
            break;
        case EOnlineSafetyDestination.Bookmarks:
            MenuPlayUI.serverBookmarksUI.open();
            break;
        case EOnlineSafetyDestination.Lobby:
            MenuPlayLobbiesUI.open();
            break;
        }
    }

    private void OnBackClicked(ISleekElement button)
    {
        MenuPlayUI.open();
        close();
    }

    private void OnContinueClicked(ISleekElement button)
    {
        OptionsSettings.onlineSafetyMenuProceedCount++;
        OptionsSettings.didProceedThroughOnlineSafetyMenuThisSession = true;
        ProceedToDestination();
        close();
    }

    private void OnProfanityFilterToggled(ISleekToggle toggle, bool state)
    {
        OptionsSettings.filter = state;
        SynchronizeValues();
    }

    private void OnInboundVoiceChatToggled(ISleekToggle toggle, bool state)
    {
        OptionsSettings.chatVoiceIn = state;
        OptionsSettings.chatVoiceOut &= state;
        SynchronizeValues();
    }

    private void OnOutboundVoiceChatToggled(ISleekToggle toggle, bool state)
    {
        OptionsSettings.chatVoiceOut = state;
        SynchronizeValues();
    }

    private void OnStreamerModeToggled(ISleekToggle toggle, bool state)
    {
        OptionsSettings.streamer = state;
        SynchronizeValues();
    }

    private void OnDontShowAgainToggled(ISleekToggle toggle, bool state)
    {
        OptionsSettings.wantsToHideOnlineSafetyMenu = state;
    }

    private void SynchronizeValues()
    {
        profanityFilterToggle.Value = OptionsSettings.filter;
        profanityFilter_Header.Text = localization.format("ProfanityFilter_Header", localization.format(OptionsSettings.filter ? "Feature_On" : "Feature_Off"));
        inboundVoiceChatToggle.Value = OptionsSettings.chatVoiceIn;
        inboundVoiceChat_Header.Text = localization.format("InboundVoiceChat_Header", localization.format(OptionsSettings.chatVoiceIn ? "Feature_On" : "Feature_Off"));
        outboundVoiceChatToggle.Value = OptionsSettings.chatVoiceOut;
        outboundVoiceChat_Header.Text = localization.format("OutboundVoiceChat_Header", localization.format(OptionsSettings.chatVoiceOut ? "Feature_On" : "Feature_Off"));
        outboundVoiceChat_Description.Text = localization.format("OutboundVoiceChat_Description", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.voice));
        outboundVoiceChatToggle.IsInteractable = OptionsSettings.chatVoiceIn;
        outboundVoiceChat_Header.TextColor = new SleekColor(ESleekTint.FONT, OptionsSettings.chatVoiceIn ? 1f : 0.5f);
        outboundVoiceChat_Description.TextColor = new SleekColor(ESleekTint.RICH_TEXT_DEFAULT, OptionsSettings.chatVoiceIn ? 1f : 0.5f);
        streamerModeToggle.Value = OptionsSettings.streamer;
        streamerMode_Header.Text = localization.format("StreamerMode_Header", localization.format(OptionsSettings.streamer ? "Feature_On" : "Feature_Off"));
        dontShowAgainToggle.Value = OptionsSettings.wantsToHideOnlineSafetyMenu;
    }

    public MenuPlayOnlineSafetyUI()
    {
        active = false;
        localization = Localization.read("/Menu/Play/MenuPlayOnlineSafety.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Play/MenuPlayOnlineSafety/MenuPlayOnlineSafety.unity3d");
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        sleekBox.SizeScale_X = 1f;
        sleekBox.SizeScale_Y = 1f;
        sleekBox.BackgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
        AddChild(sleekBox);
        ISleekScrollView sleekScrollView = Glazier.Get().CreateScrollView();
        sleekScrollView.PositionOffset_X = -380f;
        sleekScrollView.PositionScale_X = 0.5f;
        sleekScrollView.PositionScale_Y = 0.1f;
        sleekScrollView.SizeOffset_X = 790f;
        sleekScrollView.SizeScale_Y = 0.8f;
        sleekScrollView.ScaleContentToWidth = true;
        AddChild(sleekScrollView);
        float num = 0f;
        ISleekImage sleekImage = Glazier.Get().CreateImage(bundle.load<Texture2D>("OnlineSafetyAlert"));
        sleekImage.PositionScale_X = 0.5f;
        sleekImage.PositionOffset_X = -64f;
        sleekImage.PositionOffset_Y = num;
        sleekImage.SizeOffset_X = 128f;
        sleekImage.SizeOffset_Y = 128f;
        sleekImage.TintColor = ESleekTint.FOREGROUND;
        sleekScrollView.AddChild(sleekImage);
        num += 128f;
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.PositionOffset_Y = num;
        sleekLabel.SizeScale_X = 1f;
        sleekLabel.SizeOffset_Y = 50f;
        sleekLabel.Text = localization.format("Header");
        sleekLabel.FontSize = ESleekFontSize.Large;
        sleekLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        sleekScrollView.AddChild(sleekLabel);
        num += 40f;
        ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
        sleekLabel2.PositionOffset_Y = num;
        sleekLabel2.SizeScale_X = 1f;
        sleekLabel2.SizeOffset_Y = 70f;
        sleekLabel2.Text = localization.format("Warning");
        sleekLabel2.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        sleekScrollView.AddChild(sleekLabel2);
        num += sleekLabel2.SizeOffset_Y + 10f;
        profanityFilterToggle = Glazier.Get().CreateToggle();
        profanityFilterToggle.PositionOffset_X = -240f;
        profanityFilterToggle.PositionOffset_Y = num;
        profanityFilterToggle.PositionScale_X = 0.5f;
        profanityFilterToggle.SizeOffset_X = 40f;
        profanityFilterToggle.SizeOffset_Y = 40f;
        profanityFilterToggle.OnValueChanged += OnProfanityFilterToggled;
        sleekScrollView.AddChild(profanityFilterToggle);
        profanityFilter_Header = Glazier.Get().CreateLabel();
        profanityFilter_Header.PositionOffset_X = -190f;
        profanityFilter_Header.PositionOffset_Y = num - 10f;
        profanityFilter_Header.PositionScale_X = 0.5f;
        profanityFilter_Header.SizeOffset_X = 400f;
        profanityFilter_Header.SizeOffset_Y = 30f;
        profanityFilter_Header.TextAlignment = TextAnchor.LowerLeft;
        profanityFilter_Header.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        sleekScrollView.AddChild(profanityFilter_Header);
        ISleekLabel sleekLabel3 = Glazier.Get().CreateLabel();
        sleekLabel3.PositionOffset_X = -190f;
        sleekLabel3.PositionOffset_Y = num + 20f;
        sleekLabel3.PositionScale_X = 0.5f;
        sleekLabel3.SizeOffset_X = 400f;
        sleekLabel3.SizeOffset_Y = 50f;
        sleekLabel3.TextAlignment = TextAnchor.UpperLeft;
        sleekLabel3.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        sleekLabel3.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        sleekLabel3.Text = localization.format("ProfanityFilter_Description");
        sleekScrollView.AddChild(sleekLabel3);
        num += 60f;
        inboundVoiceChatToggle = Glazier.Get().CreateToggle();
        inboundVoiceChatToggle.PositionOffset_X = -240f;
        inboundVoiceChatToggle.PositionOffset_Y = num;
        inboundVoiceChatToggle.PositionScale_X = 0.5f;
        inboundVoiceChatToggle.SizeOffset_X = 40f;
        inboundVoiceChatToggle.SizeOffset_Y = 40f;
        inboundVoiceChatToggle.OnValueChanged += OnInboundVoiceChatToggled;
        sleekScrollView.AddChild(inboundVoiceChatToggle);
        inboundVoiceChat_Header = Glazier.Get().CreateLabel();
        inboundVoiceChat_Header.PositionOffset_X = -190f;
        inboundVoiceChat_Header.PositionOffset_Y = num - 10f;
        inboundVoiceChat_Header.PositionScale_X = 0.5f;
        inboundVoiceChat_Header.SizeOffset_X = 400f;
        inboundVoiceChat_Header.SizeOffset_Y = 30f;
        inboundVoiceChat_Header.TextAlignment = TextAnchor.LowerLeft;
        inboundVoiceChat_Header.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        sleekScrollView.AddChild(inboundVoiceChat_Header);
        ISleekLabel sleekLabel4 = Glazier.Get().CreateLabel();
        sleekLabel4.PositionOffset_X = -190f;
        sleekLabel4.PositionOffset_Y = num + 20f;
        sleekLabel4.PositionScale_X = 0.5f;
        sleekLabel4.SizeOffset_X = 400f;
        sleekLabel4.SizeOffset_Y = 50f;
        sleekLabel4.TextAlignment = TextAnchor.UpperLeft;
        sleekLabel4.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        sleekLabel4.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        sleekLabel4.Text = localization.format("InboundVoiceChat_Description");
        sleekScrollView.AddChild(sleekLabel4);
        num += 60f;
        outboundVoiceChatToggle = Glazier.Get().CreateToggle();
        outboundVoiceChatToggle.PositionOffset_X = -240f;
        outboundVoiceChatToggle.PositionOffset_Y = num;
        outboundVoiceChatToggle.PositionScale_X = 0.5f;
        outboundVoiceChatToggle.SizeOffset_X = 40f;
        outboundVoiceChatToggle.SizeOffset_Y = 40f;
        outboundVoiceChatToggle.OnValueChanged += OnOutboundVoiceChatToggled;
        sleekScrollView.AddChild(outboundVoiceChatToggle);
        outboundVoiceChat_Header = Glazier.Get().CreateLabel();
        outboundVoiceChat_Header.PositionOffset_X = -190f;
        outboundVoiceChat_Header.PositionOffset_Y = num - 10f;
        outboundVoiceChat_Header.PositionScale_X = 0.5f;
        outboundVoiceChat_Header.SizeOffset_X = 400f;
        outboundVoiceChat_Header.SizeOffset_Y = 30f;
        outboundVoiceChat_Header.TextAlignment = TextAnchor.LowerLeft;
        outboundVoiceChat_Header.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        sleekScrollView.AddChild(outboundVoiceChat_Header);
        outboundVoiceChat_Description = Glazier.Get().CreateLabel();
        outboundVoiceChat_Description.PositionOffset_X = -190f;
        outboundVoiceChat_Description.PositionOffset_Y = num + 20f;
        outboundVoiceChat_Description.PositionScale_X = 0.5f;
        outboundVoiceChat_Description.SizeOffset_X = 400f;
        outboundVoiceChat_Description.SizeOffset_Y = 50f;
        outboundVoiceChat_Description.TextAlignment = TextAnchor.UpperLeft;
        outboundVoiceChat_Description.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        outboundVoiceChat_Description.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        sleekScrollView.AddChild(outboundVoiceChat_Description);
        num += 60f;
        streamerModeToggle = Glazier.Get().CreateToggle();
        streamerModeToggle.PositionOffset_X = -240f;
        streamerModeToggle.PositionOffset_Y = num;
        streamerModeToggle.PositionScale_X = 0.5f;
        streamerModeToggle.SizeOffset_X = 40f;
        streamerModeToggle.SizeOffset_Y = 40f;
        streamerModeToggle.OnValueChanged += OnStreamerModeToggled;
        sleekScrollView.AddChild(streamerModeToggle);
        streamerMode_Header = Glazier.Get().CreateLabel();
        streamerMode_Header.PositionOffset_X = -190f;
        streamerMode_Header.PositionOffset_Y = num - 10f;
        streamerMode_Header.PositionScale_X = 0.5f;
        streamerMode_Header.SizeOffset_X = 400f;
        streamerMode_Header.SizeOffset_Y = 30f;
        streamerMode_Header.TextAlignment = TextAnchor.LowerLeft;
        streamerMode_Header.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        sleekScrollView.AddChild(streamerMode_Header);
        ISleekLabel sleekLabel5 = Glazier.Get().CreateLabel();
        sleekLabel5.PositionOffset_X = -190f;
        sleekLabel5.PositionOffset_Y = num + 20f;
        sleekLabel5.PositionScale_X = 0.5f;
        sleekLabel5.SizeOffset_X = 400f;
        sleekLabel5.SizeOffset_Y = 50f;
        sleekLabel5.TextAlignment = TextAnchor.UpperLeft;
        sleekLabel5.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        sleekLabel5.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        sleekLabel5.Text = localization.format("StreamerMode_Description");
        sleekScrollView.AddChild(sleekLabel5);
        num += 60f;
        ISleekLabel sleekLabel6 = Glazier.Get().CreateLabel();
        sleekLabel6.PositionOffset_Y = num;
        sleekLabel6.SizeScale_X = 1f;
        sleekLabel6.SizeOffset_Y = 30f;
        sleekLabel6.Text = localization.format("OptionsNote");
        sleekLabel6.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        sleekLabel6.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        sleekScrollView.AddChild(sleekLabel6);
        num += sleekLabel6.SizeOffset_Y + 10f;
        SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        sleekButtonIcon.PositionOffset_X = -205f;
        sleekButtonIcon.PositionOffset_Y = num;
        sleekButtonIcon.PositionScale_X = 0.5f;
        sleekButtonIcon.SizeOffset_X = 200f;
        sleekButtonIcon.SizeOffset_Y = 50f;
        sleekButtonIcon.text = MenuDashboardUI.localization.format("BackButtonText");
        sleekButtonIcon.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        sleekButtonIcon.onClickedButton += OnBackClicked;
        sleekButtonIcon.fontSize = ESleekFontSize.Medium;
        sleekButtonIcon.iconColor = ESleekTint.FOREGROUND;
        sleekScrollView.AddChild(sleekButtonIcon);
        SleekButtonIcon sleekButtonIcon2 = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Play"));
        sleekButtonIcon2.PositionOffset_X = 5f;
        sleekButtonIcon2.PositionOffset_Y = num;
        sleekButtonIcon2.PositionScale_X = 0.5f;
        sleekButtonIcon2.SizeOffset_X = 200f;
        sleekButtonIcon2.SizeOffset_Y = 50f;
        sleekButtonIcon2.text = localization.format("ContinueButton_Label");
        sleekButtonIcon2.tooltip = localization.format("ContinueButton_Tooltip");
        sleekButtonIcon2.onClickedButton += OnContinueClicked;
        sleekButtonIcon2.fontSize = ESleekFontSize.Medium;
        sleekButtonIcon2.iconColor = ESleekTint.FOREGROUND;
        sleekScrollView.AddChild(sleekButtonIcon2);
        num += 60f;
        dontShowAgainToggle = Glazier.Get().CreateToggle();
        dontShowAgainToggle.PositionOffset_X = 5f;
        dontShowAgainToggle.PositionOffset_Y = num;
        dontShowAgainToggle.PositionScale_X = 0.5f;
        dontShowAgainToggle.SizeOffset_X = 40f;
        dontShowAgainToggle.SizeOffset_Y = 40f;
        dontShowAgainToggle.AddLabel(localization.format("DontShowAgain_Label"), ESleekSide.RIGHT);
        dontShowAgainToggle.TooltipText = localization.format("DontShowAgain_Tooltip");
        dontShowAgainToggle.OnValueChanged += OnDontShowAgainToggled;
        sleekScrollView.AddChild(dontShowAgainToggle);
        num += 50f;
        sleekScrollView.ContentSizeOffset = new Vector2(0f, num - 10f);
        bundle.unload();
    }
}
