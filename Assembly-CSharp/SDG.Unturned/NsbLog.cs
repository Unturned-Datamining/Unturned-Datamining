using System.Diagnostics;

namespace SDG.Unturned;

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
