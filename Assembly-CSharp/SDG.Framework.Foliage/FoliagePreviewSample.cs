using UnityEngine;

namespace SDG.Framework.Foliage;

public struct FoliagePreviewSample
{
    public Vector3 position;

    public Color color;

    public FoliagePreviewSample(Vector3 newPosition, Color newColor)
    {
        position = newPosition;
        color = newColor;
    }
}
