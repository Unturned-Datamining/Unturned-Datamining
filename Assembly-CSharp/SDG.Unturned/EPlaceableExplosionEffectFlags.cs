using System;

namespace SDG.Unturned;

/// <summary>
/// Determines how the "Explosion" effect is spawned when a barricade or structure is destroyed.
///
/// Nelson 2025-09-08: although explosion effect currently exists in Barricade and Structure
/// sub-classes I think it makes sense to share this option (and ideally more in future).
/// </summary>
[Flags]
public enum EPlaceableExplosionEffectFlags
{
    /// <summary>
    /// Legacy behavior.
    /// </summary>
    None = 0,
    /// <summary>
    /// Effect spawns exactly at the model position without any offset.
    /// </summary>
    CopyModelPosition = 1,
    /// <summary>
    /// Effect spawns with same rotation as the model.
    /// </summary>
    CopyModelRotation = 2
}
