using UnityEngine;

namespace SDG.Framework.Landscapes;

public struct LandscapePreviewSample
{
    public Vector3 position;

    public float weight;

    public LandscapePreviewSample(Vector3 newPosition, float newWeight)
    {
        position = newPosition;
        weight = newWeight;
    }
}
