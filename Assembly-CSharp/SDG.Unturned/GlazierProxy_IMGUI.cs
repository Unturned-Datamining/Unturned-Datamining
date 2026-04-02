namespace SDG.Unturned;

internal class GlazierProxy_IMGUI : GlazierElementBase_IMGUI
{
    private SleekWrapper owner;

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
