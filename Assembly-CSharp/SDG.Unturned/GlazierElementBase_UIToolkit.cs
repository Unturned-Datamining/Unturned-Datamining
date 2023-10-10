using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned;

internal abstract class GlazierElementBase_UIToolkit : GlazierElementBase
{
    public GlazierElementBase_UIToolkit _parent;

    public VisualElement visualElement;

    private List<GlazierElementBase_UIToolkit> _children = new List<GlazierElementBase_UIToolkit>();

    public Glazier_UIToolkit glazier { get; private set; }

    public override bool IsVisible
    {
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                visualElement.visible = _isVisible;
                visualElement.style.visibility = ((!_isVisible) ? Visibility.Hidden : Visibility.Visible);
                visualElement.style.display = ((!_isVisible) ? DisplayStyle.None : DisplayStyle.Flex);
            }
        }
    }

    public override bool UseManualLayout
    {
        set
        {
            isTransformDirty |= _useManualLayout != value;
            _useManualLayout = value;
        }
    }

    public override bool UseWidthLayoutOverride
    {
        set
        {
            isTransformDirty |= _useWidthLayoutOverride != value;
            _useWidthLayoutOverride = value;
        }
    }

    public override bool UseHeightLayoutOverride
    {
        set
        {
            isTransformDirty |= _useHeightLayoutOverride != value;
            _useHeightLayoutOverride = value;
        }
    }

    public override ESleekChildLayout UseChildAutoLayout
    {
        set
        {
            base.UseChildAutoLayout = value;
            visualElement.style.flexDirection = ((value == ESleekChildLayout.Horizontal) ? ((StyleEnum<FlexDirection>)FlexDirection.Row) : ((StyleEnum<FlexDirection>)StyleKeyword.Null));
            ApplyChildPerpendicularAlignment();
        }
    }

    public override ESleekChildPerpendicularAlignment ChildPerpendicularAlignment
    {
        set
        {
            base.ChildPerpendicularAlignment = value;
            ApplyChildPerpendicularAlignment();
        }
    }

    public override bool ExpandChildren
    {
        set
        {
            bool num = _expandChildren != value;
            _expandChildren = value;
            if (!num)
            {
                return;
            }
            StyleFloat flexGrow = (_expandChildren ? ((StyleFloat)1f) : ((StyleFloat)StyleKeyword.Null));
            foreach (GlazierElementBase_UIToolkit child in _children)
            {
                child.visualElement.style.flexGrow = flexGrow;
            }
        }
    }

    public override bool IgnoreLayout
    {
        set
        {
            isTransformDirty |= _ignoreLayout != value;
            _ignoreLayout = value;
        }
    }

    public override ISleekElement Parent => _parent;

    public GlazierElementBase_UIToolkit(Glazier_UIToolkit glazier)
    {
        this.glazier = glazier;
    }

    public override int FindIndexOfChild(ISleekElement child)
    {
        return _children.IndexOf((GlazierElementBase_UIToolkit)child.AttachmentRoot);
    }

    public override ISleekElement GetChildAtIndex(int index)
    {
        return _children[index];
    }

    public override void RemoveChild(ISleekElement child)
    {
        if (child == null)
        {
            throw new ArgumentNullException("child");
        }
        if (child.AttachmentRoot is GlazierElementBase_UIToolkit glazierElementBase_UIToolkit)
        {
            glazierElementBase_UIToolkit._parent = null;
            glazierElementBase_UIToolkit.InternalDestroy();
            _children.Remove(glazierElementBase_UIToolkit);
        }
        else
        {
            UnturnedLog.warn("{0} cannot remove non-UIToolkit element {1}", GetType().Name, child.AttachmentRoot.GetType().Name);
        }
    }

    public override void RemoveAllChildren()
    {
        foreach (GlazierElementBase_UIToolkit child in _children)
        {
            child._parent = null;
            child.InternalDestroy();
        }
        _children.Clear();
    }

    protected override void UpdateChildren()
    {
        foreach (GlazierElementBase_UIToolkit child in _children)
        {
            if (child.IsVisible)
            {
                child.Update();
            }
        }
    }

    public override void AddChild(ISleekElement child)
    {
        if (child.AttachmentRoot is GlazierElementBase_UIToolkit glazierElementBase_UIToolkit)
        {
            if (glazierElementBase_UIToolkit._parent != this)
            {
                if (glazierElementBase_UIToolkit._parent != null)
                {
                    glazierElementBase_UIToolkit._parent._children.Remove(glazierElementBase_UIToolkit);
                }
                _children.Add(glazierElementBase_UIToolkit);
                glazierElementBase_UIToolkit._parent = this;
                glazierElementBase_UIToolkit.visualElement.style.flexGrow = (_expandChildren ? ((StyleFloat)1f) : ((StyleFloat)StyleKeyword.Null));
                visualElement.Add(glazierElementBase_UIToolkit.visualElement);
                glazierElementBase_UIToolkit.UpdateDirtyTransform();
            }
        }
        else
        {
            UnturnedLog.warn("{0} cannot add non-UIToolkit element {1}", GetType().Name, child.AttachmentRoot.GetType().Name);
        }
    }

    public override Vector2 ViewportToNormalizedPosition(Vector2 viewportPosition)
    {
        if (visualElement.panel == null)
        {
            return Vector2.zero;
        }
        Rect worldBound = visualElement.worldBound;
        if (Mathf.Approximately(worldBound.width, 0f) || Mathf.Approximately(worldBound.height, 0f))
        {
            return Vector2.zero;
        }
        Rect worldBound2 = visualElement.panel.visualTree.worldBound;
        return new Vector2((viewportPosition.x * worldBound2.width - worldBound.xMin) / worldBound.width, ((1f - viewportPosition.y) * worldBound2.height - worldBound.yMin) / worldBound.height);
    }

    public override Vector2 GetNormalizedCursorPosition()
    {
        if (visualElement.panel == null)
        {
            return Vector2.zero;
        }
        Rect worldBound = visualElement.panel.visualTree.worldBound;
        if (Mathf.Approximately(worldBound.width, 0f) || Mathf.Approximately(worldBound.height, 0f))
        {
            return Vector2.zero;
        }
        Vector2 normalizedMousePosition = InputEx.NormalizedMousePosition;
        Rect worldBound2 = visualElement.worldBound;
        return new Vector2((normalizedMousePosition.x - worldBound2.xMin / worldBound.width) / (worldBound2.width / worldBound.width), (1f - normalizedMousePosition.y - worldBound2.yMin / worldBound.height) / (worldBound2.height / worldBound.height));
    }

    public override Vector2 GetAbsoluteSize()
    {
        if (visualElement.panel == null)
        {
            return Vector2.zero;
        }
        Rect worldBound = visualElement.panel.visualTree.worldBound;
        if (Mathf.Approximately(worldBound.width, 0f) || Mathf.Approximately(worldBound.height, 0f))
        {
            return Vector2.zero;
        }
        Rect worldBound2 = visualElement.worldBound;
        return new Vector2(worldBound2.width / worldBound.width * (float)Screen.width, worldBound2.height / worldBound.height * (float)Screen.height);
    }

    public override void SetAsFirstSibling()
    {
        if (_parent != null)
        {
            visualElement.SendToBack();
            if (_parent._children.Remove(this))
            {
                _parent._children.Insert(0, this);
            }
        }
    }

    public override void InternalDestroy()
    {
        RemoveAllChildren();
        visualElement.RemoveFromHierarchy();
        glazier.RemoveDestroyedElement(this);
    }

    internal virtual void SynchronizeColors()
    {
    }

    internal virtual bool GetTooltipParameters(out string tooltipText, out Color tooltipColor)
    {
        tooltipText = null;
        tooltipColor = default(Color);
        return false;
    }

    protected override void UpdateDirtyTransform()
    {
        isTransformDirty = false;
        visualElement.style.position = ((_useManualLayout || _ignoreLayout) ? Position.Absolute : Position.Relative);
        if (_useManualLayout)
        {
            visualElement.style.left = Length.Percent(base.PositionScale_X * 100f);
            visualElement.style.top = Length.Percent(base.PositionScale_Y * 100f);
            visualElement.style.right = Length.Percent((1f - base.SizeScale_X - base.PositionScale_X) * 100f);
            visualElement.style.bottom = Length.Percent((1f - base.SizeScale_Y - base.PositionScale_Y) * 100f);
            visualElement.style.marginLeft = base.PositionOffset_X;
            visualElement.style.marginTop = base.PositionOffset_Y;
            visualElement.style.marginRight = 0f - base.PositionOffset_X - base.SizeOffset_X;
            visualElement.style.marginBottom = 0f - base.PositionOffset_Y - base.SizeOffset_Y;
            visualElement.style.width = StyleKeyword.Null;
            visualElement.style.height = StyleKeyword.Null;
        }
        else
        {
            visualElement.style.left = StyleKeyword.Null;
            visualElement.style.right = StyleKeyword.Null;
            visualElement.style.top = StyleKeyword.Null;
            visualElement.style.bottom = StyleKeyword.Null;
            visualElement.style.marginLeft = StyleKeyword.Null;
            visualElement.style.marginTop = StyleKeyword.Null;
            visualElement.style.marginRight = StyleKeyword.Null;
            visualElement.style.marginBottom = StyleKeyword.Null;
            visualElement.style.width = (_useWidthLayoutOverride ? ((StyleLength)base.SizeOffset_X) : ((StyleLength)StyleKeyword.Null));
            visualElement.style.height = (_useHeightLayoutOverride ? ((StyleLength)base.SizeOffset_Y) : ((StyleLength)StyleKeyword.Null));
        }
    }

    private void ApplyChildPerpendicularAlignment()
    {
        if (_useChildAutoLayout == ESleekChildLayout.Horizontal)
        {
            switch (_childPerpendicularAlignment)
            {
            default:
                visualElement.style.alignItems = StyleKeyword.Null;
                break;
            case ESleekChildPerpendicularAlignment.Top:
                visualElement.style.alignItems = Align.FlexStart;
                break;
            case ESleekChildPerpendicularAlignment.Bottom:
                visualElement.style.alignItems = Align.FlexEnd;
                break;
            }
        }
    }
}
