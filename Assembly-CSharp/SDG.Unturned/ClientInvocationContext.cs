using System.Diagnostics;
using SDG.NetPak;

namespace SDG.Unturned;

/// <summary>
/// Optional parameter for error logging.
/// </summary>
public readonly struct ClientInvocationContext
{
    public enum EOrigin
    {
        Remote,
        Loopback,
        Deferred
    }

    public readonly EOrigin origin;

    public readonly NetPakReader reader;

    internal readonly ClientMethodInfo clientMethodInfo;

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("DEBUG_NETINVOKABLES")]
    public void ReadParameterFailed(string parameterName)
    {
        UnturnedLog.warn("{0}: unable to read {1}", clientMethodInfo, parameterName);
    }

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("DEBUG_NETINVOKABLES")]
    public void IndexOutOfRange(string parameterName, int index, int max)
    {
        UnturnedLog.error("{0}: {1} out of range ({2}/{3})", clientMethodInfo, parameterName, index, max);
    }

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    [Conditional("DEBUG_NETINVOKABLES")]
    public void LogWarning(string message)
    {
        UnturnedLog.warn("{0}: {1}", clientMethodInfo, message);
    }

    internal ClientInvocationContext(EOrigin origin, NetPakReader reader, ClientMethodInfo clientMethodInfo)
    {
        this.origin = origin;
        this.reader = reader;
        this.clientMethodInfo = clientMethodInfo;
    }
}
