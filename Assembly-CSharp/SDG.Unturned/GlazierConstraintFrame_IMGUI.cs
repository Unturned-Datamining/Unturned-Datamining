using UnityEngine;

namespace SDG.Unturned;

internal class GlazierConstraintFrame_IMGUI : GlazierElementBase_IMGUI, ISleekConstraintFrame, ISleekElement
{
    private ESleekConstraint _constraint;

    private float _aspectRatio = 1f;

    public ESleekConstraint Constraint
    {
        get
        {
            return _constraint;
        }
        set
        {
            _constraint = value;
            isTransformDirty = true;
        }
    }

    public float AspectRatio
    {
        get
        {
            return _aspectRatio;
        }
        set
        {
            _aspectRatio = value;
            isTransformDirty = true;
        }
    }

    protected override Rect CalculateDrawRect()
    {
        Rect result = base.CalculateDrawRect();
        if (Constraint == ESleekConstraint.FitInParent)
        {
            if (result.width < result.height * _aspectRatio)
            {
                float num = result.width / _aspectRatio;
                result.y += (result.height - num) * 0.5f;
                result.height = num;
            }
            else
            {
                float num2 = result.height * _aspectRatio;
                result.x += (result.width - num2) * 0.5f;
                result.width = num2;
            }
        }
        return result;
    }
}
