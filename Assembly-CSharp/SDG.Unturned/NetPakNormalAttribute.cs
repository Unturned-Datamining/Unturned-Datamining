using System;
using System.Diagnostics;

namespace SDG.Unturned;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Parameter)]
public class NetPakNormalAttribute : Attribute
{
    public readonly int bitsPerComponent;

    public NetPakNormalAttribute(int bitsPerComponent = 9)
    {
        this.bitsPerComponent = bitsPerComponent;
    }
}
