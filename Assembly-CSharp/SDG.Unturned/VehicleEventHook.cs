using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Vehicle Event Hook")]
public class VehicleEventHook : MonoBehaviour
{
    public UnityEvent OnDriverAdded;

    public UnityEvent OnDriverRemoved;

    public UnityEvent OnLocalDriverAdded;

    public UnityEvent OnLocalDriverRemoved;

    public UnityEvent OnLocalPassengerAdded;

    public UnityEvent OnLocalPassengerRemoved;

    public UnityEvent OnLocked;

    public UnityEvent OnUnlocked;

    public UnityEvent OnHornUsed;
}
