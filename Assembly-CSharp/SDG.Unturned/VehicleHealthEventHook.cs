using UnityEngine;
using UnityEngine.Events;
using Unturned.UnityEx;

namespace SDG.Unturned;

/// <summary>
/// Can be added to descendants of Vehicle GameObject to receive events.
/// </summary>
[AddComponentMenu("Unturned/Vehicle Health Event Hook")]
public class VehicleHealthEventHook : MonoBehaviour
{
    [Tooltip("Health threshold to compare against.")]
    public int TargetHealth;

    [Tooltip("How to compare current health against TargetHealth. For example: is health < target health?")]
    public ENPCLogicType ComparisonType;

    [Tooltip("Invoked when vehicle health passes comparison with target health.")]
    public UnityEvent OnConditionMet;

    [Tooltip("Invoked when vehicle health no longer passes comparison with target health.")]
    public UnityEvent OnConditionUnmet;

    [Tooltip("If true, this event is only invoked on the server/singleplayer.")]
    public bool AuthorityOnly;

    [Tooltip("If true, OnConditionMet/Unmet is invoked when vehicle first loads.")]
    public bool InvokeWhenInitialized;

    private InteractableVehicle vehicle;

    private bool wasConditionMet;

    protected void Start()
    {
        if (AuthorityOnly && !Provider.isServer)
        {
            return;
        }
        vehicle = DamageTool.getVehicle(base.transform);
        if (vehicle != null)
        {
            vehicle.OnHealthChanged += OnHealthChanged;
            wasConditionMet = NPCTool.doesLogicPass(ComparisonType, vehicle.health, TargetHealth);
            if (InvokeWhenInitialized)
            {
                if (wasConditionMet)
                {
                    OnConditionMet.TryInvoke(this);
                }
                else
                {
                    OnConditionUnmet.TryInvoke(this);
                }
            }
        }
        else
        {
            UnturnedLog.warn("VehicleHealthEventHook unable to find vehicle at " + this.GetSceneHierarchyPath());
        }
    }

    protected void OnDestroy()
    {
        if (vehicle != null)
        {
            vehicle.OnHealthChanged -= OnHealthChanged;
            vehicle = null;
        }
    }

    private void OnHealthChanged(InteractableVehicle vehicle)
    {
        bool flag = NPCTool.doesLogicPass(ComparisonType, vehicle.health, TargetHealth);
        if (flag != wasConditionMet)
        {
            wasConditionMet = flag;
            if (wasConditionMet)
            {
                OnConditionMet.TryInvoke(this);
            }
            else
            {
                OnConditionUnmet.TryInvoke(this);
            }
        }
    }
}
