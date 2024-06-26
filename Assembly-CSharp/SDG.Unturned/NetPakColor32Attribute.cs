using System;
using System.Diagnostics;

namespace SDG.Unturned;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Parameter)]
public class NetPakColor32Attribute : Attribute
{
    public bool withAlpha;

    public NetPakColor32Attribute(bool withAlpha)
    {
        this.withAlpha = withAlpha;
    }

    private NetPakColor32Attribute()
    {
    }
}
