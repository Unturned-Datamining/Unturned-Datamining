using System;

namespace SDG.Unturned;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class NetInvokableGeneratedClassAttribute : Attribute
{
    /// <summary>
    /// Type the annotated class was generated for.
    /// </summary>
    public readonly Type targetType;

    public NetInvokableGeneratedClassAttribute(Type targetType)
    {
        this.targetType = targetType;
    }
}
