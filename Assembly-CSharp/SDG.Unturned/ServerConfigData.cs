using System;
using UnityEngine;

namespace SDG.Unturned;

public class ServerConfigData
{
    public bool VAC_Secure;

    public bool BattlEye_Secure;

    public uint Max_Ping_Milliseconds;

    public float Timeout_Queue_Seconds;

    public float Timeout_Game_Seconds;

    public float Max_Packets_Per_Second;

    /// <summary>
    /// If a rate-limited method is called this many times within cooldown window the client will be kicked.
    /// For example a value of 1 means the client will be kicked the first time they call the method off-cooldown. (not recommended)
    /// </summary>
    public int Rate_Limit_Kick_Threshold;

    /// <summary>
    /// Ordinarily the server should be receiving multiple input packets per second from a client. If more than this
    /// amount of time passes between input packets we flag the client as potentially using a lag switch, and modify
    /// their stats (e.g. reduce player damage) for a corresponding duration.
    /// Minimum value is PlayerInput.MIN_FAKE_LAG_THRESHOLD_SECONDS.
    /// </summary>
    public float Fake_Lag_Threshold_Seconds;

    /// <summary>
    /// Whether fake lag detection should log to command output. False positives are relatively likely when client
    /// framerate hitches (e.g. loading dense region), so this is best used for tuning threshold rather than bans.
    /// </summary>
    public bool Fake_Lag_Log_Warnings;

    /// <summary>
    /// PvP damage multiplier while under fake lag penalty.
    /// </summary>
    public float Fake_Lag_Damage_Penalty_Multiplier;

    /// <summary>
    /// Should we kick players after detecting spammed calls to askInput?
    /// </summary>
    public bool Enable_Kick_Input_Spam;

    /// <summary>
    /// Should we kick players if they do not submit inputs for a long time?
    /// </summary>
    public bool Enable_Kick_Input_Timeout;

    /// <summary>
    /// Should the server automatically shutdown at a configured time?
    /// </summary>
    public bool Enable_Scheduled_Shutdown;

    /// <summary>
    /// When the server should shutdown if Enable_Scheduled_Shutdown is true.
    /// </summary>
    public string Scheduled_Shutdown_Time = "1:30 am";

    /// <summary>
    /// Broadcast "shutting down for scheduled maintenance" warnings at these intervals.
    /// </summary>
    public string[] Scheduled_Shutdown_Warnings = new string[9] { "00:30:00", "00:15:00", "00:05:00", "00:01:00", "00:00:30", "00:00:15", "00:00:03", "00:00:02", "00:00:01" };

    /// <summary>
    /// Should the server automatically shutdown when a new version is detected?
    /// </summary>
    public bool Enable_Update_Shutdown;

    /// <summary>
    /// Unfortunately the server does not have a way to automatically determine the current beta branch.
    /// </summary>
    public string Update_Steam_Beta_Name = "public";

    /// <summary>
    /// Broadcast "shutting down for update" warnings at these intervals.
    /// </summary>
    public string[] Update_Shutdown_Warnings = new string[7] { "00:03:00", "00:01:00", "00:00:30", "00:00:15", "00:00:03", "00:00:02", "00:00:01" };

    /// <summary>
    /// Should vanilla text chat messages always use rich text?
    /// Servers with plugins may want to enable because IMGUI does not fade out rich text.
    /// Kept because plugins might be setting this directly, but it no longer does anything.
    /// </summary>
    [NonSerialized]
    [Obsolete("uGUI supports rich text fade out.")]
    public bool Chat_Always_Use_Rich_Text;

    /// <summary>
    /// Should the EconInfo.json hash be checked by the server?
    /// </summary>
    public bool Validate_EconInfo_Hash;

    /// <summary>
    /// If true, opt-in to SteamNetworkingSockets "FakeIP" system.
    /// https://partner.steamgames.com/doc/api/ISteamNetworkingSockets#1
    /// </summary>
    public bool Use_FakeIP;

    /// <summary>
    /// Limit max queue timeout duration so that if server encounters an error or doesn't
    /// process the request the client can timeout locally.
    /// </summary>
    internal const float MAX_TIMEOUT_QUEUE_SECONDS = 25f;

    /// <summary>
    /// Longer than server timeout so that ideally more context is logged on the server
    /// rather than just "client disconnected."
    /// </summary>
    internal const float CLIENT_TIMEOUT_QUEUE_SECONDS = 30f;

    public ServerConfigData()
    {
        VAC_Secure = true;
        BattlEye_Secure = true;
        Max_Ping_Milliseconds = 750u;
        Timeout_Queue_Seconds = 15f;
        Timeout_Game_Seconds = 30f;
        Max_Packets_Per_Second = 50f;
        Rate_Limit_Kick_Threshold = 10;
        Fake_Lag_Threshold_Seconds = 3f;
        Fake_Lag_Damage_Penalty_Multiplier = 0.1f;
        Enable_Kick_Input_Spam = false;
        Enable_Kick_Input_Timeout = false;
        Validate_EconInfo_Hash = true;
    }

    internal float GetClampedTimeoutQueueSeconds()
    {
        return Mathf.Clamp(Timeout_Queue_Seconds, 1f, 25f);
    }
}
