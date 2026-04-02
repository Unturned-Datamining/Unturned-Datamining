namespace SDG.Unturned;

internal class GlazierProxy_IMGUI : GlazierElementBase_IMGUI, ISleekProxyImplementation, ISleekElement
{
    private SleekWrapper owner;

    public SleekWrapper GetWrapper()
    {
        return owner;
    }

    public GlazierProxy_IMGUI(SleekWrapper owner)
    {
        this.owner = owner;
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
}
