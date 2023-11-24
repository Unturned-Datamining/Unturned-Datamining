using System;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Unturned wrapper for Debug.Log, Debug.LogWarning, Debug.LogError, etc.
/// </summary>
public static class UnturnedLog
{
    private static bool insideLog;

    public static void info(string message)
    {
        if (insideLog)
        {
            return;
        }
        try
        {
            insideLog = true;
            Logs.printLine(message);
        }
        finally
        {
            insideLog = false;
        }
    }

    public static void warn(string message)
    {
        if (insideLog)
        {
            return;
        }
        try
        {
            insideLog = true;
            Logs.printLine(message);
        }
        finally
        {
            insideLog = false;
        }
    }

    public static void error(string message)
    {
        if (insideLog)
        {
            return;
        }
        try
        {
            insideLog = true;
            Logs.printLine(message);
            CommandWindow.LogError(message);
        }
        finally
        {
            insideLog = false;
        }
    }

    public static void exception(Exception e)
    {
        if (e == null)
        {
            error("UnturnedLog.exception called with null argument");
        }
        else if (!insideLog)
        {
            try
            {
                insideLog = true;
                internalException(e);
            }
            finally
            {
                insideLog = false;
            }
        }
    }

    /// <summary>
    /// Log an exception with message providing context.
    /// </summary>
    public static void exception(Exception e, string message)
    {
        error(message);
        exception(e);
    }

    /// <summary>
    /// Recursively logs inner exception.
    ///
    /// Should only be called by itself and exception because notifications
    /// to CommandWindow would otherwise get re-sent here as errors.
    /// </summary>
    private static void internalException(Exception e)
    {
        string text = e.Message;
        if (string.IsNullOrEmpty(text))
        {
            text = "(empty exception message)";
        }
        string text2 = e.StackTrace;
        if (string.IsNullOrEmpty(text2))
        {
            text2 = "(empty stack trace)";
        }
        Logs.printLine(text);
        Logs.printLine(text2);
        CommandWindow.LogError(text);
        CommandWindow.LogError(text2);
        if (e.InnerException != null)
        {
            internalException(e.InnerException);
        }
    }

    /// <summary>
    /// This is the ONLY place Unturned should be binding logMessageReceived.
    ///
    /// This gives us greater control over how logging is handled. In particular, Unity's
    /// headless builds route logs (including stack traces) through stdout which is undesirable
    /// for dedicated servers, so we only call Debug.Log* in the editor and development builds. 
    /// </summary>
    private static void onBuiltinUnityLogMessageReceived(string text, string stack, LogType type)
    {
        if (!insideLog)
        {
            Logs.printLine(text);
            if ((uint)type <= 2u || type == LogType.Exception)
            {
                Logs.printLine(stack);
            }
        }
    }

    static UnturnedLog()
    {
        Application.logMessageReceived += onBuiltinUnityLogMessageReceived;
    }

    public static void info(object message)
    {
        if (message != null)
        {
            info(message.ToString());
        }
    }

    public static void warn(object message)
    {
        if (message != null)
        {
            warn(message.ToString());
        }
    }

    public static void error(object message)
    {
        if (message != null)
        {
            error(message.ToString());
        }
    }

    public static void info(string format, params object[] args)
    {
        info(string.Format(format, args));
    }

    public static void warn(string format, params object[] args)
    {
        warn(string.Format(format, args));
    }

    public static void error(string format, params object[] args)
    {
        error(string.Format(format, args));
    }

    /// <summary>
    /// Log an exception with message providing context.
    /// </summary>
    public static void exception(Exception e, string format, params object[] args)
    {
        try
        {
            error(string.Format(format, args));
        }
        catch
        {
        }
        exception(e);
    }
}
