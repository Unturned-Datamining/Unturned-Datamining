using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Activation Event Hook")]
public class ActivationEventHook : MonoBehaviour
{
    public UnityEvent OnEnabled;

    public UnityEvent OnDisabled;

    private void OnEnable()
    {
        OnEnabled.Invoke();
    }

    private void OnDisable()
    {
        OnDisabled.Invoke();
    }
}
