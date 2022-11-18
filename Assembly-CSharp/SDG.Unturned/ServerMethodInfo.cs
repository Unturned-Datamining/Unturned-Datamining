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

    internal int rateLimitIndex;

    public override string ToString()
    {
        return debugName;
    }
}
