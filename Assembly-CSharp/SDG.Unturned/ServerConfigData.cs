using System;
using UnityEngine;

namespace SDG.Unturned;

public class ServerConfigData
{
    /// <summary>
    /// Whether to enable Valve Anti-Cheat.
    /// </summary>
    public bool VAC_Secure;

    /// <summary>
    /// Whether to enable BattlEye Anti-Cheat.
    /// </summary>
    public bool BattlEye_Secure;

    /// <summary>
    /// Players with a ping higher than this are kicked.
    /// </summary>
    public uint Max_Ping_Milliseconds;

    /// <summary>
    /// Players in the pre-join queue we haven't heard from in this past number of seconds are kicked.
    /// </summary>
    public float Timeout_Queue_Seconds;

    /// <summary>
    /// Players in the server we haven't heard from in this past number of seconds are kicked.
    /// </summary>
    public float Timeout_Game_Seconds;

    [NonSerialized]
    [Obsolete]
    public float Max_Packets_Per_Second;

    /// <summary>
    /// If ready-to-connect messages are received more than twice from the same client in less than this many
    /// seconds they will be kicked.
    /// </summary>
    public float Join_Rate_Limit_Window_Seconds;

    /// <summary>
    /// If bad packets (that *may* be legitimate) are received more than threshold times within this many seconds
    /// of each other, reject the calling connection.
    /// </summary>
    public float Bad_Packet_Rate_Limit_Window_Seconds;

    /// <summary>
    /// If more than this many bad packets (that *may* be legitimate) are received within window seconds of each
    /// other, reject the calling connection.
    /// </summary>
    public int Bad_Packet_Rate_Limit_Threshold;

    /// <summary>
    /// If a rate-limited method is called this many times within cooldown window the client will be kicked.
    /// For example a value of 1 means the client will be kicked the first time they call the method off-cooldown. (not recommended)
    /// </summary>
    public int Rate_Limit_Kick_Threshold;

    /// <summary>
    /// Only applicable when Fake IP is off. When a client is connecting, if their connection would push the number
    /// of simultaneous connections from the same IP address past this number, they are prevented from joining.
    ///
    /// May be useful to prevent against fake join requests coming from a single source IP. (public issue #5001)
    ///
    /// Defaults to a high value because some regions will have many more clients with the same IPv4 address than
    /// others. For example, due to Carrier-grade NAT (CGNAT).
    /// </summary>
    public int Max_Clients_With_Same_IP_Address = 64;

    /// <summary>
    /// Whether rejections for Max_Clients_With_Same_IP_Address should log to command output. Useful for checking
    /// if the limit is appropriate.
    /// </summary>
    public bool Max_Clients_With_Same_IP_Address_Log_Warnings = true;

    /// <summary>
    /// Ordinarily the server should be receiving multiple input packets per second from a client. If more than this
    /// amount of time passes between input packets we flag the client as potentially using a lag switch, and modify
    /// their stats (e.g. reduce player damage) for a corresponding duration.
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
    /// Format is a list of hours:minutes:seconds, for example to warn only 5 seconds before:
    /// ` Scheduled_Shutdown_Warnings
    /// ` [
    /// `     00:00:05
    /// ` ]
    /// Default starts at 30 minutes and counts down.
    /// </summary>
    public string[] Scheduled_Shutdown_Warnings = new string[9] { "00:30:00", "00:15:00", "00:05:00", "00:01:00", "00:00:30", "00:00:15", "00:00:03", "00:00:02", "00:00:01" };

    /// <summary>
    /// Should the server automatically shutdown when a new version is detected?
    /// </summary>
    public bool Enable_Update_Shutdown;

    /// <summary>
    /// If Enable_Update_Shutdown is true, we check for updates to this branch of the game.
    /// (Unfortunately the server does not have a way to automatically determine the current beta branch.)
    /// </summary>
    public string Update_Steam_Beta_Name = "public";

    /// <summary>
    /// Broadcast "shutting down for update" warnings at these intervals.
    /// Refer to Scheduled_Shutdown_Warnings for an explanation of the format.
    /// Default starts at 3 minutes and counts down.
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
    /// Documentation: https://docs.smartlydressedgames.com/en/stable/servers/fake-ip.html
    /// </summary>
    public bool Use_FakeIP;

    /// <summary>
    /// If greater than zero, vehicles with XZ position outside this threshold are saved in the center of the map.
    /// By default, vehicles outside ±40 km are teleported into the map.
    /// Intended to help with physics issues caused by vehicles far out in space. (public issue #4465)
    /// </summary>
    public float Reset_Vehicles_Outside_Horizontal_Distance = 40000f;

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
        Join_Rate_Limit_Window_Seconds = 40f;
        Bad_Packet_Rate_Limit_Window_Seconds = 2.5f;
        Bad_Packet_Rate_Limit_Threshold = 10;
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
