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

    private Rect contentRect;

    private Rect viewRect;

    public bool scaleContentToWidth
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

    public bool scaleContentToHeight
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

    public float contentScaleFactor
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

    public bool reduceWidthWhenScrollbarVisible
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

    public Vector2 contentSizeOffset
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

    public Vector2 state { get; set; }

    public Vector2 normalizedStateCenter
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

    public bool handleScrollWheel { get; set; } = true;


    public SleekColor backgroundColor { get; set; } = GlazierConst.DefaultScrollViewBackgroundColor;


    public SleekColor foregroundColor { get; set; } = GlazierConst.DefaultScrollViewForegroundColor;


    public float normalizedVerticalPosition
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

    public float normalizedViewportHeight
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

    public event Action<Vector2> onValueChanged;

    public void ScrollToBottom()
    {
        state = new Vector2(state.x, contentRect.height);
    }

    public override void OnGUI()
    {
        GUI.backgroundColor = backgroundColor;
        Vector2 vector = GUI.BeginScrollView(drawRect, state, viewRect);
        if (state != vector)
        {
            state = vector;
            if (this.onValueChanged != null)
            {
                Vector2 obj = new Vector2(state.x / (contentRect.width - drawRect.width), state.y / (contentRect.height - drawRect.height));
                this.onValueChanged(obj);
            }
        }
        ChildrenOnGUI();
        GUI.EndScrollView(handleScrollWheel);
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
        contentRect.width = contentSizeOffset.x * userInterfaceScale;
        contentRect.height = contentSizeOffset.y * userInterfaceScale;
        if (scaleContentToWidth)
        {
            contentRect.width += drawRect.width * contentScaleFactor;
        }
        if (scaleContentToHeight)
        {
            contentRect.height += drawRect.height * contentScaleFactor;
        }
        bool num = contentRect.height >= drawRect.height;
        if (num && reduceWidthWhenScrollbarVisible && scaleContentToWidth)
        {
            contentRect.width -= 30f;
        }
        if (contentRect.width >= drawRect.width && scaleContentToHeight)
        {
            contentRect.height -= 30f;
        }
        viewRect = contentRect;
        if (num && !reduceWidthWhenScrollbarVisible && scaleContentToWidth)
        {
            viewRect.width -= 30f;
        }
    }
}
