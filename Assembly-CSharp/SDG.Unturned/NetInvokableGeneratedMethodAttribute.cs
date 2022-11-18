using System;

namespace SDG.Unturned;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class NetInvokableGeneratedMethodAttribute : Attribute
{
    public readonly string targetMethodName;

    public readonly ENetInvokableGeneratedMethodPurpose purpose;

    public NetInvokableGeneratedMethodAttribute(string targetMethodName, ENetInvokableGeneratedMethodPurpose purpose)
    {
        this.targetMethodName = targetMethodName;
        this.purpose = purpose;
    }
}
