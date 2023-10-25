using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Base class for IMGUI implementations of primitive building block widgets.
/// </summary>
internal class GlazierElementBase_IMGUI : GlazierElementBase
{
    public GlazierElementBase_IMGUI _parent;

    /// <summary>
    /// Position passed into the GUI draw methods.
    /// </summary>
    public Rect drawRect;

    private List<GlazierElementBase_IMGUI> _children = new List<GlazierElementBase_IMGUI>();

    public override ISleekElement Parent => _parent;

    public virtual void OnGUI()
    {
        ChildrenOnGUI();
    }

    public override int FindIndexOfChild(ISleekElement child)
    {
        return _children.IndexOf((GlazierElementBase_IMGUI)child.AttachmentRoot);
    }

    public override ISleekElement GetChildAtIndex(int index)
    {
        return _children[index];
    }

    public override void RemoveChild(ISleekElement child)
    {
        if (child.AttachmentRoot is GlazierElementBase_IMGUI glazierElementBase_IMGUI)
        {
            glazierElementBase_IMGUI._parent = null;
            glazierElementBase_IMGUI.InternalDestroy();
            _children.Remove(glazierElementBase_IMGUI);
        }
        else
        {
            UnturnedLog.warn("{0} cannot remove non-IMGUI element {1}", GetType().Name, child.AttachmentRoot.GetType().Name);
        }
    }

    public override void RemoveAllChildren()
    {
        foreach (GlazierElementBase_IMGUI child in _children)
        {
            child._parent = null;
            child.InternalDestroy();
        }
        _children.Clear();
    }

    protected override void UpdateChildren()
    {
        foreach (GlazierElementBase_IMGUI child in _children)
        {
            if (child.IsVisible)
            {
                child.Update();
            }
        }
    }

    public override void AddChild(ISleekElement child)
    {
        if (child.AttachmentRoot is GlazierElementBase_IMGUI glazierElementBase_IMGUI)
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
            UnturnedLog.warn("{0} cannot add non-IMGUI element {1}", GetType().Name, child.AttachmentRoot.GetType().Name);
        }
    }

    public override void InternalDestroy()
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

    public override Vector2 GetAbsoluteSize()
    {
        return GetDrawRectInScreenSpace().size;
    }

    public override void SetAsFirstSibling()
    {
        if (_parent != null && _parent._children.Remove(this))
        {
            _parent._children.Insert(0, this);
        }
    }

    protected virtual void TransformChildDrawPositionIntoParentSpace(ref Vector2 position)
    {
    }

    protected Rect GetDrawRectInScreenSpace()
    {
        Rect result = drawRect;
        Vector2 position = result.position;
        for (GlazierElementBase_IMGUI parent = _parent; parent != null; parent = parent._parent)
        {
            parent.TransformChildDrawPositionIntoParentSpace(ref position);
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
            return new Rect(base.PositionOffset_X, base.PositionOffset_Y, Screen.width, Screen.height);
        }
        float userInterfaceScale = GraphicsSettings.userInterfaceScale;
        Rect layoutRect = _parent.GetLayoutRect();
        layoutRect.x += base.PositionOffset_X * userInterfaceScale + layoutRect.width * base.PositionScale_X;
        layoutRect.y += base.PositionOffset_Y * userInterfaceScale + layoutRect.height * base.PositionScale_Y;
        layoutRect.width = base.SizeOffset_X * userInterfaceScale + layoutRect.width * base.SizeScale_X;
        layoutRect.height = base.SizeOffset_Y * userInterfaceScale + layoutRect.height * base.SizeScale_Y;
        return layoutRect;
    }

    protected override void UpdateDirtyTransform()
    {
        isTransformDirty = false;
        drawRect = CalculateDrawRect();
        foreach (GlazierElementBase_IMGUI child in _children)
        {
            child.isTransformDirty = true;
        }
    }

    protected void ChildrenOnGUI()
    {
        for (int i = 0; i < _children.Count; i++)
        {
            GlazierElementBase_IMGUI glazierElementBase_IMGUI = _children[i];
            if (glazierElementBase_IMGUI.IsVisible)
            {
                glazierElementBase_IMGUI.OnGUI();
            }
        }
    }
}
