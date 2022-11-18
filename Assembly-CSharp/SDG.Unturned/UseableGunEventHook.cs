using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Useable Gun Event Hook")]
public class UseableGunEventHook : MonoBehaviour
{
    public UnityEvent OnShotFired;

    public UnityEvent OnReloadingStarted;

    public UnityEvent OnChamberingStarted;

    public UnityEvent OnAimingStarted;

    public UnityEvent OnAimingStopped;
}
