using System;
using System.Reflection;

namespace SDG.Unturned;

public class ClientMethodInfo
{
    internal Type declaringType;

    internal string name;

    internal string debugName;

    internal SteamCall customAttribute;

    internal ClientMethodReceive readMethod;

    internal MethodInfo writeMethodInfo;

    internal uint methodIndex;

    public override string ToString()
    {
        return debugName;
    }
}
