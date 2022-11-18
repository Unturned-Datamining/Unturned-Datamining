using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal class GlazierElementBase_IMGUI : GlazierElementBase
{
    public GlazierElementBase_IMGUI _parent;

    public Rect drawRect;

    private List<GlazierElementBase_IMGUI> _children = new List<GlazierElementBase_IMGUI>();

    public override ISleekElement parent => _parent;

    protected override IEnumerable<ISleekElement> children => _children;

    public virtual void OnGUI()
    {
        ChildrenOnGUI();
    }

    public override int FindIndexOfChild(ISleekElement child)
    {
        return _children.IndexOf((GlazierElementBase_IMGUI)child.attachmentRoot);
    }

    public override ISleekElement GetChildAtIndex(int index)
    {
        return _children[index];
    }

    public override void RemoveChild(ISleekElement child)
    {
        if (child.attachmentRoot is GlazierElementBase_IMGUI glazierElementBase_IMGUI)
        {
            glazierElementBase_IMGUI._parent = null;
            glazierElementBase_IMGUI.destroy();
            _children.Remove(glazierElementBase_IMGUI);
        }
        else
        {
            UnturnedLog.warn("{0} cannot remove non-IMGUI element {1}", GetType().Name, child.attachmentRoot.GetType().Name);
        }
    }

    public override void RemoveAllChildren()
    {
        foreach (GlazierElementBase_IMGUI child in children)
        {
            child._parent = null;
            child.destroy();
        }
        _children.Clear();
    }

    public override void AddChild(ISleekElement child)
    {
        if (child.attachmentRoot is GlazierElementBase_IMGUI glazierElementBase_IMGUI)
        {
            if (glazierElementBase_IMGUI._parent != this)
            {
                if (glazierElementBase_IMGUI._parent != null)
                {
                    glazierElementBase_IMGUI._parent._children.Remove(glazierElementBase_IMGUI);
                }
                _children.Add(glazierElementBase_IMGUI);
                glazierElementBase_IMGUI._parent = this;
                glazierElementBase_IMGUI.UpdateDirtyTransform();
            }
        }
        else
        {
            UnturnedLog.warn("{0} cannot add non-IMGUI element {1}", GetType().Name, child.attachmentRoot.GetType().Name);
        }
    }

    public override void destroy()
    {
        RemoveAllChildren();
    }

    public override Vector2 ViewportToNormalizedPosition(Vector2 viewportPosition)
    {
        Rect drawRectInScreenSpace = GetDrawRectInScreenSpace();
        Vector2 result = default(Vector2);
        if (drawRectInScreenSpace.width > 0f)
        {
            result.x = (viewportPosition.x * (float)Screen.width - drawRectInScreenSpace.xMin) / drawRectInScreenSpace.width;
        }
        else
        {
            result.x = 0.5f;
        }
        if (drawRectInScreenSpace.height > 0f)
        {
            result.y = ((1f - viewportPosition.y) * (float)Screen.height - drawRectInScreenSpace.yMin) / drawRectInScreenSpace.height;
        }
        else
        {
            result.y = 0.5f;
        }
        return result;
    }

    public override Vector2 GetNormalizedCursorPosition()
    {
        Vector2 vector = Input.mousePosition;
        Rect drawRectInScreenSpace = GetDrawRectInScreenSpace();
        Vector2 result = default(Vector2);
        if (drawRectInScreenSpace.width > 0f)
        {
            result.x = (vector.x - drawRectInScreenSpace.xMin) / drawRectInScreenSpace.width;
        }
        else
        {
            result.x = 0.5f;
        }
        if (drawRectInScreenSpace.height > 0f)
        {
            result.y = ((float)Screen.height - vector.y - drawRectInScreenSpace.yMin) / drawRectInScreenSpace.height;
        }
        else
        {
            result.y = 0.5f;
        }
        return result;
    }

    protected virtual void TransformChildDrawPositionIntoParentSpace(ref Vector2 position)
    {
    }

    protected Rect GetDrawRectInScreenSpace()
    {
        Rect result = drawRect;
        Vector2 position = result.position;
        for (GlazierElementBase_IMGUI glazierElementBase_IMGUI = _parent; glazierElementBase_IMGUI != null; glazierElementBase_IMGUI = glazierElementBase_IMGUI._parent)
        {
            glazierElementBase_IMGUI.TransformChildDrawPositionIntoParentSpace(ref position);
        }
        result.position = position;
        return result;
    }

    protected virtual Rect GetLayoutRect()
    {
        return drawRect;
    }

    protected virtual Rect CalculateDrawRect()
    {
        if (_parent == null)
        {
            if (Screen.width == 5760 && Screen.height == 1080)
            {
                return new Rect(1920f, 0f, 1920f, 1080f);
            }
            return new Rect(base.positionOffset_X, base.positionOffset_Y, Screen.width, Screen.height);
        }
        float userInterfaceScale = GraphicsSettings.userInterfaceScale;
        Rect layoutRect = _parent.GetLayoutRect();
        layoutRect.x += (float)base.positionOffset_X * userInterfaceScale + layoutRect.width * base.positionScale_X;
        layoutRect.y += (float)base.positionOffset_Y * userInterfaceScale + layoutRect.height * base.positionScale_Y;
        layoutRect.width = (float)base.sizeOffset_X * userInterfaceScale + layoutRect.width * base.sizeScale_X;
        layoutRect.height = (float)base.sizeOffset_Y * userInterfaceScale + layoutRect.height * base.sizeScale_Y;
        return layoutRect;
    }

    protected override void UpdateDirtyTransform()
    {
        isTransformDirty = false;
        drawRect = CalculateDrawRect();
        foreach (GlazierElementBase_IMGUI child in children)
        {
            child.isTransformDirty = true;
        }
    }

    protected void ChildrenOnGUI()
    {
        for (int i = 0; i < _children.Count; i++)
        {
            GlazierElementBase_IMGUI glazierElementBase_IMGUI = _children[i];
            if (glazierElementBase_IMGUI.isVisible)
            {
                glazierElementBase_IMGUI.OnGUI();
            }
        }
    }
}
