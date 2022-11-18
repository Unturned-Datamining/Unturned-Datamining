using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal abstract class GlazierElementBase_uGUI : GlazierElementBase
{
    public class PoolData
    {
        public GameObject gameObject;
    }

    public GlazierElementBase_uGUI _parent;

    internal List<GlazierElementBase_uGUI> _children = new List<GlazierElementBase_uGUI>();

    public Glazier_uGUI glazier { get; private set; }

    public override bool isVisible
    {
        get
        {
            return base.isVisible;
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

    public override ISleekElement parent => _parent;

    public virtual RectTransform AttachmentTransform => transform;

    public GameObject gameObject { get; private set; }

    public RectTransform transform { get; private set; }

    protected override IEnumerable<ISleekElement> children => _children;

    public GlazierElementBase_uGUI(Glazier_uGUI glazier)
    {
        this.glazier = glazier;
    }

    public virtual void ConstructNew()
    {
        gameObject = new GameObject(GetType().Name, typeof(RectTransform));
        transform = gameObject.GetRectTransform();
        transform.pivot = new Vector2(0f, 1f);
    }

    public void ConstructFromPool(PoolData poolData)
    {
        gameObject = poolData.gameObject;
        transform = gameObject.GetRectTransform();
        gameObject.SetActive(value: true);
    }

    public override int FindIndexOfChild(ISleekElement child)
    {
        return _children.IndexOf((GlazierElementBase_uGUI)child.attachmentRoot);
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
        if (child.attachmentRoot is GlazierElementBase_uGUI glazierElementBase_uGUI)
        {
            glazierElementBase_uGUI._parent = null;
            glazierElementBase_uGUI.destroy();
            _children.Remove(glazierElementBase_uGUI);
        }
        else
        {
            UnturnedLog.warn("{0} cannot remove non-IMGUI element {1}", GetType().Name, child.attachmentRoot.GetType().Name);
        }
    }

    public override void RemoveAllChildren()
    {
        foreach (GlazierElementBase_uGUI child in children)
        {
            child._parent = null;
            child.destroy();
        }
        _children.Clear();
    }

    public virtual void SynchronizeColors()
    {
    }

    public virtual void SynchronizeTheme()
    {
    }

    public override void AddChild(ISleekElement child)
    {
        if (child.attachmentRoot is GlazierElementBase_uGUI glazierElementBase_uGUI)
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
            UnturnedLog.warn("{0} cannot add non-uGUI element {1}", GetType().Name, child.attachmentRoot.GetType().Name);
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

    protected override void UpdateDirtyTransform()
    {
        isTransformDirty = false;
        transform.anchorMin = new Vector2(base.positionScale_X, 1f - base.positionScale_Y - base.sizeScale_Y);
        transform.anchorMax = new Vector2(base.positionScale_X + base.sizeScale_X, 1f - base.positionScale_Y);
        transform.anchoredPosition = new Vector2(base.positionOffset_X, -base.positionOffset_Y);
        transform.sizeDelta = new Vector2(base.sizeOffset_X, base.sizeOffset_Y);
    }

    protected virtual bool ReleaseIntoPool()
    {
        return false;
    }

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

    public override void destroy()
    {
        RemoveAllChildren();
        if (!ReleaseIntoPool() && gameObject != null)
        {
            UnityEngine.Object.Destroy(gameObject);
            gameObject = null;
        }
    }
}
