using System;

namespace SDG.Unturned;

[AttributeUsage(AttributeTargets.Method)]
public class SteamCall : Attribute
{
    public ESteamCallValidation validation;

    public int ratelimitHz = -1;

    public float ratelimitSeconds = -1f;

    public int rateLimitIndex = -1;

    public string legacyName;

    public ENetInvocationDeferMode deferMode;

    public SteamCall(ESteamCallValidation validation)
    {
        this.validation = validation;
    }
}
