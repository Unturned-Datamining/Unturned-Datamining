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
            volumeSlider.state = stereo.volume;
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
        Assets.find(songs);
        songsBox.RemoveAllChildren();
        songsBox.contentSizeOffset = new Vector2(0f, songs.Count * 30);
        for (int i = 0; i < songs.Count; i++)
        {
            StereoSongAsset stereoSongAsset = songs[i];
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.positionOffset_Y = i * 30;
            sleekButton.sizeOffset_Y = 30;
            sleekButton.sizeScale_X = 1f;
            sleekButton.onClickedButton += onClickedPlayButton;
            sleekButton.textColor = ESleekTint.RICH_TEXT_DEFAULT;
            sleekButton.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            sleekButton.enableRichText = true;
            songsBox.AddChild(sleekButton);
            if (!string.IsNullOrEmpty(stereoSongAsset.titleText))
            {
                sleekButton.text = stereoSongAsset.titleText;
            }
            else
            {
                sleekButton.text = "Sorry, I broke some song names. :( -Nelson";
            }
            if (!string.IsNullOrEmpty(stereoSongAsset.linkURL))
            {
                sleekButton.sizeOffset_X -= 30;
                SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("External_Link"));
                sleekButtonIcon.positionScale_X = 1f;
                sleekButtonIcon.sizeOffset_X = 30;
                sleekButtonIcon.sizeOffset_Y = 30;
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
            volumeSlider.updateLabel(localization.format("Volume_Slider_Label", stereo.compressedVolume));
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
        int num = songsBox.FindIndexOfChild(button.parent);
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
        base.positionScale_Y = 1f;
        base.positionOffset_X = 10;
        base.positionOffset_Y = 10;
        base.sizeOffset_X = -20;
        base.sizeOffset_Y = -20;
        base.sizeScale_X = 1f;
        base.sizeScale_Y = 1f;
        active = false;
        stereo = null;
        stopButton = Glazier.Get().CreateButton();
        stopButton.positionOffset_X = -200;
        stopButton.positionOffset_Y = 5;
        stopButton.positionScale_X = 0.5f;
        stopButton.positionScale_Y = 0.9f;
        stopButton.sizeOffset_X = 195;
        stopButton.sizeOffset_Y = 30;
        stopButton.text = localization.format("Stop_Button");
        stopButton.tooltipText = localization.format("Stop_Button_Tooltip");
        stopButton.onClickedButton += onClickedStopButton;
        AddChild(stopButton);
        closeButton = Glazier.Get().CreateButton();
        closeButton.positionOffset_X = 5;
        closeButton.positionOffset_Y = 5;
        closeButton.positionScale_X = 0.5f;
        closeButton.positionScale_Y = 0.9f;
        closeButton.sizeOffset_X = 195;
        closeButton.sizeOffset_Y = 30;
        closeButton.text = localization.format("Close_Button");
        closeButton.tooltipText = localization.format("Close_Button_Tooltip");
        closeButton.onClickedButton += onClickedCloseButton;
        AddChild(closeButton);
        volumeSlider = Glazier.Get().CreateSlider();
        volumeSlider.positionOffset_X = -200;
        volumeSlider.positionOffset_Y = -25;
        volumeSlider.positionScale_X = 0.5f;
        volumeSlider.positionScale_Y = 0.1f;
        volumeSlider.sizeOffset_X = 250;
        volumeSlider.sizeOffset_Y = 20;
        volumeSlider.orientation = ESleekOrientation.HORIZONTAL;
        volumeSlider.onDragged += onDraggedVolumeSlider;
        volumeSlider.addLabel("", ESleekSide.RIGHT);
        AddChild(volumeSlider);
        songsBox = Glazier.Get().CreateScrollView();
        songsBox.positionOffset_X = -200;
        songsBox.positionScale_X = 0.5f;
        songsBox.positionScale_Y = 0.1f;
        songsBox.sizeOffset_X = 400;
        songsBox.sizeScale_Y = 0.8f;
        songsBox.scaleContentToWidth = true;
        AddChild(songsBox);
    }
}
