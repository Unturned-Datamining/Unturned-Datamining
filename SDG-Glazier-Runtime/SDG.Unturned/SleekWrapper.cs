using System.Diagnostics;
using UnityEngine;

namespace SDG.Unturned;

public class SleekWrapper : ISleekElement
{
    private ISleekElement implementation;

    public bool isVisible
    {
        get
        {
            return implementation.isVisible;
        }
        set
        {
            implementation.isVisible = value;
        }
    }

    public ISleekElement parent => implementation.parent;

    public ISleekLabel sideLabel => implementation.sideLabel;

    public int positionOffset_X
    {
        get
        {
            return implementation.positionOffset_X;
        }
        set
        {
            implementation.positionOffset_X = value;
        }
    }

    public int positionOffset_Y
    {
        get
        {
            return implementation.positionOffset_Y;
        }
        set
        {
            implementation.positionOffset_Y = value;
        }
    }

    public float positionScale_X
    {
        get
        {
            return implementation.positionScale_X;
        }
        set
        {
            implementation.positionScale_X = value;
        }
    }

    public float positionScale_Y
    {
        get
        {
            return implementation.positionScale_Y;
        }
        set
        {
            implementation.positionScale_Y = value;
        }
    }

    public int sizeOffset_X
    {
        get
        {
            return implementation.sizeOffset_X;
        }
        set
        {
            implementation.sizeOffset_X = value;
        }
    }

    public int sizeOffset_Y
    {
        get
        {
            return implementation.sizeOffset_Y;
        }
        set
        {
            implementation.sizeOffset_Y = value;
        }
    }

    public float sizeScale_X
    {
        get
        {
            return implementation.sizeScale_X;
        }
        set
        {
            implementation.sizeScale_X = value;
        }
    }

    public float sizeScale_Y
    {
        get
        {
            return implementation.sizeScale_Y;
        }
        set
        {
            implementation.sizeScale_Y = value;
        }
    }

    public ISleekElement attachmentRoot => implementation;

    public bool isAnimatingTransform => implementation.isAnimatingTransform;

    public void destroy()
    {
        implementation.destroy();
    }

    public void lerpPositionOffset(int newPositionOffset_X, int newPositionOffset_Y, ESleekLerp lerp, float time)
    {
        implementation.lerpPositionOffset(newPositionOffset_X, newPositionOffset_Y, lerp, time);
    }

    public void lerpPositionScale(float newPositionScale_X, float newPositionScale_Y, ESleekLerp lerp, float time)
    {
        implementation.lerpPositionScale(newPositionScale_X, newPositionScale_Y, lerp, time);
    }

    public void lerpSizeOffset(int newSizeOffset_X, int newSizeOffset_Y, ESleekLerp lerp, float time)
    {
        implementation.lerpSizeOffset(newSizeOffset_X, newSizeOffset_Y, lerp, time);
    }

    public void lerpSizeScale(float newSizeScale_X, float newSizeScale_Y, ESleekLerp lerp, float time)
    {
        implementation.lerpSizeScale(newSizeScale_X, newSizeScale_Y, lerp, time);
    }

    public void AddChild(ISleekElement sleek)
    {
        implementation.AddChild(sleek);
    }

    public void addLabel(string text, ESleekSide side)
    {
        implementation.addLabel(text, side);
    }

    public void addLabel(string text, Color color, ESleekSide side)
    {
        implementation.addLabel(text, color, side);
    }

    public void updateLabel(string text)
    {
        implementation.updateLabel(text);
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
