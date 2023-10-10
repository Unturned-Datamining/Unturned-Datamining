using UnityEngine.UIElements;

namespace SDG.Unturned;

internal class GlazierProxy_UIToolkit : GlazierElementBase_UIToolkit
{
    private SleekWrapper owner;

    public GlazierProxy_UIToolkit(Glazier_UIToolkit glazier, SleekWrapper owner)
        : base(glazier)
    {
        this.owner = owner;
        visualElement = new VisualElement();
        visualElement.userData = this;
        visualElement.AddToClassList("unturned-empty");
        visualElement.pickingMode = PickingMode.Ignore;
        visualElement.name = owner.GetType().Name;
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
