using System;

namespace SDG.Unturned;

internal struct UpdateBlueprintStatusParameters
{
    public BlueprintStatus status;

    /// <summary>
    /// If true, cancel updating status as soon as anything goes wrong.
    /// False for client UI where all info about blueprint is needed for display.
    /// True on server where extra processing is a waste.
    /// </summary>
    public bool shouldExitEarly;

    /// <summary>
    /// If set, log errors here.
    /// </summary>
    public Action<string> logCallback;
}
