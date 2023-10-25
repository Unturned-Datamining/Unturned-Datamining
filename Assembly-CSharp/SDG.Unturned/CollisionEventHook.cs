using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned;

/// <summary>
/// Can be added to any GameObject with a Trigger to receive events.
/// Ensure that Layer will detect player overlaps. Trap is a good candidate.
/// </summary>
[AddComponentMenu("Unturned/Collision Event Hook")]
public class CollisionEventHook : MonoBehaviour
{
    /// <summary>
    /// Invoked when a player enters the trigger.
    /// Called before OnFirstPlayerEnter.
    /// </summary>
    public UnityEvent OnPlayerEnter;

    /// <summary>
    /// Invoked when a player exits the trigger.
    /// Called before OnAllPlayersExit.
    /// </summary>
    public UnityEvent OnPlayerExit;

    /// <summary>
    /// Invoked when first player enters the trigger, and not again until all players have left.
    /// Called after OnPlayerEnter.
    /// </summary>
    public UnityEvent OnFirstPlayerEnter;

    /// <summary>
    /// Invoked when last player exits the trigger.
    /// Called after OnPlayerExit.
    /// </summary>
    public UnityEvent OnAllPlayersExit;

    private int _numOverlappingPlayers;

    private int numOverlappingPlayers
    {
        get
        {
            return _numOverlappingPlayers;
        }
        set
        {
            value = Mathf.Max(0, value);
            if (_numOverlappingPlayers != value)
            {
                _numOverlappingPlayers = value;
                switch (value)
                {
                case 0:
                    OnAllPlayersExit.Invoke();
                    break;
                case 1:
                    OnFirstPlayerEnter.Invoke();
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!(other == null) && other.CompareTag("Player"))
        {
            OnPlayerEnter.Invoke();
            int num = numOverlappingPlayers + 1;
            numOverlappingPlayers = num;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!(other == null) && other.CompareTag("Player"))
        {
            OnPlayerExit.Invoke();
            int num = numOverlappingPlayers - 1;
            numOverlappingPlayers = num;
        }
    }
}
