using System;
using System.Diagnostics;

namespace SDG.Unturned;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Parameter)]
public class NetPakVectorAsYawAttribute : Attribute
{
    public readonly int yawBitCount;

    public NetPakVectorAsYawAttribute(int yawBitCount = 16)
    {
        this.yawBitCount = yawBitCount;
    }
}
