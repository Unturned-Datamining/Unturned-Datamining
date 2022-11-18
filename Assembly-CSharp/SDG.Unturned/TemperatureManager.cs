using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class TemperatureManager : MonoBehaviour
{
    private static List<TemperatureBubble> bubbles;

    public static EPlayerTemperature checkPointTemperature(Vector3 point, bool proofFire)
    {
        EPlayerTemperature ePlayerTemperature = EPlayerTemperature.NONE;
        for (int i = 0; i < bubbles.Count; i++)
        {
            TemperatureBubble temperatureBubble = bubbles[i];
            if (!(temperatureBubble.origin == null) && (!proofFire || temperatureBubble.temperature != EPlayerTemperature.BURNING) && (temperatureBubble.origin.position - point).sqrMagnitude < temperatureBubble.sqrRadius)
            {
                if (temperatureBubble.temperature == EPlayerTemperature.ACID)
                {
                    return temperatureBubble.temperature;
                }
                if (temperatureBubble.temperature == EPlayerTemperature.BURNING)
                {
                    ePlayerTemperature = temperatureBubble.temperature;
                }
                else if (ePlayerTemperature != EPlayerTemperature.BURNING)
                {
                    ePlayerTemperature = temperatureBubble.temperature;
                }
            }
        }
        return ePlayerTemperature;
    }

    public static TemperatureBubble registerBubble(Transform origin, float radius, EPlayerTemperature temperature)
    {
        TemperatureBubble temperatureBubble = new TemperatureBubble(origin, radius * radius, temperature);
        bubbles.Add(temperatureBubble);
        return temperatureBubble;
    }

    public static void deregisterBubble(TemperatureBubble bubble)
    {
        bubbles.Remove(bubble);
    }

    private void onLevelLoaded(int level)
    {
        bubbles = new List<TemperatureBubble>();
    }

    private void Start()
    {
        Level.onPrePreLevelLoaded = (PrePreLevelLoaded)Delegate.Combine(Level.onPrePreLevelLoaded, new PrePreLevelLoaded(onLevelLoaded));
    }
}
