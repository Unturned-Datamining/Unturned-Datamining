using System;
using System.Diagnostics;

namespace SDG.Unturned;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Parameter)]
public class NetPakSpecialQuaternionAttribute : Attribute
{
    public readonly int yawBitCount;

    public NetPakSpecialQuaternionAttribute(int yawBitCount = 9)
    {
        this.yawBitCount = yawBitCount;
    }
}
