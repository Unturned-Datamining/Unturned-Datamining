using UnityEngine;
using UnityEngine.Events;
using Unturned.UnityEx;

namespace SDG.Unturned;

/// <summary>
/// Can be added to descendants of Vehicle GameObject to receive events.
/// </summary>
[AddComponentMenu("Unturned/Vehicle Gearshift Event Hook")]
public class VehicleGearshiftEventHook : MonoBehaviour
{
    [Tooltip("Gear to monitor. -1 is reverse, 0 is neutral, +X is forward.")]
    public int GearNumber;

    [Tooltip("Invoked when vehicle shifts to this gear.")]
    public UnityEvent OnGearEntered;

    [Tooltip("Invoked when vehicle shifts away from this gear.")]
    public UnityEvent OnGearExited;

    [Tooltip("If true, this event is only invoked on the server/singleplayer.")]
    public bool AuthorityOnly;

    [Tooltip("If true, OnGearEntered/OnGearExited is invoked when vehicle first loads.")]
    public bool InvokeWhenInitialized;

    private InteractableVehicle vehicle;

    private bool wasInGear;

    protected void Start()
    {
        if (AuthorityOnly && !Provider.isServer)
        {
            return;
        }
        vehicle = DamageTool.getVehicle(base.transform);
        if (vehicle != null)
        {
            vehicle.OnGearChanged += OnGearChanged;
            wasInGear = vehicle.GearNumber == GearNumber;
            if (InvokeWhenInitialized)
            {
                if (wasInGear)
                {
                    OnGearEntered.TryInvoke(this);
                }
                else
                {
                    OnGearExited.TryInvoke(this);
                }
            }
        }
        else
        {
            UnturnedLog.warn("VehicleGearshiftEventHook unable to find vehicle at " + this.GetSceneHierarchyPath());
        }
    }

    protected void OnDestroy()
    {
        if (vehicle != null)
        {
            vehicle.OnGearChanged -= OnGearChanged;
            vehicle = null;
        }
    }

    private void OnGearChanged(InteractableVehicle vehicle)
    {
        bool flag = vehicle.GearNumber == GearNumber;
        if (flag != wasInGear)
        {
            wasInGear = flag;
            if (wasInGear)
            {
                OnGearEntered.TryInvoke(this);
            }
            else
            {
                OnGearExited.TryInvoke(this);
            }
        }
    }
}
