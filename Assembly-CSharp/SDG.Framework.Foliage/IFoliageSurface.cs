using UnityEngine;

namespace SDG.Framework.Foliage;

public interface IFoliageSurface
{
    FoliageBounds getFoliageSurfaceBounds();

    bool getFoliageSurfaceInfo(Vector3 position, out Vector3 surfacePosition, out Vector3 surfaceNormal);

    void bakeFoliageSurface(FoliageBakeSettings bakeSettings, FoliageTile foliageTile);
}
