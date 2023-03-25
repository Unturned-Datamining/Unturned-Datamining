using System;
using System.Diagnostics;

namespace SDG.NetPak;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Enum)]
public class NetEnumAttribute : Attribute
{
}
