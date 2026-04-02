using System;

namespace SDG.Unturned;

public interface IUnturnedPathfindingInterface
{
    void OnGameLevelInstantiated();

    IUnturnedNavmeshInterface CreateNavmesh();

    /// <summary>
    /// Create editor-only per-navmesh marker.
    /// </summary>
    IUnturnedPerNavmeshEditorInterface CreateFlag(Flag owner);

    /// <summary>
    /// IOBS gets or adds a NavmeshCut component in some situations.
    /// Returns null if not applicable.
    /// </summary>
    IUnturnedNavmeshCutInterface CreateCutForIOBS(InteractableObjectBinaryState iobs);

    Type GetCutComponentType();

    IUnturnedPathfindingMovementComponentInterface CreateMovementComponentForZombie(Zombie zombie);
}
