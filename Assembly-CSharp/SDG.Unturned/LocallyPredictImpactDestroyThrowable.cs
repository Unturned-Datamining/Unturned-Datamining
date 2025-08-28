using UnityEngine;

namespace SDG.Unturned;

internal class LocallyPredictImpactDestroyThrowable : MonoBehaviour, IExplodableThrowable
{
    public void Explode()
    {
        Object.Destroy(base.gameObject);
    }
}
