using System;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableStereo : Interactable
{
    protected float _volume;

    public AssetReference<StereoSongAsset> track;

    public AudioSource audioSource;

    internal static readonly ClientInstanceMethod<Guid> SendTrack = ClientInstanceMethod<Guid>.Get(typeof(InteractableStereo), "ReceiveTrack");

    private static readonly ServerInstanceMethod<Guid> SendTrackRequest = ServerInstanceMethod<Guid>.Get(typeof(InteractableStereo), "ReceiveTrackRequest");

    private static readonly ClientInstanceMethod<byte> SendChangeVolume = ClientInstanceMethod<byte>.Get(typeof(InteractableStereo), "ReceiveChangeVolume");

    private static readonly ServerInstanceMethod<byte> SendChangeVolumeRequest = ServerInstanceMethod<byte>.Get(typeof(InteractableStereo), "ReceiveChangeVolumeRequest");

    public float volume
    {
        get
        {
            return _volume;
        }
        set
        {
            _volume = Mathf.Clamp01(value);
            if (audioSource != null)
            {
                audioSource.volume = _volume;
            }
        }
    }

    public byte compressedVolume
    {
        get
        {
            return (byte)Mathf.RoundToInt(volume * 100f);
        }
        set
        {
            volume = Mathf.Clamp01((float)(int)value / 100f);
        }
    }

    public void updateTrack(Guid newTrack)
    {
        track.GUID = newTrack;
        if (!(audioSource != null))
        {
            return;
        }
        audioSource.clip = null;
        audioSource.loop = false;
        StereoSongAsset stereoSongAsset = Assets.find(track);
        if (stereoSongAsset != null)
        {
            if (stereoSongAsset.songMbRef.isValid)
            {
                audioSource.clip = stereoSongAsset.songMbRef.loadAsset();
            }
            else if (stereoSongAsset.songContentRef.isValid)
            {
                audioSource.clip = Assets.load(stereoSongAsset.songContentRef);
            }
            audioSource.loop = stereoSongAsset.isLoop;
        }
        if (audioSource.clip != null)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
    }

    public override void updateState(Asset asset, byte[] state)
    {
        GuidBuffer guidBuffer = default(GuidBuffer);
        guidBuffer.Read(state, 0);
        updateTrack(guidBuffer.GUID);
        compressedVolume = state[16];
    }

    public override void use()
    {
        PlayerUI.instance.boomboxUI.open(this);
        PlayerLifeUI.close();
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        message = EPlayerMessage.USE;
        text = "";
        color = Color.white;
        return !PlayerUI.window.showCursor;
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveTrack(Guid newTrack)
    {
        updateTrack(newTrack);
    }

    public void ClientSetTrack(Guid newTrack)
    {
        SendTrackRequest.Invoke(GetNetId(), ENetReliability.Unreliable, newTrack);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
    public void ReceiveTrackRequest(in ServerInvocationContext context, Guid newTrack)
    {
        Player player = context.GetPlayer();
        if (!(player == null) && !player.life.isDead && !((base.transform.position - player.transform.position).sqrMagnitude > 400f) && BarricadeManager.tryGetRegion(base.transform, out var x, out var y, out var plant, out var region))
        {
            BarricadeManager.ServerSetStereoTrackInternal(this, x, y, plant, region, newTrack);
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveChangeVolume(byte newVolume)
    {
        compressedVolume = newVolume;
    }

    public void ClientSetVolume(byte newVolume)
    {
        SendChangeVolumeRequest.Invoke(GetNetId(), ENetReliability.Unreliable, newVolume);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 8)]
    public void ReceiveChangeVolumeRequest(in ServerInvocationContext context, byte newVolume)
    {
        Player player = context.GetPlayer();
        if (!(player == null) && !player.life.isDead && !((base.transform.position - player.transform.position).sqrMagnitude > 400f) && BarricadeManager.tryGetRegion(base.transform, out var x, out var y, out var plant, out var region))
        {
            newVolume = MathfEx.Min(newVolume, 100);
            SendChangeVolume.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, BarricadeManager.EnumerateClients_Remote(x, y, plant), newVolume);
            region.FindBarricadeByRootFast(base.transform).serversideData.barricade.state[16] = newVolume;
        }
    }
}
