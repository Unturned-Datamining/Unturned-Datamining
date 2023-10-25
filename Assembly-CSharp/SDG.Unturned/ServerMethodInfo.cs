using System;
using System.Reflection;

namespace SDG.Unturned;

public class ServerMethodInfo
{
    internal Type declaringType;

    internal string name;

    internal string debugName;

    internal SteamCall customAttribute;

    internal ServerMethodReceive readMethod;

    internal MethodInfo writeMethodInfo;

    internal uint methodIndex;

    /// <summary>
    /// Index into per-connection rate limiting array.
    /// </summary>
    internal int rateLimitIndex;

    public override string ToString()
    {
        return debugName;
    }
}
