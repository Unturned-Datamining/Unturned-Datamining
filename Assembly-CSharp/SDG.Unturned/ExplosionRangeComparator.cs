using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class ExplosionRangeComparator : IComparer<Transform>
{
    public Vector3 point;

    public int Compare(Transform a, Transform b)
    {
        return Mathf.RoundToInt(((a.position - point).sqrMagnitude - (b.position - point).sqrMagnitude) * 100f);
    }
}
