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

    /// <summary>
    /// Speaker writes compressed audio to this buffer.
    /// Listener copies network buffer here for decompression.
    /// </summary>
    private static byte[] compressedVoiceBuffer = new byte[8000];

    /// <summary>
    /// Listener writes decompressed PCM data to this buffer.
    /// </summary>
    private static readonly byte[] DECOMPRESSED_VOICE_BUFFER = new byte[22000];

    /// <summary>
    /// Seconds interval to wait between asking recording subsystem for voice data.
    /// Rather than polling every frame we wait until data has accumulated to send.
    /// </summary>
    private static readonly float RECORDING_POLL_INTERVAL = 0.05f;

    /// <summary>
    /// Seconds to wait before playing back newly received data.
    /// Allows a few samples to buffer up so that we don't stutter as more arrive.
    /// </summary>
    private static readonly float PLAYBACK_DELAY = 0.2f;

    /// <summary>
    /// Seconds to wait after playback before stopping audio source.
    /// We zero this portion of the clip to prevent pops.
    /// </summary>
    private static readonly float SILENCE_DURATION = 0.1f;

    /// <summary>
    /// Max calls to askVoice server will allow per second before blocking their voice data.
    /// Prevents spamming many tiny requests bogging down server output.
    /// </summary>
    private static readonly uint EXPECTED_ASKVOICE_PER_SECOND = (uint)(1f / RECORDING_POLL_INTERVAL) + 3;

    /// <summary>
    /// Max compressed bytes server will allow per second before blocking their voice data.
    /// When logging compressed size they averaged 3000-5000 per second, so this affords some wiggle-room.
    /// </summary>
    private static readonly uint EXPECTED_BYTES_PER_SECOND = 7000u;

    [Obsolete("Replaced by ServerSetPermissions which is replicated to owner.")]
    public bool allowVoiceChat = true;

    /// <summary>
    /// Internal value managed by isTalking.
    /// </summary>
    private bool _isTalking;

    /// <summary>
    /// Is a UseableWalkieTalkie currently equipped?
    /// Set by useable's equip and dequip events.
    /// </summary>
    public bool hasUseableWalkieTalkie;

    /// <summary>
    /// Was the most recent voice data we received sent using walkie talkie?
    /// </summary>
    private bool playbackUsingWalkieTalkie;

    /// <summary>
    /// Has voice data recently been received, but we're waiting slightly to begin playback?
    /// Important to give clip a chance to buffer up so that we don't stutter as more samples arrive.
    /// </summary>
    private bool hasPendingVoiceData;

    /// <summary>
    /// AudioSource.isPlaying is not trustworthy.
    /// </summary>
    private bool isPlayingVoiceData;

    /// <summary>
    /// Timer counting down to begin playback of recently received voice data.
    /// We use a timer rather than availableSamples.Count because a very short phrase could be less than threshold.
    /// </summary>
    private float pendingPlaybackDelay;

    /// <summary>
    /// Timer counting down to end playback.
    /// </summary>
    private float availablePlaybackTime;

    /// <summary>
    /// Accumulated realtime since we last polled data from voice subsystem.
    /// </summary>
    private float pollRecordingTimer;

    /// <summary>
    /// Last time askVoiceChat was invoked over network.
    /// </summary>
    private float lastAskVoiceRealtime;

    /// <summary>
    /// Number of times askVoiceChat has been called recently, to prevent calling it many times
    /// with tiny durations getting server to relay many packets to clients.
    /// </summary>
    private uint recentVoiceCalls;

    /// <summary>
    /// Total of recent compressed voice payload lengths.
    /// </summary>
    private uint recentVoiceBytes;

    /// <summary>
    /// Realtime since this recent conversation began.
    /// </summary>
    private float recentVoiceDuration;

    private static readonly ClientInstanceMethod<bool, bool> SendPermissions = ClientInstanceMethod<bool, bool>.Get(typeof(PlayerVoice), "ReceivePermissions");

    private static readonly ServerInstanceMethod SendVoiceChatRelay = ServerInstanceMethod.Get(typeof(PlayerVoice), "ReceiveVoiceChatRelay");

    private static ClientInstanceMethod SendPlayVoiceChat = ClientInstanceMethod.Get(typeof(PlayerVoice), "ReceivePlayVoiceChat");

    /// <summary>
    /// Set to true during OnDestroy to make sure we don't start recording again.
    /// </summary>
    private bool isBeingDestroyed;

    private bool _isSteamRecording;

    /// <summary>
    /// Internal value managed by inputWantsToRecord.
    /// </summary>
    private bool _inputWantsToRecord;

    private static StaticResourceRef<AudioClip> radioClip = new StaticResourceRef<AudioClip>("Sounds/General/Radio");

    /// <summary>
    /// Player's voice audio source cached during Start.
    /// </summary>
    private AudioSource audioSource;

    /// <summary>
    /// Looping voice audio clip.
    /// </summary>
    private AudioClip audioClip;

    /// <summary>
    /// Playback buffer.
    /// </summary>
    private float[] audioData;

    private int audioDataWriteIndex;

    /// <summary>
    /// Steam does less work on the main thread if we request samples at the native decompresser sample rate,
    /// so the re-sampling can be done on the Unity audio thread instead.
    /// </summary>
    private uint steamOptimalSampleRate;

    /// <summary>
    /// 1 / frequency
    /// </summary>
    private float secondsPerSample;

    /// <summary>
    /// Number of samples to zero after writing new audio data.
    /// </summary>
    private int zeroSamples;

    private bool allowTalkingWhileDead;

    private bool customAllowTalking = true;

    /// <summary>
    /// Is this player broadcasting their voice?
    /// Used in the menus to show an indicator who's talking.
    /// Locally set when recording starts/stops, and remotely when voice data starts/stops being received.
    /// </summary>
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

    /// <summary>
    /// Can this player currently hear global (radio) voice chat?
    /// </summary>
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

    /// <summary>
    /// Is the player wearing an earpiece?
    /// Allows global (radio) voice chat to be heard without equipping the walkie-talkie item.
    /// </summary>
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

    /// <summary>
    /// If true, SteamUser.StartVoiceRecording has been called without a corresponding call to
    /// SteamUser.StopVoiceRecording yet.
    /// </summary>
    private bool SteamIsRecording
    {
        get
        {
            return _isSteamRecording;
        }
        set
        {
            if (_isSteamRecording != value)
            {
                _isSteamRecording = value;
                if (_isSteamRecording)
                {
                    SteamUser.StartVoiceRecording();
                }
                else
                {
                    SteamUser.StopVoiceRecording();
                }
            }
        }
    }

    /// <summary>
    /// Set by updateInput based on whether voice is enabled, key is held, is alive, etc.
    /// Reset to false during OnDestroy to stop recording.
    /// </summary>
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
            if (_inputWantsToRecord)
            {
                pollRecordingTimer = 0f;
            }
            SynchronizeSteamIsRecording();
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

    /// <summary>
    /// Will this component ever need to record voice data?
    /// </summary>
    private bool canEverRecord
    {
        get
        {
            if (base.channel.IsLocalPlayer)
            {
                return !Provider.isServer;
            }
            return false;
        }
    }

    /// <summary>
    /// Will this component ever need to play voice data?
    /// In release builds this is only true for remote clients, but in debug we may want to locally listen.
    /// </summary>
    private bool canEverPlayback => !base.channel.IsLocalPlayer;

    /// <summary>
    /// Broadcasts after isTalking changes.
    /// </summary>
    public event Talked onTalkingChanged;

    /// <summary>
    /// Only used by plugins.
    /// Called on server to allow plugins to override the default area and walkie-talkie voice channels.
    /// </summary>
    public static event RelayVoiceHandler onRelayVoice;

    /// <summary>
    /// Default culling handler when speaking over walkie-talkie.
    /// </summary>
    public static bool handleRelayVoiceCulling_RadioFrequency(PlayerVoice speaker, PlayerVoice listener)
    {
        if (listener.canHearRadio && speaker.player.quests.radioFrequency == listener.player.quests.radioFrequency)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Default culling handler when speaking in proximity.
    /// </summary>
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

    /// <summary>
    /// Called by owner to relay voice data to clients.
    /// Not using rate limit attribute because it internally tracks bytes per second.
    /// </summary>
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

    /// <summary>
    /// Called by server to relay voice data from clients.
    /// </summary>
    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceivePlayVoiceChat(in ClientInvocationContext context)
    {
        if (OptionsSettings.chatVoiceIn && !base.channel.owner.isVoiceChatLocallyMuted && audioData != null && !(audioSource == null) && !(audioSource.clip == null))
        {
            NetPakReader reader = context.reader;
            reader.ReadUInt16(out var value);
            reader.ReadBit(out var value2);
            if (reader.ReadBytes(compressedVoiceBuffer, value))
            {
                AppendVoiceData(compressedVoiceBuffer, value, value2);
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
        float num9 = (float)(nBytesWritten / 2) * secondsPerSample;
        if (!isPlayingVoiceData && !hasPendingVoiceData)
        {
            hasPendingVoiceData = true;
            pendingPlaybackDelay = PLAYBACK_DELAY;
            availablePlaybackTime = SILENCE_DURATION;
        }
        availablePlaybackTime += num9;
    }

    /// <summary>
    /// Called during Update on owner client to start/stop recording.
    /// </summary>
    private void updateInput()
    {
        bool chatVoiceOut = OptionsSettings.chatVoiceOut;
        bool key = InputEx.GetKey(ControlsSettings.voice);
        bool flag = base.player.life.IsAlive || allowTalkingWhileDead;
        inputWantsToRecord = chatVoiceOut && key && flag && customAllowTalking;
    }

    /// <summary>
    /// Called during Update on owner client to record voice data.
    /// </summary>
    private void updateRecording()
    {
        pollRecordingTimer += Time.unscaledDeltaTime;
        if (pollRecordingTimer < RECORDING_POLL_INTERVAL)
        {
            return;
        }
        pollRecordingTimer = 0f;
        uint pcbCompressed;
        EVoiceResult availableVoice = SteamUser.GetAvailableVoice(out pcbCompressed);
        if (availableVoice != 0 && availableVoice != EVoiceResult.k_EVoiceResultNoData && availableVoice != EVoiceResult.k_EVoiceResultNotRecording)
        {
            UnturnedLog.error("GetAvailableVoice result: " + availableVoice);
        }
        if (availableVoice != 0 || pcbCompressed < 1)
        {
            return;
        }
        if (pcbCompressed > compressedVoiceBuffer.Length)
        {
            UnturnedLog.info($"Resizing compressed voice buffer ({compressedVoiceBuffer.Length}) to fit available size ({pcbCompressed})");
            compressedVoiceBuffer = new byte[pcbCompressed];
        }
        uint compressedSize;
        EVoiceResult voice = SteamUser.GetVoice(bWantCompressed: true, compressedVoiceBuffer, pcbCompressed, out compressedSize);
        if (voice != 0 && voice != EVoiceResult.k_EVoiceResultNoData)
        {
            UnturnedLog.error("GetVoice result: " + voice);
        }
        if (voice != 0 || compressedSize < 1 || !_inputWantsToRecord)
        {
            return;
        }
        if (Provider.isServer)
        {
            AppendVoiceData(compressedVoiceBuffer, compressedSize, hasUseableWalkieTalkie);
            return;
        }
        SendVoiceChatRelay.Invoke(GetNetId(), ENetReliability.Unreliable, delegate(NetPakWriter writer)
        {
            ushort num = (ushort)compressedSize;
            writer.WriteUInt16(num);
            writer.WriteBit(hasUseableWalkieTalkie);
            writer.WriteBytes(compressedVoiceBuffer, num);
        });
    }

    /// <summary>
    /// Play walkie-talkie squawk at our position.
    /// </summary>
    private void playWalkieTalkieSound()
    {
        OneShotAudioParameters oneShotAudioParameters = new OneShotAudioParameters(base.transform.position, radioClip);
        oneShotAudioParameters.RandomizeVolume(0.74f, 0.76f);
        oneShotAudioParameters.RandomizePitch(0.99f, 1.01f);
        oneShotAudioParameters.SetSpatialBlend2D();
        oneShotAudioParameters.Play();
    }

    /// <summary>
    /// Start and stop playback of received audio stream.
    /// </summary>
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
            if (canEverRecord)
            {
                OptionsSettings.OnVoiceAlwaysRecordingChanged += SynchronizeSteamIsRecording;
                SynchronizeSteamIsRecording();
            }
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
        isBeingDestroyed = true;
        if (canEverRecord)
        {
            OptionsSettings.OnVoiceAlwaysRecordingChanged -= SynchronizeSteamIsRecording;
            inputWantsToRecord = false;
        }
    }

    private void SynchronizeSteamIsRecording()
    {
        bool chatVoiceOut = OptionsSettings.chatVoiceOut;
        SteamIsRecording = chatVoiceOut && !isBeingDestroyed && (inputWantsToRecord || OptionsSettings.VoiceAlwaysRecording);
    }
}
