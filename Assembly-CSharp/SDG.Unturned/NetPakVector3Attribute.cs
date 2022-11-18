using System;
using System.Diagnostics;

namespace SDG.Unturned;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Parameter)]
public class NetPakVector3Attribute : Attribute
{
    public readonly int intBitCount;

    public readonly int fracBitCount;

    public NetPakVector3Attribute(int intBitCount = 13, int fracBitCount = 9)
    {
        this.intBitCount = intBitCount;
        this.fracBitCount = fracBitCount;
    }
}
