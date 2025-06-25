using System;
using System.Diagnostics;

namespace SDG.Unturned;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Parameter)]
public class NetPakNormalAsYawAttribute : Attribute
{
    public readonly int bitCount;

    public NetPakNormalAsYawAttribute(int bitCount = 16)
    {
        this.bitCount = bitCount;
    }
}
