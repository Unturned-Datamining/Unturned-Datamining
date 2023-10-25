using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned;

/// <summary>
/// Single percentage randomness with two outcomes.
/// </summary>
[AddComponentMenu("Unturned/Binary Random")]
public class BinaryRandomComponent : MonoBehaviour
{
    /// <summary>
    /// If true the event will only be invoked in offline mode and on the server.
    /// </summary>
    public bool AuthorityOnly;

    /// <summary>
    /// Percentage chance of event occurring.
    /// </summary>
    [Range(0f, 1f)]
    public float DefaultProbability;

    /// <summary>
    /// Invoked when random event occurs.
    /// </summary>
    public UnityEvent OnTrue;

    /// <summary>
    /// Invoked when random event does NOT occur.
    /// </summary>
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
