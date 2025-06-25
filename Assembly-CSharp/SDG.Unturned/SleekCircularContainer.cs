using System;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Non-item replacement for SleekJars.
/// Arranges children in an equally-spaced ring around the center.
/// </summary>
public class SleekCircularContainer : SleekWrapper
{
    public float StartAngleRadians { get; set; }

    public float Radius { get; set; }

    public void UpdateLayout()
    {
        int childCount = GetChildCount();
        if (childCount >= 1)
        {
            float num = MathF.PI * 2f / (float)childCount;
            for (int i = 0; i < childCount; i++)
            {
                ISleekElement childAtIndex = GetChildAtIndex(i);
                childAtIndex.PositionOffset_X = Mathf.Cos(StartAngleRadians + num * (float)i) * Radius - childAtIndex.SizeOffset_X / 2f;
                childAtIndex.PositionOffset_Y = Mathf.Sin(StartAngleRadians + num * (float)i) * Radius - childAtIndex.SizeOffset_Y / 2f;
                childAtIndex.PositionScale_X = 0.5f;
                childAtIndex.PositionScale_Y = 0.5f;
            }
        }
    }

    public SleekCircularContainer(float radius, float startAngleRadians)
    {
        StartAngleRadians = startAngleRadians;
        Radius = radius;
    }
}
