using UnityEngine;

namespace SDG.Framework.Utilities;

public interface IShapeVolume
{
    /// <summary>
    /// Not necessarily cheap to calculate - probably best to cache.
    /// </summary>
    Bounds worldBounds { get; }

    /// <summary>
    /// Internal cubic meter volume.
    /// </summary>
    float internalVolume { get; }

    /// <summary>
    /// Surface square meters area.
    /// </summary>
    float surfaceArea { get; }

    bool containsPoint(Vector3 point);
}
