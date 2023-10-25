using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned;

/// <summary>
/// Can be added to EquipablePrefab item GameObject to receive events.
/// </summary>
[AddComponentMenu("Unturned/Useable Gun Event Hook")]
public class UseableGunEventHook : MonoBehaviour
{
    /// <summary>
    /// Invoked when gun is fired.
    /// </summary>
    public UnityEvent OnShotFired;

    /// <summary>
    /// Invoked when gun begins reload sequence.
    /// </summary>
    public UnityEvent OnReloadingStarted;

    /// <summary>
    /// Invoked when gun begins hammer sequence.
    /// </summary>
    public UnityEvent OnChamberingStarted;

    /// <summary>
    /// Invoked when gun begins aiming.
    /// </summary>
    public UnityEvent OnAimingStarted;

    /// <summary>
    /// Invoked when gun ends aiming.
    /// </summary>
    public UnityEvent OnAimingStopped;
}
