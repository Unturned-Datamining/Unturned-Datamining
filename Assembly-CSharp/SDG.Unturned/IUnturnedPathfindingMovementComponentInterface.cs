using UnityEngine;

namespace SDG.Unturned;

public interface IUnturnedPathfindingMovementComponentInterface
{
    bool CanMove { get; set; }

    bool CanTurn { get; set; }

    bool CanSearch { get; set; }

    float Speed { get; set; }

    Transform TargetTransform { get; set; }

    Vector3 TargetDirection { get; set; }

    void Move(float deltaTime);
}
