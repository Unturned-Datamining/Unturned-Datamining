using System;
using UnityEngine;

namespace SDG.Unturned;

internal class GlazierScrollView_IMGUI : GlazierElementBase_IMGUI, ISleekScrollView, ISleekElement
{
    private bool _scaleContentToWidth;

    private bool _scaleContentToHeight;

    private float _contentScaleFactor = 1f;

    private bool _reduceWidthWhenScrollbarVisible = true;

    private Vector2 _contentSizeOffset;

    private Vector2 state;

    private bool _isRaycastTarget = true;

    private Rect contentRect;

    private Rect viewRect;

    public bool ScaleContentToWidth
    {
        get
        {
            return _scaleContentToWidth;
        }
        set
        {
            _scaleContentToWidth = value;
            isTransformDirty = true;
        }
    }

    public bool ScaleContentToHeight
    {
        get
        {
            return _scaleContentToHeight;
        }
        set
        {
            _scaleContentToHeight = value;
            isTransformDirty = true;
        }
    }

    public float ContentScaleFactor
    {
        get
        {
            return _contentScaleFactor;
        }
        set
        {
            _contentScaleFactor = value;
            isTransformDirty = true;
        }
    }

    public bool ReduceWidthWhenScrollbarVisible
    {
        get
        {
            return _reduceWidthWhenScrollbarVisible;
        }
        set
        {
            _reduceWidthWhenScrollbarVisible = value;
            isTransformDirty = true;
        }
    }

    public ESleekScrollbarVisibility VerticalScrollbarVisibility { get; set; }

    public Vector2 ContentSizeOffset
    {
        get
        {
            return _contentSizeOffset;
        }
        set
        {
            _contentSizeOffset = value;
            isTransformDirty = true;
        }
    }

    public Vector2 NormalizedStateCenter
    {
        get
        {
            if (isTransformDirty)
            {
                UpdateDirtyTransform();
            }
            return new Vector2((state.x + drawRect.width * 0.5f) / contentRect.width, (state.y + drawRect.height * 0.5f) / contentRect.height);
        }
        set
        {
            if (isTransformDirty)
            {
                UpdateDirtyTransform();
            }
            state = new Vector2(value.x * contentRect.width - drawRect.width * 0.5f, value.y * contentRect.height - drawRect.height * 0.5f);
        }
    }

    public bool HandleScrollWheel { get; set; } = true;


    public SleekColor BackgroundColor { get; set; } = GlazierConst.DefaultScrollViewBackgroundColor;


    public SleekColor ForegroundColor { get; set; } = GlazierConst.DefaultScrollViewForegroundColor;


    public float NormalizedVerticalPosition
    {
        get
        {
            if (isTransformDirty)
            {
                UpdateDirtyTransform();
            }
            return state.y / (contentRect.height - drawRect.height);
        }
    }

    public float NormalizedViewportHeight
    {
        get
        {
            if (isTransformDirty)
            {
                UpdateDirtyTransform();
            }
            return drawRect.height / contentRect.height;
        }
    }

    public bool ContentUseManualLayout { get; set; } = true;


    public bool AlignContentToBottom { get; set; }

    public bool IsRaycastTarget
    {
        get
        {
            return _isRaycastTarget;
        }
        set
        {
            _isRaycastTarget = value;
        }
    }

    public event Action<Vector2> OnNormalizedValueChanged;

    public void ScrollToTop()
    {
        state = new Vector2(state.x, state.y);
    }

    public void ScrollToBottom()
    {
        state = new Vector2(state.x, contentRect.height);
    }

    public override void OnGUI()
    {
        GUI.backgroundColor = BackgroundColor;
        Vector2 vector = GUI.BeginScrollView(drawRect, state, viewRect);
        if (state != vector)
        {
            state = vector;
            if (this.OnNormalizedValueChanged != null)
            {
                Vector2 obj = new Vector2(state.x / (contentRect.width - drawRect.width), state.y / (contentRect.height - drawRect.height));
                this.OnNormalizedValueChanged(obj);
            }
        }
        ChildrenOnGUI();
        GUI.EndScrollView(HandleScrollWheel);
    }

    protected override void TransformChildDrawPositionIntoParentSpace(ref Vector2 position)
    {
        position.x += drawRect.x;
        position.x -= state.x;
        position.y += drawRect.y;
        position.y -= state.y;
    }

    protected override Rect GetLayoutRect()
    {
        return contentRect;
    }

    protected override void UpdateDirtyTransform()
    {
        base.UpdateDirtyTransform();
        float userInterfaceScale = GraphicsSettings.userInterfaceScale;
        contentRect.width = ContentSizeOffset.x * userInterfaceScale;
        contentRect.height = ContentSizeOffset.y * userInterfaceScale;
        if (ScaleContentToWidth)
        {
            contentRect.width += drawRect.width * ContentScaleFactor;
        }
        if (ScaleContentToHeight)
        {
            contentRect.height += drawRect.height * ContentScaleFactor;
        }
        bool num = contentRect.height >= drawRect.height;
        if (num && ReduceWidthWhenScrollbarVisible && ScaleContentToWidth)
        {
            contentRect.width -= 30f;
        }
        if (contentRect.width >= drawRect.width && ScaleContentToHeight)
        {
            contentRect.height -= 30f;
        }
        viewRect = contentRect;
        if (num && !ReduceWidthWhenScrollbarVisible && ScaleContentToWidth)
        {
            viewRect.width -= 30f;
        }
    }
}
