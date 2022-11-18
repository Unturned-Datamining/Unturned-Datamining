using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Collision Event Hook")]
public class CollisionEventHook : MonoBehaviour
{
    public UnityEvent OnPlayerEnter;

    public UnityEvent OnPlayerExit;

    public UnityEvent OnFirstPlayerEnter;

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
            numOverlappingPlayers++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!(other == null) && other.CompareTag("Player"))
        {
            OnPlayerExit.Invoke();
            numOverlappingPlayers--;
        }
    }
}
