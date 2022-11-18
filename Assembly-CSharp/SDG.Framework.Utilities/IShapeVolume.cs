using UnityEngine;

namespace SDG.Framework.Utilities;

public interface IShapeVolume
{
    Bounds worldBounds { get; }

    float internalVolume { get; }

    float surfaceArea { get; }

    bool containsPoint(Vector3 point);
}
