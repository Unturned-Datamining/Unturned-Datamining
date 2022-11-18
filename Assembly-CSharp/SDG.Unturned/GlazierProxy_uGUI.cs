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

    public override void destroy()
    {
        owner.OnDestroy();
        base.destroy();
    }

    protected override bool ReleaseIntoPool()
    {
        PoolData poolData = new PoolData();
        PopulateBasePoolData(poolData);
        base.glazier.ReleaseEmptyToPool(poolData);
        return true;
    }
}
