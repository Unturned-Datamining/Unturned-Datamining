using System;
using UnityEngine;

namespace SDG.Unturned;

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
        if (insideLog)
        {
            return;
        }
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

    public static void exception(Exception e, string format, params object[] args)
    {
        error(format, args);
        exception(e);
    }

    private static void internalException(Exception e)
    {
        Logs.printLine(e.Message);
        Logs.printLine(e.StackTrace);
        CommandWindow.LogError(e.Message);
        CommandWindow.LogError(e.StackTrace);
        if (e.InnerException != null)
        {
            internalException(e.InnerException);
        }
    }

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
}
