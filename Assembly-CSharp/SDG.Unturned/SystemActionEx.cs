using System;

namespace SDG.Unturned;

public static class SystemActionEx
{
    public static void TryInvoke(this System.Action action, string debugName)
    {
        try
        {
            action?.Invoke();
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception invoking {0}:", debugName);
        }
    }

    public static void TryInvoke<T>(this Action<T> action, string debugName, T obj)
    {
        try
        {
            action?.Invoke(obj);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception invoking {0}({1}):", debugName, obj);
        }
    }

    public static void TryInvoke<T1, T2>(this Action<T1, T2> action, string debugName, T1 arg1, T2 arg2)
    {
        try
        {
            action?.Invoke(arg1, arg2);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception invoking {0}({1}, {2}):", debugName, arg1, arg2);
        }
    }

    public static void TryInvoke<T1, T2, T3>(this Action<T1, T2, T3> action, string debugName, T1 arg1, T2 arg2, T3 arg3)
    {
        try
        {
            action?.Invoke(arg1, arg2, arg3);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception invoking {0}({1}, {2}, {3}):", debugName, arg1, arg2, arg3);
        }
    }

    public static void TryInvoke<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, string debugName, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        try
        {
            action?.Invoke(arg1, arg2, arg3, arg4);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception invoking {0}({1}, {2}, {3}, {4}):", debugName, arg1, arg2, arg3, arg4);
        }
    }
}
