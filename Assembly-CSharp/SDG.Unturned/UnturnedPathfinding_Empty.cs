using System;

namespace SDG.Unturned;

public class UnturnedPathfinding_Empty : IUnturnedPathfindingInterface
{
    public void OnGameLevelInstantiated()
    {
    }

    public IUnturnedNavmeshInterface CreateNavmesh()
    {
        return new UnturnedNavmesh_Empty();
    }

    public IUnturnedPerNavmeshEditorInterface CreateFlag(Flag owner)
    {
        return new UnturnedNavmeshFlag_Empty();
    }

    public IUnturnedNavmeshCutInterface CreateCutForIOBS(InteractableObjectBinaryState iobs)
    {
        return null;
    }

    public Type GetCutComponentType()
    {
        return null;
    }

    public IUnturnedPathfindingMovementComponentInterface CreateMovementComponentForZombie(Zombie zombie)
    {
        return zombie.gameObject.AddComponent<NonPathfindingZombieMovementComponent>();
    }
}
