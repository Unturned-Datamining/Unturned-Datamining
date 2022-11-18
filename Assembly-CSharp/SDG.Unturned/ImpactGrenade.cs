namespace SDG.Unturned;

public class ImpactGrenade : TriggerGrenadeBase
{
    public IExplodableThrowable explodable;

    protected override void GrenadeTriggered()
    {
        base.GrenadeTriggered();
        if (explodable == null)
        {
            UnturnedLog.warn("Missing explodable", this);
        }
        else
        {
            explodable.Explode();
        }
    }
}
