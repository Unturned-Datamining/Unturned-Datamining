using System;
using System.Diagnostics;

namespace SDG.NetPak;

/// <summary>
/// Indicates net reader/writer implementation should be generated.
/// </summary>
[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Enum)]
public class NetEnumAttribute : Attribute
{
}
