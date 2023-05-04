using System.Diagnostics;
using UnityEngine;

namespace SDG.Unturned;

public abstract class GlazierElementBase : ISleekElement
{
    protected bool _isVisible = true;

    private int fromPositionOffset_X;

    private int fromPositionOffset_Y;

    private int toPositionOffset_X;

    private int toPositionOffset_Y;

    private float fromPositionScale_X;

    private float fromPositionScale_Y;

    private float toPositionScale_X;

    private float toPositionScale_Y;

    private int fromSizeOffset_X;

    private int fromSizeOffset_Y;

    private int toSizeOffset_X;

    private int toSizeOffset_Y;

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

    private int _positionOffset_X;

    private int _positionOffset_Y;

    private float _positionScale_X;

    private float _positionScale_Y;

    private int _sizeOffset_X;

    private int _sizeOffset_Y;

    private float _sizeScale_X;

    private float _sizeScale_Y;

    public virtual bool isVisible
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

    public abstract ISleekElement parent { get; }

    public ISleekLabel sideLabel { get; private set; }

    public int positionOffset_X
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

    public int positionOffset_Y
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

    public float positionScale_X
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

    public float positionScale_Y
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

    public int sizeOffset_X
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

    public int sizeOffset_Y
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

    public float sizeScale_X
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

    public float sizeScale_Y
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

    public ISleekElement attachmentRoot => this;

    public bool isAnimatingTransform => isAnimatingPositionOffset | isAnimatingPositionScale | isAnimatingSizeOffset | isAnimatingSizeScale;

    public abstract void destroy();

    public void lerpPositionOffset(int newPositionOffset_X, int newPositionOffset_Y, ESleekLerp lerp, float time)
    {
        isAnimatingPositionOffset = true;
        positionOffsetLerpMethod = lerp;
        positionOffsetLerpTime = time;
        positionOffsetLerpValue = 0f;
        fromPositionOffset_X = positionOffset_X;
        fromPositionOffset_Y = positionOffset_Y;
        toPositionOffset_X = newPositionOffset_X;
        toPositionOffset_Y = newPositionOffset_Y;
    }

    public void lerpPositionScale(float newPositionScale_X, float newPositionScale_Y, ESleekLerp lerp, float time)
    {
        isAnimatingPositionScale = true;
        positionScaleLerpMethod = lerp;
        positionScaleLerpTime = time;
        positionScaleLerpValue = 0f;
        fromPositionScale_X = positionScale_X;
        fromPositionScale_Y = positionScale_Y;
        toPositionScale_X = newPositionScale_X;
        toPositionScale_Y = newPositionScale_Y;
    }

    public void lerpSizeOffset(int newSizeOffset_X, int newSizeOffset_Y, ESleekLerp lerp, float time)
    {
        isAnimatingSizeOffset = true;
        sizeOffsetLerpMethod = lerp;
        sizeOffsetLerpTime = time;
        sizeOffsetLerpValue = 0f;
        fromSizeOffset_X = sizeOffset_X;
        fromSizeOffset_Y = sizeOffset_Y;
        toSizeOffset_X = newSizeOffset_X;
        toSizeOffset_Y = newSizeOffset_Y;
    }

    public void lerpSizeScale(float newSizeScale_X, float newSizeScale_Y, ESleekLerp lerp, float time)
    {
        isAnimatingSizeScale = true;
        sizeScaleLerpMethod = lerp;
        sizeScaleLerpTime = time;
        sizeScaleLerpValue = 0f;
        fromSizeScale_X = sizeScale_X;
        fromSizeScale_Y = sizeScale_Y;
        toSizeScale_X = newSizeScale_X;
        toSizeScale_Y = newSizeScale_Y;
    }

    public abstract void AddChild(ISleekElement child);

    public void addLabel(string text, ESleekSide side)
    {
        addLabel(text, Color.white, side);
    }

    public void addLabel(string text, Color color, ESleekSide side)
    {
        sideLabel = Glazier.Get().CreateLabel();
        switch (side)
        {
        case ESleekSide.LEFT:
            sideLabel.positionOffset_X = -205;
            sideLabel.fontAlignment = TextAnchor.MiddleRight;
            break;
        case ESleekSide.RIGHT:
            sideLabel.positionOffset_X = 5;
            sideLabel.positionScale_X = 1f;
            sideLabel.fontAlignment = TextAnchor.MiddleLeft;
            break;
        }
        sideLabel.positionOffset_Y = -30;
        sideLabel.positionScale_Y = 0.5f;
        sideLabel.sizeOffset_X = 200;
        sideLabel.sizeOffset_Y = 60;
        if (color != Color.white)
        {
            sideLabel.textColor = color;
        }
        sideLabel.text = text;
        sideLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        AddChild(sideLabel);
    }

    public void updateLabel(string text)
    {
        sideLabel.text = text;
    }

    public abstract int FindIndexOfChild(ISleekElement sleek);

    public abstract ISleekElement GetChildAtIndex(int index);

    public abstract void RemoveChild(ISleekElement child);

    public abstract void RemoveAllChildren();

    public virtual void Update()
    {
        if (isAnimatingTransform)
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
                positionOffset_X = toPositionOffset_X;
                positionOffset_Y = toPositionOffset_Y;
            }
            else
            {
                positionOffset_X = Mathf.RoundToInt(Mathf.Lerp(fromPositionOffset_X, toPositionOffset_X, positionOffsetLerpValue));
                positionOffset_Y = Mathf.RoundToInt(Mathf.Lerp(fromPositionOffset_Y, toPositionOffset_Y, positionOffsetLerpValue));
            }
            positionOffsetLerpValue = InterpValue(positionOffsetLerpValue, positionOffsetLerpMethod, positionOffsetLerpTime, unscaledDeltaTime);
        }
        if (isAnimatingPositionScale)
        {
            if (positionScaleLerpValue >= 0.999f)
            {
                isAnimatingPositionScale = false;
                positionScale_X = toPositionScale_X;
                positionScale_Y = toPositionScale_Y;
            }
            else
            {
                positionScale_X = Mathf.Lerp(fromPositionScale_X, toPositionScale_X, positionScaleLerpValue);
                positionScale_Y = Mathf.Lerp(fromPositionScale_Y, toPositionScale_Y, positionScaleLerpValue);
            }
            positionScaleLerpValue = InterpValue(positionScaleLerpValue, positionScaleLerpMethod, positionScaleLerpTime, unscaledDeltaTime);
        }
        if (isAnimatingSizeOffset)
        {
            if (sizeOffsetLerpValue >= 0.999f)
            {
                isAnimatingSizeOffset = false;
                sizeOffset_X = toSizeOffset_X;
                sizeOffset_Y = toSizeOffset_Y;
            }
            else
            {
                sizeOffset_X = Mathf.RoundToInt(Mathf.Lerp(fromSizeOffset_X, toSizeOffset_X, sizeOffsetLerpValue));
                sizeOffset_Y = Mathf.RoundToInt(Mathf.Lerp(fromSizeOffset_Y, toSizeOffset_Y, sizeOffsetLerpValue));
            }
            sizeOffsetLerpValue = InterpValue(sizeOffsetLerpValue, sizeOffsetLerpMethod, sizeOffsetLerpTime, unscaledDeltaTime);
        }
        if (isAnimatingSizeScale)
        {
            if (sizeScaleLerpValue >= 0.999f)
            {
                isAnimatingSizeScale = false;
                sizeScale_X = toSizeScale_X;
                sizeScale_Y = toSizeScale_Y;
            }
            else
            {
                sizeScale_X = Mathf.Lerp(fromSizeScale_X, toSizeScale_X, sizeScaleLerpValue);
                sizeScale_Y = Mathf.Lerp(fromSizeScale_Y, toSizeScale_Y, sizeScaleLerpValue);
            }
            sizeScaleLerpValue = InterpValue(sizeScaleLerpValue, sizeScaleLerpMethod, sizeScaleLerpTime, unscaledDeltaTime);
        }
    }

    public abstract Vector2 ViewportToNormalizedPosition(Vector2 viewportPosition);

    public abstract Vector2 GetNormalizedCursorPosition();

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
        sideLabel = null;
        _positionOffset_X = 0;
        _positionOffset_Y = 0;
        _positionScale_X = 0f;
        _positionScale_Y = 0f;
        _sizeOffset_X = 0;
        _sizeOffset_Y = 0;
        _sizeScale_X = 0f;
        _sizeScale_Y = 0f;
    }
}
