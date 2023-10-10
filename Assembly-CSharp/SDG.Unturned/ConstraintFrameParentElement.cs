using UnityEngine.UIElements;

namespace SDG.Unturned;

internal class ConstraintFrameParentElement : VisualElement
{
    public VisualElement _contentContainerOverride;

    public override VisualElement contentContainer => _contentContainerOverride;
}
