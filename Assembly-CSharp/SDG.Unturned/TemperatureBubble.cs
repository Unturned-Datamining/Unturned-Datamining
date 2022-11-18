using UnityEngine;

namespace SDG.Unturned;

public class TemperatureBubble
{
    public Transform origin;

    public float sqrRadius;

    public EPlayerTemperature temperature;

    public TemperatureBubble(Transform newOrigin, float newSqrRadius, EPlayerTemperature newTemperature)
    {
        origin = newOrigin;
        sqrRadius = newSqrRadius;
        temperature = newTemperature;
    }
}
