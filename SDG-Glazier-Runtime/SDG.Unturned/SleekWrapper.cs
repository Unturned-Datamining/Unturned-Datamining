using System.Diagnostics;
using UnityEngine;

namespace SDG.Unturned;

public class SleekWrapper : ISleekElement
{
    private ISleekElement implementation;

    public bool IsVisible
    {
        get
        {
            return implementation.IsVisible;
        }
        set
        {
            implementation.IsVisible = value;
        }
    }

    public ISleekElement Parent => implementation.Parent;

    public ISleekLabel SideLabel => implementation.SideLabel;

    public float PositionOffset_X
    {
        get
        {
            return implementation.PositionOffset_X;
        }
        set
        {
            implementation.PositionOffset_X = value;
        }
    }

    public float PositionOffset_Y
    {
        get
        {
            return implementation.PositionOffset_Y;
        }
        set
        {
            implementation.PositionOffset_Y = value;
        }
    }

    public float PositionScale_X
    {
        get
        {
            return implementation.PositionScale_X;
        }
        set
        {
            implementation.PositionScale_X = value;
        }
    }

    public float PositionScale_Y
    {
        get
        {
            return implementation.PositionScale_Y;
        }
        set
        {
            implementation.PositionScale_Y = value;
        }
    }

    public float SizeOffset_X
    {
        get
        {
            return implementation.SizeOffset_X;
        }
        set
        {
            implementation.SizeOffset_X = value;
        }
    }

    public float SizeOffset_Y
    {
        get
        {
            return implementation.SizeOffset_Y;
        }
        set
        {
            implementation.SizeOffset_Y = value;
        }
    }

    public float SizeScale_X
    {
        get
        {
            return implementation.SizeScale_X;
        }
        set
        {
            implementation.SizeScale_X = value;
        }
    }

    public float SizeScale_Y
    {
        get
        {
            return implementation.SizeScale_Y;
        }
        set
        {
            implementation.SizeScale_Y = value;
        }
    }

    public ISleekElement AttachmentRoot => implementation;

    public bool IsAnimatingTransform => implementation.IsAnimatingTransform;

    public virtual bool UseManualLayout
    {
        get
        {
            return implementation.UseManualLayout;
        }
        set
        {
            implementation.UseManualLayout = value;
        }
    }

    public bool UseWidthLayoutOverride
    {
        get
        {
            return implementation.UseWidthLayoutOverride;
        }
        set
        {
            implementation.UseWidthLayoutOverride = value;
        }
    }

    public bool UseHeightLayoutOverride
    {
        get
        {
            return implementation.UseHeightLayoutOverride;
        }
        set
        {
            implementation.UseHeightLayoutOverride = value;
        }
    }

    public ESleekChildLayout UseChildAutoLayout
    {
        get
        {
            return implementation.UseChildAutoLayout;
        }
        set
        {
            implementation.UseChildAutoLayout = value;
        }
    }

    public ESleekChildPerpendicularAlignment ChildPerpendicularAlignment
    {
        get
        {
            return implementation.ChildPerpendicularAlignment;
        }
        set
        {
            implementation.ChildPerpendicularAlignment = value;
        }
    }

    public bool ExpandChildren
    {
        get
        {
            return implementation.ExpandChildren;
        }
        set
        {
            implementation.ExpandChildren = value;
        }
    }

    public bool IgnoreLayout
    {
        get
        {
            return implementation.IgnoreLayout;
        }
        set
        {
            implementation.IgnoreLayout = value;
        }
    }

    public float ChildAutoLayoutPadding
    {
        get
        {
            return implementation.ChildAutoLayoutPadding;
        }
        set
        {
            implementation.ChildAutoLayoutPadding = value;
        }
    }

    public void InternalDestroy()
    {
        implementation.InternalDestroy();
    }

    public void AnimatePositionOffset(float newPositionOffset_X, float newPositionOffset_Y, ESleekLerp lerp, float time)
    {
        implementation.AnimatePositionOffset(newPositionOffset_X, newPositionOffset_Y, lerp, time);
    }

    public void AnimatePositionScale(float newPositionScale_X, float newPositionScale_Y, ESleekLerp lerp, float time)
    {
        implementation.AnimatePositionScale(newPositionScale_X, newPositionScale_Y, lerp, time);
    }

    public void AnimateSizeOffset(float newSizeOffset_X, float newSizeOffset_Y, ESleekLerp lerp, float time)
    {
        implementation.AnimateSizeOffset(newSizeOffset_X, newSizeOffset_Y, lerp, time);
    }

    public void AnimateSizeScale(float newSizeScale_X, float newSizeScale_Y, ESleekLerp lerp, float time)
    {
        implementation.AnimateSizeScale(newSizeScale_X, newSizeScale_Y, lerp, time);
    }

    public void AddChild(ISleekElement sleek)
    {
        implementation.AddChild(sleek);
    }

    public void AddLabel(string text, ESleekSide side)
    {
        implementation.AddLabel(text, side);
    }

    public void AddLabel(string text, Color color, ESleekSide side)
    {
        implementation.AddLabel(text, color, side);
    }

    public void UpdateLabel(string text)
    {
        implementation.UpdateLabel(text);
    }

    public int FindIndexOfChild(ISleekElement sleek)
    {
        return implementation.FindIndexOfChild(sleek);
    }

    public ISleekElement GetChildAtIndex(int index)
    {
        return implementation.GetChildAtIndex(index);
    }

    public void Update()
    {
        implementation.Update();
    }

    public void RemoveChild(ISleekElement sleek)
    {
        implementation.RemoveChild(sleek);
    }

    public void RemoveAllChildren()
    {
        implementation.RemoveAllChildren();
    }

    public Vector2 ViewportToNormalizedPosition(Vector2 viewportPosition)
    {
        return implementation.ViewportToNormalizedPosition(viewportPosition);
    }

    public Vector2 GetNormalizedCursorPosition()
    {
        return implementation.GetNormalizedCursorPosition();
    }

    public Vector2 GetAbsoluteSize()
    {
        return implementation.GetAbsoluteSize();
    }

    public void SetAsFirstSibling()
    {
        implementation.SetAsFirstSibling();
    }

    public virtual void OnUpdate()
    {
    }

    public virtual void OnDestroy()
    {
    }

    [Conditional("VALIDATE_SLEEK_PROXY_USE_AFTER_DESTROY")]
    public void ValidateNotDestroyed()
    {
    }

    public SleekWrapper()
    {
        implementation = Glazier.Get().CreateProxyImplementation(this);
    }
}
