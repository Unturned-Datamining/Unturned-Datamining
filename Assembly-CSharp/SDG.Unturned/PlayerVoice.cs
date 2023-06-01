using System;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerVoice : PlayerCaller
{
    public delegate void RelayVoiceHandler(PlayerVoice speaker, bool wantsToUseWalkieTalkie, ref bool shouldAllow, ref bool shouldBroadcastOverRadio, ref RelayVoiceCullingHandler cullingHandler);

    public delegate bool RelayVoiceCullingHandler(PlayerVoice speaker, PlayerVoice listener);

    private static readonly byte[] COMPRESSED_VOICE_BUFFER = new byte[8000];

    private static readonly byte[] DECOMPRESSED_VOICE_BUFFER = new byte[22000];

    private static readonly float RECORDING_POLL_INTERVAL = 0.05f;

    private static readonly float PLAYBACK_DELAY = 0.2f;

    private static readonly float SILENCE_DURATION = 0.1f;

    private static readonly uint EXPECTED_ASKVOICE_PER_SECOND = (uint)(1f / RECORDING_POLL_INTERVAL) + 3;

    private static readonly uint EXPECTED_BYTES_PER_SECOND = 7000u;

    [Obsolete("Replaced by ServerSetPermissions which is replicated to owner.")]
    public bool allowVoiceChat = true;

    private bool _isTalking;

    public bool hasUseableWalkieTalkie;

    private bool playbackUsingWalkieTalkie;

    private bool hasPendingVoiceData;

    private bool isPlayingVoiceData;

    private float pendingPlaybackDelay;

    private float availablePlaybackTime;

    private bool wasRecording;

    private float pollRecordingTimer;

    private float lastAskVoiceRealtime;

    private uint recentVoiceCalls;

    private uint recentVoiceBytes;

    private float recentVoiceDuration;

    private static readonly ClientInstanceMethod<bool, bool> SendPermissions = ClientInstanceMethod<bool, bool>.Get(typeof(PlayerVoice), "ReceivePermissions");

    private static readonly ServerInstanceMethod SendVoiceChatRelay = ServerInstanceMethod.Get(typeof(PlayerVoice), "ReceiveVoiceChatRelay");

    private static ClientInstanceMethod SendPlayVoiceChat = ClientInstanceMethod.Get(typeof(PlayerVoice), "ReceivePlayVoiceChat");

    private bool _inputWantsToRecord;

    private static StaticResourceRef<AudioClip> radioClip = new StaticResourceRef<AudioClip>("Sounds/General/Radio");

    private AudioSource audioSource;

    private AudioClip audioClip;

    private float[] audioData;

    private int audioDataWriteIndex;

    private uint steamOptimalSampleRate;

    private float secondsPerSample;

    private int zeroSamples;

    private bool allowTalkingWhileDead;

    private bool customAllowTalking = true;

    public bool isTalking
    {
        get
        {
            return _isTalking;
        }
        private set
        {
            if (_isTalking != value)
            {
                _isTalking = value;
                this.onTalkingChanged?.Invoke(value);
            }
        }
    }

    public bool canHearRadio
    {
        get
        {
            if (!hasUseableWalkieTalkie)
            {
                return hasEarpiece;
            }
            return true;
        }
    }

    public bool hasEarpiece
    {
        get
        {
            if (base.player.clothing != null && base.player.clothing.maskAsset != null)
            {
                return base.player.clothing.maskAsset.isEarpiece;
            }
            return false;
        }
    }

    private bool inputWantsToRecord
    {
        get
        {
            return _inputWantsToRecord;
        }
        set
        {
            if (_inputWantsToRecord == value)
            {
                return;
            }
            _inputWantsToRecord = value;
            if (inputWantsToRecord && !wasRecording)
            {
                wasRecording = true;
                pollRecordingTimer = 0f;
            }
            if (inputWantsToRecord)
            {
                SteamUser.StartVoiceRecording();
            }
            else
            {
                SteamUser.StopVoiceRecording();
            }
            SteamFriends.SetInGameVoiceSpeaking(Provider.user, inputWantsToRecord);
            if (!canEverPlayback)
            {
                if (hasUseableWalkieTalkie)
                {
                    playWalkieTalkieSound();
                }
                isTalking = inputWantsToRecord;
            }
        }
    }

    private bool canEverRecord
    {
        get
        {
            if (base.channel.isOwner)
            {
                return !Provider.isServer;
            }
            return false;
        }
    }

    private bool canEverPlayback => !base.channel.isOwner;

    public event Talked onTalkingChanged;

    public static event RelayVoiceHandler onRelayVoice;

    public static bool handleRelayVoiceCulling_RadioFrequency(PlayerVoice speaker, PlayerVoice listener)
    {
        if (listener.canHearRadio && speaker.player.quests.radioFrequency == listener.player.quests.radioFrequency)
        {
            return true;
        }
        return false;
    }

    public static bool handleRelayVoiceCulling_Proximity(PlayerVoice speaker, PlayerVoice listener)
    {
        if ((speaker.transform.position - listener.transform.position).sqrMagnitude < 16384f)
        {
            return true;
        }
        return false;
    }

    public bool GetAllowTalkingWhileDead()
    {
        return allowTalkingWhileDead;
    }

    public bool GetCustomAllowTalking()
    {
        return customAllowTalking;
    }

    public void ServerSetPermissions(bool allowTalkingWhileDead, bool customAllowTalking)
    {
        if (this.allowTalkingWhileDead != allowTalkingWhileDead || this.customAllowTalking != customAllowTalking)
        {
            this.allowTalkingWhileDead = allowTalkingWhileDead;
            this.customAllowTalking = customAllowTalking;
            SendPermissions.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), allowTalkingWhileDead, customAllowTalking);
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceivePermissions(bool allowTalkingWhileDead, bool customAllowTalking)
    {
        this.allowTalkingWhileDead = allowTalkingWhileDead;
        this.customAllowTalking = customAllowTalking;
    }

    [Obsolete]
    public void askVoiceChat(byte[] packet)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER)]
    public void ReceiveVoiceChatRelay(in ServerInvocationContext context)
    {
        if ((base.player.life.isDead && !allowTalkingWhileDead) || !customAllowTalking || !allowVoiceChat)
        {
            return;
        }
        NetPakReader reader = context.reader;
        reader.ReadUInt16(out var compressedSize);
        reader.ReadBit(out var value);
        if (!reader.ReadBytesPtr(compressedSize, out var source, out var sourceOffset) || compressedSize < 1)
        {
            return;
        }
        float num = Time.realtimeSinceStartup - lastAskVoiceRealtime;
        if (num > 2f)
        {
            recentVoiceCalls = 1u;
            recentVoiceBytes = compressedSize;
            recentVoiceDuration = RECORDING_POLL_INTERVAL;
        }
        else
        {
            recentVoiceCalls++;
            recentVoiceBytes += compressedSize;
            recentVoiceDuration += num;
        }
        lastAskVoiceRealtime = Time.realtimeSinceStartup;
        if (recentVoiceCalls >= EXPECTED_ASKVOICE_PER_SECOND || recentVoiceBytes > EXPECTED_BYTES_PER_SECOND || recentVoiceDuration > 1f)
        {
            float num2 = (float)recentVoiceCalls / recentVoiceDuration;
            float num3 = (float)recentVoiceBytes / recentVoiceDuration;
            if (num2 >= (float)EXPECTED_ASKVOICE_PER_SECOND || num3 > (float)EXPECTED_BYTES_PER_SECOND)
            {
                return;
            }
        }
        bool shouldAllow;
        bool shouldBroadcastOverRadio;
        if (value)
        {
            if (hasUseableWalkieTalkie)
            {
                shouldAllow = true;
                shouldBroadcastOverRadio = true;
            }
            else
            {
                shouldAllow = false;
                shouldBroadcastOverRadio = false;
            }
        }
        else
        {
            shouldAllow = true;
            shouldBroadcastOverRadio = false;
        }
        RelayVoiceCullingHandler cullingHandler = null;
        PlayerVoice.onRelayVoice?.Invoke(this, value, ref shouldAllow, ref shouldBroadcastOverRadio, ref cullingHandler);
        if (!shouldAllow)
        {
            return;
        }
        if (cullingHandler == null)
        {
            cullingHandler = (shouldBroadcastOverRadio ? new RelayVoiceCullingHandler(handleRelayVoiceCulling_RadioFrequency) : new RelayVoiceCullingHandler(handleRelayVoiceCulling_Proximity));
        }
        SendPlayVoiceChat.Invoke(GetNetId(), ENetReliability.Unreliable, Provider.GatherRemoteClientConnectionsMatchingPredicate(delegate(SteamPlayer potentialRecipient)
        {
            if (potentialRecipient == null || potentialRecipient.player == null || potentialRecipient.player.voice == null)
            {
                return false;
            }
            return potentialRecipient != base.channel.owner && cullingHandler(this, potentialRecipient.player.voice);
        }), delegate(NetPakWriter writer)
        {
            writer.WriteUInt16(compressedSize);
            writer.WriteBit(shouldBroadcastOverRadio);
            writer.WriteBytes(source, sourceOffset, compressedSize);
        });
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceivePlayVoiceChat(in ClientInvocationContext context)
    {
        if (OptionsSettings.chatVoiceIn && !base.channel.owner.isMuted && audioData != null && !(audioSource == null) && !(audioSource.clip == null))
        {
            NetPakReader reader = context.reader;
            reader.ReadUInt16(out var value);
            reader.ReadBit(out var value2);
            if (reader.ReadBytes(COMPRESSED_VOICE_BUFFER, value))
            {
                AppendVoiceData(COMPRESSED_VOICE_BUFFER, value, value2);
            }
        }
    }

    private void AppendVoiceData(byte[] compressedBuffer, uint compressedSize, bool wantsToUseWalkieTalkie)
    {
        playbackUsingWalkieTalkie = wantsToUseWalkieTalkie;
        if (SteamUser.DecompressVoice(compressedBuffer, compressedSize, DECOMPRESSED_VOICE_BUFFER, (uint)DECOMPRESSED_VOICE_BUFFER.Length, out var nBytesWritten, steamOptimalSampleRate) != 0 || nBytesWritten < 1)
        {
            return;
        }
        int num = audioDataWriteIndex;
        float num2 = 0f;
        for (uint num3 = 0u; num3 < nBytesWritten; num3 += 2)
        {
            byte num4 = DECOMPRESSED_VOICE_BUFFER[num3];
            byte b = DECOMPRESSED_VOICE_BUFFER[num3 + 1];
            float num5 = (float)(short)(num4 | (b << 8)) / 32767f;
            num2 = Mathf.Max(Mathf.Abs(num5), num2);
            audioData[audioDataWriteIndex] = num5;
            audioDataWriteIndex = (audioDataWriteIndex + 1) % audioData.Length;
        }
        if (num2 != 0f)
        {
            float num6 = Mathf.Min(1f / num2, 8f);
            num6 *= OptionsSettings.voiceVolume;
            for (uint num7 = 0u; num7 < nBytesWritten; num7 += 2)
            {
                audioData[num] *= num6;
                num = (num + 1) % audioData.Length;
            }
        }
        int num8 = audioDataWriteIndex;
        for (int i = 0; i < zeroSamples; i++)
        {
            audioData[num8] = 0f;
            num8 = (num8 + 1) % audioData.Length;
        }
        audioClip.SetData(audioData, 0);
        float num9 = (float)(nBytesWritten / 2u) * secondsPerSample;
        if (!isPlayingVoiceData && !hasPendingVoiceData)
        {
            hasPendingVoiceData = true;
            pendingPlaybackDelay = PLAYBACK_DELAY;
            availablePlaybackTime = SILENCE_DURATION;
        }
        availablePlaybackTime += num9;
    }

    private void updateInput()
    {
        bool chatVoiceOut = OptionsSettings.chatVoiceOut;
        bool key = InputEx.GetKey(ControlsSettings.voice);
        bool flag = base.player.life.IsAlive || allowTalkingWhileDead;
        inputWantsToRecord = chatVoiceOut && key && flag && customAllowTalking;
    }

    private void updateRecording()
    {
        if (!wasRecording)
        {
            return;
        }
        pollRecordingTimer += Time.unscaledDeltaTime;
        if (pollRecordingTimer < RECORDING_POLL_INTERVAL)
        {
            return;
        }
        wasRecording = inputWantsToRecord;
        pollRecordingTimer = 0f;
        uint pcbCompressed;
        EVoiceResult availableVoice = SteamUser.GetAvailableVoice(out pcbCompressed);
        if (availableVoice != 0 && availableVoice != EVoiceResult.k_EVoiceResultNoData)
        {
            UnturnedLog.error("GetAvailableVoice result: " + availableVoice);
        }
        if (availableVoice != 0 || pcbCompressed < 1)
        {
            return;
        }
        uint compressedSize;
        EVoiceResult voice = SteamUser.GetVoice(bWantCompressed: true, COMPRESSED_VOICE_BUFFER, pcbCompressed, out compressedSize);
        if (voice != 0 && voice != EVoiceResult.k_EVoiceResultNoData)
        {
            UnturnedLog.error("GetVoice result: " + voice);
        }
        if (voice != 0 || compressedSize < 1)
        {
            return;
        }
        if (Provider.isServer)
        {
            AppendVoiceData(COMPRESSED_VOICE_BUFFER, compressedSize, hasUseableWalkieTalkie);
            return;
        }
        SendVoiceChatRelay.Invoke(GetNetId(), ENetReliability.Unreliable, delegate(NetPakWriter writer)
        {
            ushort num = (ushort)compressedSize;
            writer.WriteUInt16(num);
            writer.WriteBit(hasUseableWalkieTalkie);
            writer.WriteBytes(COMPRESSED_VOICE_BUFFER, num);
        });
    }

    private void playWalkieTalkieSound()
    {
        OneShotAudioParameters oneShotAudioParameters = new OneShotAudioParameters(base.transform.position, radioClip);
        oneShotAudioParameters.RandomizeVolume(0.74f, 0.76f);
        oneShotAudioParameters.RandomizePitch(0.99f, 1.01f);
        oneShotAudioParameters.SetSpatialBlend2D();
        oneShotAudioParameters.Play();
    }

    private void updatePlayback()
    {
        if (audioSource == null)
        {
            return;
        }
        if (playbackUsingWalkieTalkie)
        {
            audioSource.spatialBlend = 0f;
        }
        else
        {
            audioSource.spatialBlend = 1f;
        }
        if (isPlayingVoiceData)
        {
            availablePlaybackTime -= Time.deltaTime;
            if (availablePlaybackTime <= 0f)
            {
                audioSource.Stop();
                audioSource.time = 0f;
                audioDataWriteIndex = 0;
                isPlayingVoiceData = false;
                hasPendingVoiceData = false;
                if (playbackUsingWalkieTalkie)
                {
                    playWalkieTalkieSound();
                }
                isTalking = false;
            }
        }
        else
        {
            if (!hasPendingVoiceData)
            {
                return;
            }
            pendingPlaybackDelay -= Time.deltaTime;
            if (pendingPlaybackDelay <= 0f)
            {
                isPlayingVoiceData = true;
                if (playbackUsingWalkieTalkie)
                {
                    playWalkieTalkieSound();
                }
                audioSource.Play();
                isTalking = true;
            }
        }
    }

    private void Update()
    {
        if (!Dedicator.IsDedicatedServer)
        {
            if (canEverRecord)
            {
                updateInput();
                updateRecording();
            }
            if (canEverPlayback)
            {
                updatePlayback();
            }
        }
    }

    internal void InitializePlayer()
    {
        if (!Dedicator.IsDedicatedServer)
        {
            audioSource = GetComponent<AudioSource>();
            if (canEverPlayback)
            {
                steamOptimalSampleRate = SteamUser.GetVoiceOptimalSampleRate();
                int num = (int)steamOptimalSampleRate;
                secondsPerSample = 1f / (float)num;
                int num2 = num * 2;
                audioData = new float[num2];
                zeroSamples = Mathf.CeilToInt(SILENCE_DURATION * 1.5f * (float)num);
                audioClip = AudioClip.Create("Voice", num2, 1, num, stream: false);
                audioSource.clip = audioClip;
            }
        }
    }

    private void OnDestroy()
    {
        if (canEverRecord)
        {
            inputWantsToRecord = false;
        }
    }
}
