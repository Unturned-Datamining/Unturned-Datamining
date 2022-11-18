using UnityEngine;

namespace SDG.Unturned;

public class StickyGrenade : TriggerGrenadeBase
{
    protected override void GrenadeTriggered()
    {
        base.GrenadeTriggered();
        Rigidbody component = GetComponent<Rigidbody>();
        component.useGravity = false;
        component.isKinematic = true;
    }
}
