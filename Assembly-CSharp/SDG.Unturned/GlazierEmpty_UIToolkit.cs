using UnityEngine.UIElements;

namespace SDG.Unturned;

internal class GlazierEmpty_UIToolkit : GlazierElementBase_UIToolkit
{
    public GlazierEmpty_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
        visualElement = new VisualElement();
        visualElement.userData = this;
        visualElement.pickingMode = PickingMode.Ignore;
        visualElement.AddToClassList("unturned-empty");
    }
}
