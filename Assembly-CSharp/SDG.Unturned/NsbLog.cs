using System.Diagnostics;

namespace SDG.Unturned;

/// <summary>
/// Logs enabled when WITH_NSB_LOGGING is defined.
/// Tracking down an issue where snapshot buffer stops working for groups of networked objects.
/// </summary>
public static class NsbLog
{
    [Conditional("WITH_NSB_LOGGING")]
    public static void Warning(object message)
    {
    }

    [Conditional("WITH_NSB_LOGGING")]
    public static void ConditionalWarning(bool condition, object message)
    {
    }

    [Conditional("WITH_NSB_LOGGING")]
    public static void WarningFormat(string format, params object[] args)
    {
    }

    [Conditional("WITH_NSB_LOGGING")]
    public static void ConditionalWarningFormat(bool condition, string format, params object[] args)
    {
    }
}
