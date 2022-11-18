using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Vehicle Turret Event Hook")]
public class VehicleTurretEventHook : MonoBehaviour
{
    public UnityEvent OnShotFired;

    public UnityEvent OnReloadingStarted;

    public UnityEvent OnChamberingStarted;

    public UnityEvent OnAimingStarted;

    public UnityEvent OnAimingStopped;

    public UnityEvent OnAimingStarted_Local;

    public UnityEvent OnAimingStopped_Local;

    public UnityEvent OnInspectingAttachmentsStarted_Local;

    public UnityEvent OnInspectingAttachmentsStopped_Local;

    public UnityEvent OnPassengerAdded;

    public UnityEvent OnPassengerRemoved;

    public UnityEvent OnLocalPassengerAdded;

    public UnityEvent OnLocalPassengerRemoved;
}
