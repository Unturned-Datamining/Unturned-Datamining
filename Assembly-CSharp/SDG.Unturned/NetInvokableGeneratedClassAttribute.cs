using System;

namespace SDG.Unturned;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class NetInvokableGeneratedClassAttribute : Attribute
{
    public readonly Type targetType;

    public NetInvokableGeneratedClassAttribute(Type targetType)
    {
        this.targetType = targetType;
    }
}
