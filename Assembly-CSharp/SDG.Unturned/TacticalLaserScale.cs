using System;
using UnityEngine;

namespace SDG.Unturned;

public class TacticalLaserScale : MonoBehaviour
{
    public float scaleMultiplier = 0.1f;

    public AnimationCurve scalingCurve;

    public void OnWillRenderObject()
    {
        Camera current = Camera.current;
        float magnitude = (base.transform.position - current.transform.position).magnitude;
        float num = current.fieldOfView * 0.5f;
        float num2 = Mathf.Tan((float)Math.PI / 180f * num);
        float num3 = scalingCurve.Evaluate(magnitude * num2) * scaleMultiplier;
        base.transform.localScale = new Vector3(num3, num3, num3);
    }
}
