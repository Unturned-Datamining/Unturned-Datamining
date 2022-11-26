using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SDG.Unturned;

internal class BuiltinAutoShutdown : MonoBehaviour
{
    public bool isScheduledShutdownEnabled;

    public DateTime scheduledShutdownTime;

    private double scheduledShutdownRealtime;

    private List<double> scheduledShutdownWarnings;

    private int scheduledShutdownWarningIndex = -1;

    private bool isShuttingDownForUpdate;

    private bool isUpdateRollback;

    private double updateShutdownRealtime;

    private string updateVersionString;

    private List<double> updateShutdownWarnings;

    private int updateShutdownWarningIndex = -1;

    private void InitScheduledShutdown()
    {
        if (!Provider.configData.Server.Enable_Scheduled_Shutdown)
        {
            return;
        }
        if (!DateTime.TryParse(Provider.configData.Server.Scheduled_Shutdown_Time, out var result))
        {
            CommandWindow.LogWarning("Unable to parse scheduled shutdown time \"" + Provider.configData.Server.Scheduled_Shutdown_Time + "\"");
            return;
        }
        isScheduledShutdownEnabled = true;
        DateTime utcNow = DateTime.UtcNow;
        scheduledShutdownTime = utcNow.Date + result.ToUniversalTime().TimeOfDay;
        if (scheduledShutdownTime < utcNow)
        {
            scheduledShutdownTime = scheduledShutdownTime.AddDays(1.0);
        }
        TimeSpan timeSpan = scheduledShutdownTime - DateTime.UtcNow;
        CommandWindow.LogFormat($"Shutdown is scheduled for {scheduledShutdownTime.ToLocalTime()} ({timeSpan:g} from now)");
        scheduledShutdownRealtime = Time.realtimeSinceStartupAsDouble + timeSpan.TotalSeconds;
        scheduledShutdownWarnings = new List<double>(Provider.configData.Server.Scheduled_Shutdown_Warnings.Length);
        string[] scheduled_Shutdown_Warnings = Provider.configData.Server.Scheduled_Shutdown_Warnings;
        foreach (string text in scheduled_Shutdown_Warnings)
        {
            if (TimeSpan.TryParse(text, out var result2))
            {
                scheduledShutdownWarnings.Add(result2.TotalSeconds);
            }
            else
            {
                CommandWindow.LogWarning("Unable to parse scheduled shutdown warning time \"" + text + "\"");
            }
        }
        scheduledShutdownWarnings.Sort();
        if (scheduledShutdownWarnings.Count > 0)
        {
            double num = scheduledShutdownRealtime - Time.realtimeSinceStartupAsDouble;
            scheduledShutdownWarningIndex = scheduledShutdownWarnings.Count - 1;
            while (scheduledShutdownWarningIndex >= 0 && !(num > scheduledShutdownWarnings[scheduledShutdownWarningIndex]))
            {
                scheduledShutdownWarningIndex--;
            }
        }
        else
        {
            scheduledShutdownWarningIndex = -1;
        }
    }

    private void InitUpdateShutdown()
    {
        if (!Provider.configData.Server.Enable_Update_Shutdown)
        {
            return;
        }
        if ((bool)Dedicator.offlineOnly)
        {
            CommandWindow.LogWarning("Disabling check for game updates because Offline-Only mode is enabled");
            return;
        }
        string update_Steam_Beta_Name = Provider.configData.Server.Update_Steam_Beta_Name;
        if (string.IsNullOrEmpty(update_Steam_Beta_Name))
        {
            CommandWindow.LogWarning("Unable to check for game updates with empty Steam beta name (default is \"public\")");
            return;
        }
        updateShutdownWarnings = new List<double>(Provider.configData.Server.Update_Shutdown_Warnings.Length);
        string[] update_Shutdown_Warnings = Provider.configData.Server.Update_Shutdown_Warnings;
        foreach (string text in update_Shutdown_Warnings)
        {
            if (TimeSpan.TryParse(text, out var result))
            {
                updateShutdownWarnings.Add(result.TotalSeconds);
            }
            else
            {
                CommandWindow.LogWarning("Unable to parse update shutdown warning time \"" + text + "\"");
            }
        }
        updateShutdownWarnings.Sort();
        CommandWindow.LogFormat("Monitoring for game updates on Steam beta branch \"" + update_Steam_Beta_Name + "\"");
        string url = "https://smartlydressedgames.com/unturned-steam-versions/" + update_Steam_Beta_Name + ".txt";
        StartCoroutine(CheckVersion(url));
    }

    private void OnEnable()
    {
        InitScheduledShutdown();
        InitUpdateShutdown();
    }

    private void Update()
    {
        if (isShuttingDownForUpdate)
        {
            double num = updateShutdownRealtime - Time.realtimeSinceStartupAsDouble;
            if (num < 0.0)
            {
                isShuttingDownForUpdate = false;
                string key = (isUpdateRollback ? "RollbackShutdown_KickExplanation" : "UpdateShutdown_KickExplanation");
                Provider.shutdown(0, Provider.localization.format(key, updateVersionString));
            }
            else if (updateShutdownWarnings.Count > 0 && updateShutdownWarningIndex >= 0 && num < updateShutdownWarnings[updateShutdownWarningIndex])
            {
                TimeSpan timeSpan = new TimeSpan(0, 0, (int)updateShutdownWarnings[updateShutdownWarningIndex]);
                string key2 = (isUpdateRollback ? "RollbackShutdown_Timer" : "UpdateShutdown_Timer");
                string text = Provider.localization.format(key2, updateVersionString, timeSpan.ToString("g"));
                CommandWindow.Log(text);
                ChatManager.say(text, ChatManager.welcomeColor);
                updateShutdownWarningIndex--;
            }
        }
        else if (isScheduledShutdownEnabled)
        {
            double num2 = scheduledShutdownRealtime - Time.realtimeSinceStartupAsDouble;
            if (num2 < 0.0)
            {
                isScheduledShutdownEnabled = false;
                Provider.shutdown(0, Provider.localization.format("ScheduledMaintenance_KickExplanation"));
            }
            else if (scheduledShutdownWarnings.Count > 0 && scheduledShutdownWarningIndex >= 0 && num2 < scheduledShutdownWarnings[scheduledShutdownWarningIndex])
            {
                TimeSpan timeSpan2 = new TimeSpan(0, 0, (int)scheduledShutdownWarnings[scheduledShutdownWarningIndex]);
                string text2 = Provider.localization.format("ScheduledMaintenance_Timer", timeSpan2.ToString("g"));
                CommandWindow.Log(text2);
                ChatManager.say(text2, ChatManager.welcomeColor);
                scheduledShutdownWarningIndex--;
            }
        }
    }

    private IEnumerator CheckVersion(string url)
    {
        yield return new WaitForSecondsRealtime(300f);
        string text;
        uint value;
        while (true)
        {
            UnturnedLog.info("Checking for game updates...");
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = 30;
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                text = request.downloadHandler.text;
                if (Parser.TryGetUInt32FromIP(text, out value))
                {
                    if (value != Provider.APP_VERSION_PACKED)
                    {
                        break;
                    }
                    UnturnedLog.info("Game version is up-to-date");
                }
                else
                {
                    UnturnedLog.info("Unable to parse newest game version \"" + text + "\"");
                }
            }
            else
            {
                UnturnedLog.info("Network error checking for game updates: \"" + request.error + "\"");
            }
            yield return new WaitForSecondsRealtime(600f);
        }
        if (value > Provider.APP_VERSION_PACKED)
        {
            CommandWindow.Log("Detected newer game version: " + text);
        }
        else
        {
            CommandWindow.Log("Detected rollback to older game version: " + text);
        }
        bool shouldShutdown = true;
        GameUpdateMonitor.NotifyGameUpdateDetected(text, ref shouldShutdown);
        if (shouldShutdown)
        {
            isShuttingDownForUpdate = true;
            isUpdateRollback = value < Provider.APP_VERSION_PACKED;
            updateVersionString = text;
            updateShutdownWarningIndex = updateShutdownWarnings.Count - 1;
            updateShutdownRealtime = Time.realtimeSinceStartupAsDouble + ((updateShutdownWarningIndex >= 0) ? updateShutdownWarnings[updateShutdownWarningIndex] : 0.0);
        }
    }
}
