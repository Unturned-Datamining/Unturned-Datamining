using System;

namespace SDG.Unturned;

/// <summary>
/// 32-bit mask indicating to the server which admin powers are being used.
/// Does not control which admin powers are available.
/// </summary>
[Flags]
public enum EPlayerAdminUsageFlags
{
    None = 0,
    /// <summary>
    /// Player is using spectator camera.
    /// </summary>
    Freecam = 1,
    /// <summary>
    /// Player is using barricade/structure transform tools.
    /// </summary>
    Workzone = 2,
    /// <summary>
    /// Player is using overlay showing player names and positions.
    /// </summary>
    SpectatorStatsOverlay = 4
}
