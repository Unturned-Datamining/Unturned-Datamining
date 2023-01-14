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

    public int Rate_Limit_Kick_Threshold;

    public float Fake_Lag_Threshold_Seconds;

    public bool Fake_Lag_Log_Warnings;

    public float Fake_Lag_Damage_Penalty_Multiplier;

    public bool Enable_Kick_Input_Spam;

    public bool Enable_Kick_Input_Timeout;

    public bool Enable_Scheduled_Shutdown;

    public string Scheduled_Shutdown_Time = "1:30 am";

    public string[] Scheduled_Shutdown_Warnings = new string[9] { "00:30:00", "00:15:00", "00:05:00", "00:01:00", "00:00:30", "00:00:15", "00:00:03", "00:00:02", "00:00:01" };

    public bool Enable_Update_Shutdown;

    public string Update_Steam_Beta_Name = "public";

    public string[] Update_Shutdown_Warnings = new string[7] { "00:03:00", "00:01:00", "00:00:30", "00:00:15", "00:00:03", "00:00:02", "00:00:01" };

    [NonSerialized]
    [Obsolete("uGUI supports rich text fade out.")]
    public bool Chat_Always_Use_Rich_Text;

    public bool Validate_EconInfo_Hash;

    public bool Validate_MasterBundle_Hashes;

    internal const float MAX_TIMEOUT_QUEUE_SECONDS = 25f;

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
        Validate_MasterBundle_Hashes = true;
    }

    internal float GetClampedTimeoutQueueSeconds()
    {
        return Mathf.Clamp(Timeout_Queue_Seconds, 1f, 25f);
    }
}
