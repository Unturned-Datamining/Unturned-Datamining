using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerBarricadeStereoUI : SleekFullscreenBox
{
    private List<StereoSongAsset> songs = new List<StereoSongAsset>();

    private Local localization;

    public bool active;

    private InteractableStereo stereo;

    private double lastUpdateVolumeRealtime;

    private bool hasPendingVolumeUpdate;

    private ISleekButton stopButton;

    private ISleekButton closeButton;

    private ISleekSlider volumeSlider;

    private ISleekScrollView songsBox;

    public void open(InteractableStereo newStereo)
    {
        if (active)
        {
            close();
            return;
        }
        active = true;
        stereo = newStereo;
        hasPendingVolumeUpdate = false;
        refreshSongs();
        if (stereo != null)
        {
            volumeSlider.Value = stereo.volume;
        }
        updateVolumeSliderLabel();
        AnimateIntoView();
    }

    public void close()
    {
        if (active)
        {
            if (stereo != null && hasPendingVolumeUpdate)
            {
                hasPendingVolumeUpdate = false;
                stereo.ClientSetVolume(stereo.compressedVolume);
            }
            active = false;
            stereo = null;
            AnimateOutOfView(0f, 1f);
        }
    }

    private void refreshSongs()
    {
        songs.Clear();
        Assets.FindAssetsByType_UseDefaultAssetMapping(songs);
        songsBox.RemoveAllChildren();
        songsBox.ContentSizeOffset = new Vector2(0f, songs.Count * 30);
        for (int i = 0; i < songs.Count; i++)
        {
            StereoSongAsset stereoSongAsset = songs[i];
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.PositionOffset_Y = i * 30;
            sleekButton.SizeOffset_Y = 30f;
            sleekButton.SizeScale_X = 1f;
            sleekButton.OnClicked += onClickedPlayButton;
            sleekButton.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
            sleekButton.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            sleekButton.AllowRichText = true;
            songsBox.AddChild(sleekButton);
            if (!string.IsNullOrEmpty(stereoSongAsset.titleText))
            {
                sleekButton.Text = stereoSongAsset.titleText;
            }
            else
            {
                sleekButton.Text = "Sorry, I broke some song names. :( -Nelson";
            }
            if (!string.IsNullOrEmpty(stereoSongAsset.linkURL))
            {
                sleekButton.SizeOffset_X -= 30f;
                SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("External_Link"));
                sleekButtonIcon.PositionScale_X = 1f;
                sleekButtonIcon.SizeOffset_X = 30f;
                sleekButtonIcon.SizeOffset_Y = 30f;
                sleekButtonIcon.tooltip = stereoSongAsset.linkURL;
                sleekButtonIcon.onClickedButton += onClickedLinkButton;
                sleekButton.AddChild(sleekButtonIcon);
            }
        }
    }

    private void updateVolumeSliderLabel()
    {
        if (stereo != null)
        {
            volumeSlider.UpdateLabel(localization.format("Volume_Slider_Label", stereo.compressedVolume));
        }
    }

    private void onDraggedVolumeSlider(ISleekSlider slider, float state)
    {
        if (stereo != null)
        {
            stereo.volume = state;
            hasPendingVolumeUpdate = true;
            updateVolumeSliderLabel();
        }
    }

    private void onClickedPlayButton(ISleekElement button)
    {
        int num = songsBox.FindIndexOfChild(button);
        if (num < songs.Count)
        {
            StereoSongAsset stereoSongAsset = songs[num];
            if (stereo != null)
            {
                stereo.ClientSetTrack(stereoSongAsset.GUID);
            }
        }
    }

    private void onClickedLinkButton(ISleekElement button)
    {
        int num = songsBox.FindIndexOfChild(button.Parent);
        if (num < songs.Count)
        {
            StereoSongAsset stereoSongAsset = songs[num];
            if (WebUtils.ParseThirdPartyUrl(stereoSongAsset.linkURL, out var _))
            {
                Provider.openURL(stereoSongAsset.linkURL);
                return;
            }
            UnturnedLog.warn("Ignoring potentially unsafe song link url {0}", stereoSongAsset.linkURL);
        }
    }

    private void onClickedStopButton(ISleekElement button)
    {
        if (stereo != null)
        {
            stereo.ClientSetTrack(Guid.Empty);
        }
    }

    private void onClickedCloseButton(ISleekElement button)
    {
        PlayerLifeUI.open();
        close();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (stereo != null && hasPendingVolumeUpdate)
        {
            double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
            if (realtimeSinceStartupAsDouble - lastUpdateVolumeRealtime > 0.20000000298023224)
            {
                lastUpdateVolumeRealtime = realtimeSinceStartupAsDouble;
                stereo.ClientSetVolume(stereo.compressedVolume);
                hasPendingVolumeUpdate = false;
            }
        }
    }

    public PlayerBarricadeStereoUI()
    {
        localization = Localization.read("/Player/PlayerBarricadeStereo.dat");
        base.PositionScale_Y = 1f;
        base.PositionOffset_X = 10f;
        base.PositionOffset_Y = 10f;
        base.SizeOffset_X = -20f;
        base.SizeOffset_Y = -20f;
        base.SizeScale_X = 1f;
        base.SizeScale_Y = 1f;
        active = false;
        stereo = null;
        stopButton = Glazier.Get().CreateButton();
        stopButton.PositionOffset_X = -200f;
        stopButton.PositionOffset_Y = 5f;
        stopButton.PositionScale_X = 0.5f;
        stopButton.PositionScale_Y = 0.9f;
        stopButton.SizeOffset_X = 195f;
        stopButton.SizeOffset_Y = 30f;
        stopButton.Text = localization.format("Stop_Button");
        stopButton.TooltipText = localization.format("Stop_Button_Tooltip");
        stopButton.OnClicked += onClickedStopButton;
        AddChild(stopButton);
        closeButton = Glazier.Get().CreateButton();
        closeButton.PositionOffset_X = 5f;
        closeButton.PositionOffset_Y = 5f;
        closeButton.PositionScale_X = 0.5f;
        closeButton.PositionScale_Y = 0.9f;
        closeButton.SizeOffset_X = 195f;
        closeButton.SizeOffset_Y = 30f;
        closeButton.Text = localization.format("Close_Button");
        closeButton.TooltipText = localization.format("Close_Button_Tooltip");
        closeButton.OnClicked += onClickedCloseButton;
        AddChild(closeButton);
        volumeSlider = Glazier.Get().CreateSlider();
        volumeSlider.PositionOffset_X = -200f;
        volumeSlider.PositionOffset_Y = -25f;
        volumeSlider.PositionScale_X = 0.5f;
        volumeSlider.PositionScale_Y = 0.1f;
        volumeSlider.SizeOffset_X = 250f;
        volumeSlider.SizeOffset_Y = 20f;
        volumeSlider.Orientation = ESleekOrientation.HORIZONTAL;
        volumeSlider.OnValueChanged += onDraggedVolumeSlider;
        volumeSlider.AddLabel("", ESleekSide.RIGHT);
        AddChild(volumeSlider);
        songsBox = Glazier.Get().CreateScrollView();
        songsBox.PositionOffset_X = -200f;
        songsBox.PositionScale_X = 0.5f;
        songsBox.PositionScale_Y = 0.1f;
        songsBox.SizeOffset_X = 400f;
        songsBox.SizeScale_Y = 0.8f;
        songsBox.ScaleContentToWidth = true;
        AddChild(songsBox);
    }
}
