using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned;

internal class ConstraintFrameChildElement : VisualElement
{
    public ESleekConstraint constraint;

    public float aspectRatio = 1f;

    public void OnParentGeometryChanged(GeometryChangedEvent geometryChangedEvent)
    {
        if (constraint == ESleekConstraint.NONE)
        {
            base.style.left = 0f;
            base.style.right = 0f;
            base.style.top = 0f;
            base.style.bottom = 0f;
            return;
        }
        Rect newRect = geometryChangedEvent.newRect;
        if (newRect.width < newRect.height * aspectRatio)
        {
            base.style.left = 0f;
            base.style.right = 0f;
            float num = newRect.width / aspectRatio / newRect.height;
            StyleLength styleLength = Length.Percent((1f - num) * 50f);
            base.style.top = styleLength;
            base.style.bottom = styleLength;
        }
        else
        {
            float num2 = newRect.height * aspectRatio / newRect.width;
            StyleLength styleLength2 = Length.Percent((1f - num2) * 50f);
            base.style.left = styleLength2;
            base.style.right = styleLength2;
            base.style.top = 0f;
            base.style.bottom = 0f;
        }
    }
}
