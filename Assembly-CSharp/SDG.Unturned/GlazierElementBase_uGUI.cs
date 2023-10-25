using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

/// <summary>
/// Base class for uGUI implementations of primitive building block widgets.
/// </summary>
internal abstract class GlazierElementBase_uGUI : GlazierElementBase
{
    public class PoolData
    {
        public GameObject gameObject;
    }

    public GlazierElementBase_uGUI _parent;

    internal List<GlazierElementBase_uGUI> _children = new List<GlazierElementBase_uGUI>();

    public Glazier_uGUI glazier { get; private set; }

    public override bool IsVisible
    {
        get
        {
            return base.IsVisible;
        }
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                gameObject.SetActive(value);
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
            bool flag = _useWidthLayoutOverride != value;
            isTransformDirty |= flag;
            _useWidthLayoutOverride = value;
            if (flag)
            {
                if (ShouldHaveLayoutElementComponent)
                {
                    transform.GetOrAddComponent<LayoutElement>();
                }
                else
                {
                    transform.DestroyComponentIfExists<LayoutElement>();
                }
            }
        }
    }

    public override bool UseHeightLayoutOverride
    {
        set
        {
            bool flag = _useHeightLayoutOverride != value;
            isTransformDirty |= flag;
            _useHeightLayoutOverride = value;
            if (flag)
            {
                if (ShouldHaveLayoutElementComponent)
                {
                    transform.GetOrAddComponent<LayoutElement>();
                }
                else
                {
                    transform.DestroyComponentIfExists<LayoutElement>();
                }
            }
        }
    }

    public override ESleekChildLayout UseChildAutoLayout
    {
        set
        {
            bool num = _useChildAutoLayout != value;
            _useChildAutoLayout = value;
            if (num)
            {
                if (_useChildAutoLayout == ESleekChildLayout.Horizontal)
                {
                    HorizontalLayoutGroup orAddComponent = transform.GetOrAddComponent<HorizontalLayoutGroup>();
                    orAddComponent.childForceExpandWidth = _expandChildren;
                    orAddComponent.childForceExpandHeight = false;
                }
                else
                {
                    transform.DestroyComponentIfExists<HorizontalLayoutGroup>();
                }
                if (_useChildAutoLayout == ESleekChildLayout.Vertical)
                {
                    VerticalLayoutGroup orAddComponent2 = transform.GetOrAddComponent<VerticalLayoutGroup>();
                    orAddComponent2.childForceExpandWidth = false;
                    orAddComponent2.childForceExpandHeight = _expandChildren;
                }
                else
                {
                    transform.DestroyComponentIfExists<VerticalLayoutGroup>();
                }
                ApplyChildPerpendicularAlignment();
            }
        }
    }

    public override ESleekChildPerpendicularAlignment ChildPerpendicularAlignment
    {
        set
        {
            _childPerpendicularAlignment = value;
            ApplyChildPerpendicularAlignment();
        }
    }

    public override bool ExpandChildren
    {
        set
        {
            bool num = _expandChildren != value;
            _expandChildren = value;
            if (num)
            {
                switch (_useChildAutoLayout)
                {
                case ESleekChildLayout.Horizontal:
                    transform.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = _expandChildren;
                    break;
                case ESleekChildLayout.Vertical:
                    transform.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = _expandChildren;
                    break;
                }
            }
        }
    }

    public override bool IgnoreLayout
    {
        set
        {
            bool num = _ignoreLayout != value;
            _ignoreLayout = value;
            if (num)
            {
                if (ShouldHaveLayoutElementComponent)
                {
                    transform.GetOrAddComponent<LayoutElement>().ignoreLayout = true;
                }
                else
                {
                    transform.DestroyComponentIfExists<LayoutElement>();
                }
            }
        }
    }

    public override ISleekElement Parent => _parent;

    /// <summary>
    /// RectTransform children should be attached to. Overridden by ScrollView content panel.
    /// </summary>
    public virtual RectTransform AttachmentTransform => transform;

    public GameObject gameObject { get; private set; }

    public RectTransform transform { get; private set; }

    /// <summary>
    /// This helper property's purpose is to:
    /// - Ensure other properties don't accidentally remove LayoutElement if others need it.
    /// - Ensure LayoutElement is destroyed before returning to pool.
    /// </summary>
    private bool ShouldHaveLayoutElementComponent
    {
        get
        {
            if (!_useWidthLayoutOverride && !_useHeightLayoutOverride)
            {
                return _ignoreLayout;
            }
            return true;
        }
    }

    public GlazierElementBase_uGUI(Glazier_uGUI glazier)
    {
        this.glazier = glazier;
    }

    /// <summary>
    /// Called after constructor when not populating from component pool.
    /// </summary>
    public virtual void ConstructNew()
    {
        gameObject = new GameObject(GetType().Name, typeof(RectTransform));
        transform = gameObject.GetRectTransform();
        transform.pivot = new Vector2(0f, 1f);
    }

    /// <summary>
    /// Called after constructor when re-using components from pool.
    /// </summary>
    public void ConstructFromPool(PoolData poolData)
    {
        gameObject = poolData.gameObject;
        transform = gameObject.GetRectTransform();
        gameObject.SetActive(value: true);
    }

    public override int FindIndexOfChild(ISleekElement child)
    {
        return _children.IndexOf((GlazierElementBase_uGUI)child.AttachmentRoot);
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
        if (child.AttachmentRoot is GlazierElementBase_uGUI glazierElementBase_uGUI)
        {
            glazierElementBase_uGUI._parent = null;
            glazierElementBase_uGUI.InternalDestroy();
            _children.Remove(glazierElementBase_uGUI);
        }
        else
        {
            UnturnedLog.warn("{0} cannot remove non-IMGUI element {1}", GetType().Name, child.AttachmentRoot.GetType().Name);
        }
    }

    public override void RemoveAllChildren()
    {
        foreach (GlazierElementBase_uGUI child in _children)
        {
            child._parent = null;
            child.InternalDestroy();
        }
        _children.Clear();
    }

    protected override void UpdateChildren()
    {
        foreach (GlazierElementBase_uGUI child in _children)
        {
            if (child.IsVisible)
            {
                child.Update();
            }
        }
    }

    /// <summary>
    /// Synchronize uGUI component colors with background/text/image etc. colors.
    /// Called when custom UI colors are changed, and after constructor.
    /// </summary>
    public virtual void SynchronizeColors()
    {
    }

    /// <summary>
    /// Synchronize uGUI component sprites with theme sprites.
    /// Called when custom UI theme is changed, and after constructor.
    /// </summary>
    public virtual void SynchronizeTheme()
    {
    }

    public override void AddChild(ISleekElement child)
    {
        if (child.AttachmentRoot is GlazierElementBase_uGUI glazierElementBase_uGUI)
        {
            if (glazierElementBase_uGUI._parent != this)
            {
                if (glazierElementBase_uGUI._parent != null)
                {
                    glazierElementBase_uGUI._parent._children.Remove(glazierElementBase_uGUI);
                }
                _children.Add(glazierElementBase_uGUI);
                glazierElementBase_uGUI._parent = this;
                glazierElementBase_uGUI.transform.SetParent(AttachmentTransform, worldPositionStays: false);
                glazierElementBase_uGUI.UpdateDirtyTransform();
                glazierElementBase_uGUI.EnableComponents();
            }
        }
        else
        {
            UnturnedLog.warn("{0} cannot add non-uGUI element {1}", GetType().Name, child.AttachmentRoot.GetType().Name);
        }
    }

    public override Vector2 ViewportToNormalizedPosition(Vector2 viewportPosition)
    {
        Rect absoluteRect = transform.GetAbsoluteRect();
        return new Vector2((viewportPosition.x * (float)Screen.width - absoluteRect.xMin) / absoluteRect.width, ((1f - viewportPosition.y) * (float)Screen.height - absoluteRect.yMin) / absoluteRect.height);
    }

    public override Vector2 GetNormalizedCursorPosition()
    {
        Vector2 vector = Input.mousePosition;
        Rect absoluteRect = transform.GetAbsoluteRect();
        return new Vector2((vector.x - absoluteRect.xMin) / absoluteRect.width, ((float)Screen.height - vector.y - absoluteRect.yMin) / absoluteRect.height);
    }

    public override Vector2 GetAbsoluteSize()
    {
        return transform.GetAbsoluteRect().size;
    }

    public override void SetAsFirstSibling()
    {
        if (_parent != null)
        {
            transform.SetAsFirstSibling();
            if (_parent._children.Remove(this))
            {
                _parent._children.Insert(0, this);
            }
        }
    }

    protected override void UpdateDirtyTransform()
    {
        isTransformDirty = false;
        if (_useManualLayout)
        {
            transform.anchorMin = new Vector2(base.PositionScale_X, 1f - base.PositionScale_Y - base.SizeScale_Y);
            transform.anchorMax = new Vector2(base.PositionScale_X + base.SizeScale_X, 1f - base.PositionScale_Y);
            transform.anchoredPosition = new Vector2(base.PositionOffset_X, 0f - base.PositionOffset_Y);
            transform.sizeDelta = new Vector2(base.SizeOffset_X, base.SizeOffset_Y);
        }
        else
        {
            transform.anchorMin = new Vector2(0f, 1f);
            transform.anchorMax = new Vector2(0f, 1f);
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta = Vector2.zero;
        }
        if (_useWidthLayoutOverride || _useHeightLayoutOverride)
        {
            LayoutElement component = transform.GetComponent<LayoutElement>();
            if (component != null)
            {
                component.preferredWidth = (_useWidthLayoutOverride ? base.SizeOffset_X : 0f);
                component.preferredHeight = (_useHeightLayoutOverride ? base.SizeOffset_Y : 0f);
                component.minWidth = component.preferredWidth;
                component.minHeight = component.preferredHeight;
            }
        }
    }

    /// <returns>False if element couldn't be released into pool and should be destroyed.</returns>
    protected virtual bool ReleaseIntoPool()
    {
        return false;
    }

    /// <summary>
    /// Unity recommends enabling components after parenting into the destination hierarchy.
    /// </summary>
    protected virtual void EnableComponents()
    {
    }

    protected void PopulateBasePoolData(PoolData poolData)
    {
        transform.SetParent(null, worldPositionStays: false);
        UnityEngine.Object.DontDestroyOnLoad(gameObject);
        poolData.gameObject = gameObject;
        gameObject = null;
        transform = null;
    }

    public override void InternalDestroy()
    {
        RemoveAllChildren();
        if ((ShouldHaveLayoutElementComponent || _useChildAutoLayout != 0 || !ReleaseIntoPool()) && gameObject != null)
        {
            UnityEngine.Object.Destroy(gameObject);
            gameObject = null;
        }
    }

    private void ApplyChildPerpendicularAlignment()
    {
        if (_useChildAutoLayout == ESleekChildLayout.Horizontal)
        {
            HorizontalLayoutGroup component = transform.GetComponent<HorizontalLayoutGroup>();
            switch (_childPerpendicularAlignment)
            {
            default:
                component.childAlignment = TextAnchor.MiddleLeft;
                break;
            case ESleekChildPerpendicularAlignment.Top:
                component.childAlignment = TextAnchor.UpperLeft;
                break;
            case ESleekChildPerpendicularAlignment.Bottom:
                component.childAlignment = TextAnchor.LowerLeft;
                break;
            }
        }
    }
}
