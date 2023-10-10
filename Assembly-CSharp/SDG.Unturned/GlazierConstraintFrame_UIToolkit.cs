using System;
using UnityEngine.UIElements;

namespace SDG.Unturned;

internal class GlazierConstraintFrame_UIToolkit : GlazierElementBase_UIToolkit, ISleekConstraintFrame, ISleekElement
{
    private ConstraintFrameParentElement containerElement;

    private ConstraintFrameChildElement contentElement;

    public ESleekConstraint Constraint
    {
        get
        {
            return contentElement.constraint;
        }
        set
        {
            if (contentElement.constraint != 0)
            {
                throw new NotSupportedException();
            }
            contentElement.constraint = value;
        }
    }

    public float AspectRatio
    {
        get
        {
            return contentElement.aspectRatio;
        }
        set
        {
            contentElement.aspectRatio = value;
        }
    }

    public GlazierConstraintFrame_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
        containerElement = new ConstraintFrameParentElement();
        containerElement.pickingMode = PickingMode.Ignore;
        containerElement.userData = this;
        containerElement.AddToClassList("unturned-constraint-frame-container");
        containerElement._contentContainerOverride = containerElement;
        contentElement = new ConstraintFrameChildElement();
        contentElement.pickingMode = PickingMode.Ignore;
        contentElement.userData = this;
        contentElement.AddToClassList("unturned-constraint-frame-content");
        containerElement.Add(contentElement);
        containerElement.RegisterCallback<GeometryChangedEvent>(contentElement.OnParentGeometryChanged);
        containerElement._contentContainerOverride = contentElement;
        visualElement = containerElement;
    }
}
