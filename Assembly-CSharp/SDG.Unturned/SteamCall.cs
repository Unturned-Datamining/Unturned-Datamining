using System;

namespace SDG.Unturned;

[AttributeUsage(AttributeTargets.Method)]
public class SteamCall : Attribute
{
    public ESteamCallValidation validation;

    /// <summary>
    /// Maximum number of calls per-second per-player.
    /// </summary>
    public int ratelimitHz = -1;

    /// <summary>
    /// Minimum seconds between calls per-player.
    /// Initialized from ratelimitHz when gathering RPCs.
    /// </summary>
    public float ratelimitSeconds = -1f;

    /// <summary>
    /// Index into per-connection rate limiting array.
    /// </summary>
    public int rateLimitIndex = -1;

    /// <summary>
    /// Backwards compatibility for older invoke by name code e.g. plugins.
    /// </summary>
    public string legacyName;

    public ENetInvocationDeferMode deferMode;

    public SteamCall(ESteamCallValidation validation)
    {
        this.validation = validation;
    }
}
