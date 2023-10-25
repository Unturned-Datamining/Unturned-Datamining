using System;
using System.IO;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Responsible for the per-process .log file in the Logs directory.
/// Kept multiple log files in the past, but now consolidates all information
/// into a single file named Client.log or Server_{Identifier}.log.
/// </summary>
public class Logs : MonoBehaviour
{
    /// <summary>
    /// Should setup of the default *.log file be disabled?
    /// </summary>
    public static CommandLineFlag noDefaultLog = new CommandLineFlag(defaultValue: false, "-NoDefaultLog");

    private static LogFile debugLog = null;

    public static void printLine(string message)
    {
        if (debugLog != null && !string.IsNullOrEmpty(message))
        {
            string text = message.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                string arg = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                debugLog.writeLine($"[{arg}] {text}");
            }
        }
    }

    /// <summary>
    /// Get logging to path.
    /// </summary>
    public static string getLogFilePath()
    {
        if (debugLog == null)
        {
            return null;
        }
        return debugLog.path;
    }

    /// <summary>
    /// Set path to log to.
    /// </summary>
    public static void setLogFilePath(string logFilePath)
    {
        if (!logFilePath.EndsWith(".log"))
        {
            throw new ArgumentException("should be a .log file", "logFilePath");
        }
        closeLogFile();
        try
        {
            string directoryName = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        try
        {
            if (File.Exists(logFilePath))
            {
                string text = logFilePath.Insert(logFilePath.Length - 4, "_Prev");
                if (File.Exists(text))
                {
                    File.Delete(text);
                }
                File.Move(logFilePath, text);
            }
        }
        catch (Exception exception2)
        {
            Debug.LogException(exception2);
        }
        try
        {
            debugLog = new LogFile(logFilePath);
        }
        catch (Exception exception3)
        {
            Debug.LogException(exception3);
        }
    }

    /// <summary>
    /// Close current log file.
    /// </summary>
    public static void closeLogFile()
    {
        if (debugLog != null)
        {
            debugLog.close();
            debugLog = null;
        }
    }

    public void awake()
    {
        if (!noDefaultLog)
        {
            string pATH = ReadWrite.PATH;
            pATH = ((!Dedicator.IsDedicatedServer) ? (pATH + "/Logs/Client.log") : (pATH + "/Logs/Server_" + Dedicator.serverID.Replace(' ', '_') + ".log"));
            double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
            setLogFilePath(pATH);
            double num = Time.realtimeSinceStartupAsDouble - realtimeSinceStartupAsDouble;
            if (num > 0.1)
            {
                UnturnedLog.info($"Initializing logging took {num}s");
            }
            NetReflection.SetLogCallback(UnturnedLog.info);
        }
    }

    private void OnDestroy()
    {
        closeLogFile();
    }
}
