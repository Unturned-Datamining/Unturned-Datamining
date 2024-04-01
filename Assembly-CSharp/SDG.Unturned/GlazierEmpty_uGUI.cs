namespace SDG.Unturned;

internal class GlazierEmpty_uGUI : GlazierElementBase_uGUI
{
    public GlazierEmpty_uGUI(Glazier_uGUI glazier)
        : base(glazier)
    {
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
