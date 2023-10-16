using System.Diagnostics;
using UnityEngine;

namespace SDG.Unturned;

public abstract class GlazierElementBase : ISleekElement
{
    protected bool _isVisible = true;

    private float fromPositionOffset_X;

    private float fromPositionOffset_Y;

    private float toPositionOffset_X;

    private float toPositionOffset_Y;

    private float fromPositionScale_X;

    private float fromPositionScale_Y;

    private float toPositionScale_X;

    private float toPositionScale_Y;

    private float fromSizeOffset_X;

    private float fromSizeOffset_Y;

    private float toSizeOffset_X;

    private float toSizeOffset_Y;

    private float fromSizeScale_X;

    private float fromSizeScale_Y;

    private float toSizeScale_X;

    private float toSizeScale_Y;

    private ESleekLerp positionOffsetLerpMethod;

    private float positionOffsetLerpTime;

    private float positionOffsetLerpValue;

    private bool isAnimatingPositionOffset;

    private ESleekLerp positionScaleLerpMethod;

    private float positionScaleLerpTime;

    private float positionScaleLerpValue;

    private bool isAnimatingPositionScale;

    private ESleekLerp sizeOffsetLerpMethod;

    private float sizeOffsetLerpTime;

    private float sizeOffsetLerpValue;

    private bool isAnimatingSizeOffset;

    private ESleekLerp sizeScaleLerpMethod;

    private float sizeScaleLerpTime;

    private float sizeScaleLerpValue;

    private bool isAnimatingSizeScale;

    public bool isTransformDirty;

    private float _positionOffset_X;

    private float _positionOffset_Y;

    private float _positionScale_X;

    private float _positionScale_Y;

    private float _sizeOffset_X;

    private float _sizeOffset_Y;

    private float _sizeScale_X;

    private float _sizeScale_Y;

    protected bool _useManualLayout = true;

    protected bool _useWidthLayoutOverride;

    protected bool _useHeightLayoutOverride;

    protected ESleekChildLayout _useChildAutoLayout;

    protected ESleekChildPerpendicularAlignment _childPerpendicularAlignment;

    protected bool _expandChildren;

    protected bool _ignoreLayout;

    public virtual bool IsVisible
    {
        get
        {
            return _isVisible;
        }
        set
        {
            _isVisible = value;
        }
    }

    public abstract ISleekElement Parent { get; }

    public ISleekLabel SideLabel { get; private set; }

    public float PositionOffset_X
    {
        get
        {
            return _positionOffset_X;
        }
        set
        {
            _positionOffset_X = value;
            isTransformDirty = true;
        }
    }

    public float PositionOffset_Y
    {
        get
        {
            return _positionOffset_Y;
        }
        set
        {
            _positionOffset_Y = value;
            isTransformDirty = true;
        }
    }

    public float PositionScale_X
    {
        get
        {
            return _positionScale_X;
        }
        set
        {
            _positionScale_X = value;
            isTransformDirty = true;
        }
    }

    public float PositionScale_Y
    {
        get
        {
            return _positionScale_Y;
        }
        set
        {
            _positionScale_Y = value;
            isTransformDirty = true;
        }
    }

    public float SizeOffset_X
    {
        get
        {
            return _sizeOffset_X;
        }
        set
        {
            _sizeOffset_X = value;
            isTransformDirty = true;
        }
    }

    public float SizeOffset_Y
    {
        get
        {
            return _sizeOffset_Y;
        }
        set
        {
            _sizeOffset_Y = value;
            isTransformDirty = true;
        }
    }

    public float SizeScale_X
    {
        get
        {
            return _sizeScale_X;
        }
        set
        {
            _sizeScale_X = value;
            isTransformDirty = true;
        }
    }

    public float SizeScale_Y
    {
        get
        {
            return _sizeScale_Y;
        }
        set
        {
            _sizeScale_Y = value;
            isTransformDirty = true;
        }
    }

    public ISleekElement AttachmentRoot => this;

    public bool IsAnimatingTransform => isAnimatingPositionOffset | isAnimatingPositionScale | isAnimatingSizeOffset | isAnimatingSizeScale;

    public virtual bool UseManualLayout
    {
        get
        {
            return _useManualLayout;
        }
        set
        {
            _useManualLayout = value;
        }
    }

    public virtual bool UseWidthLayoutOverride
    {
        get
        {
            return _useWidthLayoutOverride;
        }
        set
        {
            _useWidthLayoutOverride = value;
        }
    }

    public virtual bool UseHeightLayoutOverride
    {
        get
        {
            return _useHeightLayoutOverride;
        }
        set
        {
            _useHeightLayoutOverride = value;
        }
    }

    public virtual ESleekChildLayout UseChildAutoLayout
    {
        get
        {
            return _useChildAutoLayout;
        }
        set
        {
            _useChildAutoLayout = value;
        }
    }

    public virtual ESleekChildPerpendicularAlignment ChildPerpendicularAlignment
    {
        get
        {
            return _childPerpendicularAlignment;
        }
        set
        {
            _childPerpendicularAlignment = value;
        }
    }

    public virtual bool ExpandChildren
    {
        get
        {
            return _expandChildren;
        }
        set
        {
            _expandChildren = value;
        }
    }

    public virtual bool IgnoreLayout
    {
        get
        {
            return _ignoreLayout;
        }
        set
        {
            _ignoreLayout = value;
        }
    }

    public abstract void InternalDestroy();

    public void AnimatePositionOffset(float newPositionOffset_X, float newPositionOffset_Y, ESleekLerp lerp, float time)
    {
        isAnimatingPositionOffset = true;
        positionOffsetLerpMethod = lerp;
        positionOffsetLerpTime = time;
        positionOffsetLerpValue = 0f;
        fromPositionOffset_X = PositionOffset_X;
        fromPositionOffset_Y = PositionOffset_Y;
        toPositionOffset_X = newPositionOffset_X;
        toPositionOffset_Y = newPositionOffset_Y;
    }

    public void AnimatePositionScale(float newPositionScale_X, float newPositionScale_Y, ESleekLerp lerp, float time)
    {
        isAnimatingPositionScale = true;
        positionScaleLerpMethod = lerp;
        positionScaleLerpTime = time;
        positionScaleLerpValue = 0f;
        fromPositionScale_X = PositionScale_X;
        fromPositionScale_Y = PositionScale_Y;
        toPositionScale_X = newPositionScale_X;
        toPositionScale_Y = newPositionScale_Y;
    }

    public void AnimateSizeOffset(float newSizeOffset_X, float newSizeOffset_Y, ESleekLerp lerp, float time)
    {
        isAnimatingSizeOffset = true;
        sizeOffsetLerpMethod = lerp;
        sizeOffsetLerpTime = time;
        sizeOffsetLerpValue = 0f;
        fromSizeOffset_X = SizeOffset_X;
        fromSizeOffset_Y = SizeOffset_Y;
        toSizeOffset_X = newSizeOffset_X;
        toSizeOffset_Y = newSizeOffset_Y;
    }

    public void AnimateSizeScale(float newSizeScale_X, float newSizeScale_Y, ESleekLerp lerp, float time)
    {
        isAnimatingSizeScale = true;
        sizeScaleLerpMethod = lerp;
        sizeScaleLerpTime = time;
        sizeScaleLerpValue = 0f;
        fromSizeScale_X = SizeScale_X;
        fromSizeScale_Y = SizeScale_Y;
        toSizeScale_X = newSizeScale_X;
        toSizeScale_Y = newSizeScale_Y;
    }

    public abstract void AddChild(ISleekElement child);

    public void AddLabel(string text, ESleekSide side)
    {
        AddLabel(text, Color.white, side);
    }

    public void AddLabel(string text, Color color, ESleekSide side)
    {
        SideLabel = Glazier.Get().CreateLabel();
        switch (side)
        {
        case ESleekSide.LEFT:
            SideLabel.PositionOffset_X = -205f;
            SideLabel.TextAlignment = TextAnchor.MiddleRight;
            break;
        case ESleekSide.RIGHT:
            SideLabel.PositionOffset_X = 5f;
            SideLabel.PositionScale_X = 1f;
            SideLabel.TextAlignment = TextAnchor.MiddleLeft;
            break;
        }
        SideLabel.PositionOffset_Y = -30f;
        SideLabel.PositionScale_Y = 0.5f;
        SideLabel.SizeOffset_X = 200f;
        SideLabel.SizeOffset_Y = 60f;
        if (color != Color.white)
        {
            SideLabel.TextColor = color;
        }
        SideLabel.Text = text;
        SideLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        AddChild(SideLabel);
    }

    public void UpdateLabel(string text)
    {
        SideLabel.Text = text;
    }

    public abstract int FindIndexOfChild(ISleekElement sleek);

    public abstract ISleekElement GetChildAtIndex(int index);

    public abstract void RemoveChild(ISleekElement child);

    public abstract void RemoveAllChildren();

    public virtual void Update()
    {
        if (IsAnimatingTransform)
        {
            UpdateAnimation();
        }
        if (isTransformDirty)
        {
            UpdateDirtyTransform();
        }
        UpdateChildren();
    }

    protected abstract void UpdateChildren();

    private float InterpValue(float value, ESleekLerp method, float time, float deltaTime)
    {
        return method switch
        {
            ESleekLerp.LINEAR => value + deltaTime / time, 
            ESleekLerp.EXPONENTIAL => value + (1f - value) * time * deltaTime, 
            _ => value, 
        };
    }

    private void UpdateAnimation()
    {
        float unscaledDeltaTime = Time.unscaledDeltaTime;
        if (isAnimatingPositionOffset)
        {
            if (positionOffsetLerpValue >= 0.999f)
            {
                isAnimatingPositionOffset = false;
                PositionOffset_X = toPositionOffset_X;
                PositionOffset_Y = toPositionOffset_Y;
            }
            else
            {
                PositionOffset_X = Mathf.Lerp(fromPositionOffset_X, toPositionOffset_X, positionOffsetLerpValue);
                PositionOffset_Y = Mathf.Lerp(fromPositionOffset_Y, toPositionOffset_Y, positionOffsetLerpValue);
            }
            positionOffsetLerpValue = InterpValue(positionOffsetLerpValue, positionOffsetLerpMethod, positionOffsetLerpTime, unscaledDeltaTime);
        }
        if (isAnimatingPositionScale)
        {
            if (positionScaleLerpValue >= 0.999f)
            {
                isAnimatingPositionScale = false;
                PositionScale_X = toPositionScale_X;
                PositionScale_Y = toPositionScale_Y;
            }
            else
            {
                PositionScale_X = Mathf.Lerp(fromPositionScale_X, toPositionScale_X, positionScaleLerpValue);
                PositionScale_Y = Mathf.Lerp(fromPositionScale_Y, toPositionScale_Y, positionScaleLerpValue);
            }
            positionScaleLerpValue = InterpValue(positionScaleLerpValue, positionScaleLerpMethod, positionScaleLerpTime, unscaledDeltaTime);
        }
        if (isAnimatingSizeOffset)
        {
            if (sizeOffsetLerpValue >= 0.999f)
            {
                isAnimatingSizeOffset = false;
                SizeOffset_X = toSizeOffset_X;
                SizeOffset_Y = toSizeOffset_Y;
            }
            else
            {
                SizeOffset_X = Mathf.Lerp(fromSizeOffset_X, toSizeOffset_X, sizeOffsetLerpValue);
                SizeOffset_Y = Mathf.Lerp(fromSizeOffset_Y, toSizeOffset_Y, sizeOffsetLerpValue);
            }
            sizeOffsetLerpValue = InterpValue(sizeOffsetLerpValue, sizeOffsetLerpMethod, sizeOffsetLerpTime, unscaledDeltaTime);
        }
        if (isAnimatingSizeScale)
        {
            if (sizeScaleLerpValue >= 0.999f)
            {
                isAnimatingSizeScale = false;
                SizeScale_X = toSizeScale_X;
                SizeScale_Y = toSizeScale_Y;
            }
            else
            {
                SizeScale_X = Mathf.Lerp(fromSizeScale_X, toSizeScale_X, sizeScaleLerpValue);
                SizeScale_Y = Mathf.Lerp(fromSizeScale_Y, toSizeScale_Y, sizeScaleLerpValue);
            }
            sizeScaleLerpValue = InterpValue(sizeScaleLerpValue, sizeScaleLerpMethod, sizeScaleLerpTime, unscaledDeltaTime);
        }
    }

    public abstract Vector2 ViewportToNormalizedPosition(Vector2 viewportPosition);

    public abstract Vector2 GetNormalizedCursorPosition();

    public abstract Vector2 GetAbsoluteSize();

    public abstract void SetAsFirstSibling();

    protected abstract void UpdateDirtyTransform();

    [Conditional("VALIDATE_GLAZIER_USE_AFTER_DESTROY")]
    public void ValidateNotDestroyed()
    {
    }

    public GlazierElementBase()
    {
        isAnimatingPositionOffset = false;
        isAnimatingPositionScale = false;
        isAnimatingSizeOffset = false;
        isAnimatingSizeScale = false;
        isTransformDirty = true;
        SideLabel = null;
        _positionOffset_X = 0f;
        _positionOffset_Y = 0f;
        _positionScale_X = 0f;
        _positionScale_Y = 0f;
        _sizeOffset_X = 0f;
        _sizeOffset_Y = 0f;
        _sizeScale_X = 0f;
        _sizeScale_Y = 0f;
    }
}
