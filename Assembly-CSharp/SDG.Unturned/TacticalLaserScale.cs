using System;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Component for the tactical laser attachment's red dot.
/// Resizes itself per-camera to maintain a constant on-screen size.
/// </summary>
public class TacticalLaserScale : MonoBehaviour
{
    public float scaleMultiplier = 0.1f;

    /// <summary>
    /// Used to tune the scale by distance so that far away laser is not quite as comically large.
    /// </summary>
    public AnimationCurve scalingCurve;

    public void OnWillRenderObject()
    {
        Camera current = Camera.current;
        float magnitude = (base.transform.position - current.transform.position).magnitude;
        float num = current.fieldOfView * 0.5f;
        float num2 = Mathf.Tan(MathF.PI / 180f * num);
        float num3 = scalingCurve.Evaluate(magnitude * num2) * scaleMultiplier;
        base.transform.localScale = new Vector3(num3, num3, num3);
    }
}
