namespace SDG.Unturned;

internal class GlazierProxy_uGUI : GlazierElementBase_uGUI
{
    private SleekWrapper owner;

    public GlazierProxy_uGUI(Glazier_uGUI glazier)
        : base(glazier)
    {
    }

    public void InitOwner(SleekWrapper owner)
    {
        this.owner = owner;
        base.gameObject.name = owner.GetType().Name;
    }

    public override void Update()
    {
        owner.OnUpdate();
        base.Update();
    }

    public override void InternalDestroy()
    {
        owner.OnDestroy();
        base.InternalDestroy();
    }

    protected override bool ReleaseIntoPool()
    {
        if (base.transform == null || base.gameObject == null)
        {
            return false;
        }
        PoolData poolData = new PoolData();
        PopulateBasePoolData(poolData);
        base.glazier.ReleaseEmptyToPool(poolData);
        return true;
    }
}
