using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Binary Random")]
public class BinaryRandomComponent : MonoBehaviour
{
    public bool AuthorityOnly;

    [Range(0f, 1f)]
    public float DefaultProbability;

    public UnityEvent OnTrue;

    public UnityEvent OnFalse;

    public void TriggerDefault()
    {
        TriggerWithProbability(DefaultProbability);
    }

    public void TriggerWithProbability(float Probability)
    {
        if (!AuthorityOnly || Provider.isServer)
        {
            if (Random.value < Probability)
            {
                OnTrue.Invoke();
            }
            else
            {
                OnFalse.Invoke();
            }
        }
    }
}
