using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned;

internal class GlazierScrollView_UIToolkit : GlazierElementBase_UIToolkit, ISleekScrollView, ISleekElement
{
    private bool _scaleContentToWidth;

    private bool _scaleContentToHeight;

    private float _contentScaleFactor = 1f;

    private bool _reduceWidthWhenScrollbarVisible = true;

    private ESleekScrollbarVisibility _verticalScrollbarVisibility;

    private Vector2 _contentSizeOffset;

    private const float MOUSE_WHEEL_SCROLL_SIZE = 600f;

    private bool _handleScrollWheel = true;

    private SleekColor _backgroundColor = GlazierConst.DefaultScrollViewBackgroundColor;

    private SleekColor _foregroundColor = GlazierConst.DefaultScrollViewForegroundColor;

    protected bool _contentUseManualLayout = true;

    protected bool _alignContentToBottom;

    private bool _isRaycastTarget = true;

    private ScrollView control;

    private VisualElement contentViewport;

    private VisualElement contentContainer;

    private VisualElement horizontalTracker;

    private VisualElement horizontalDragger;

    private VisualElement verticalTracker;

    private VisualElement verticalDragger;

    private bool wantsToScrollToBottom;

    public bool ScaleContentToWidth
    {
        get
        {
            return _scaleContentToWidth;
        }
        set
        {
            _scaleContentToWidth = value;
            SynchronizeContentContainerStyle();
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
            SynchronizeContentContainerStyle();
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
            SynchronizeContentContainerStyle();
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
        }
    }

    public ESleekScrollbarVisibility VerticalScrollbarVisibility
    {
        get
        {
            return _verticalScrollbarVisibility;
        }
        set
        {
            _verticalScrollbarVisibility = value;
            control.verticalScrollerVisibility = ((_verticalScrollbarVisibility == ESleekScrollbarVisibility.Hidden) ? ScrollerVisibility.Hidden : ScrollerVisibility.Auto);
        }
    }

    public Vector2 ContentSizeOffset
    {
        get
        {
            return _contentSizeOffset;
        }
        set
        {
            _contentSizeOffset = value;
            SynchronizeContentContainerStyle();
        }
    }

    public Vector2 NormalizedStateCenter
    {
        get
        {
            return new Vector2(NormalizedHorizontalPosition + NormalizedViewportWidth * 0.5f, NormalizedVerticalPosition + NormalizedViewportHeight * 0.5f);
        }
        set
        {
            NormalizedHorizontalPosition = value.x - NormalizedViewportWidth * 0.5f;
            NormalizedVerticalPosition = value.y - NormalizedViewportHeight * 0.5f;
        }
    }

    public bool HandleScrollWheel
    {
        get
        {
            return false;
        }
        set
        {
            _handleScrollWheel = value;
            control.mouseWheelScrollSize = (_handleScrollWheel ? (600f * GlazierBase.ScrollViewSensitivityMultiplier) : 0f);
        }
    }

    public SleekColor BackgroundColor
    {
        get
        {
            return _backgroundColor;
        }
        set
        {
            _backgroundColor = value;
            horizontalTracker.style.unityBackgroundImageTintColor = _backgroundColor;
            verticalTracker.style.unityBackgroundImageTintColor = _backgroundColor;
        }
    }

    public SleekColor ForegroundColor
    {
        get
        {
            return _foregroundColor;
        }
        set
        {
            _foregroundColor = value;
            horizontalDragger.style.unityBackgroundImageTintColor = _foregroundColor;
            verticalDragger.style.unityBackgroundImageTintColor = _foregroundColor;
        }
    }

    private float NormalizedHorizontalPosition
    {
        get
        {
            Scroller horizontalScroller = control.horizontalScroller;
            return Mathf.InverseLerp(horizontalScroller.lowValue, horizontalScroller.highValue, horizontalScroller.value);
        }
        set
        {
            Scroller horizontalScroller = control.horizontalScroller;
            horizontalScroller.value = Mathf.Lerp(horizontalScroller.lowValue, horizontalScroller.highValue, value);
        }
    }

    public float NormalizedVerticalPosition
    {
        get
        {
            Scroller verticalScroller = control.verticalScroller;
            return Mathf.InverseLerp(verticalScroller.lowValue, verticalScroller.highValue, verticalScroller.value);
        }
        private set
        {
            Scroller verticalScroller = control.verticalScroller;
            verticalScroller.value = Mathf.Lerp(verticalScroller.lowValue, verticalScroller.highValue, value);
        }
    }

    private float NormalizedViewportWidth => control.contentViewport.layout.width / control.contentContainer.localBound.width;

    public float NormalizedViewportHeight => control.contentViewport.layout.height / control.contentContainer.localBound.height;

    public bool ContentUseManualLayout
    {
        get
        {
            return _contentUseManualLayout;
        }
        set
        {
            _contentUseManualLayout = value;
            SynchronizeContentContainerStyle();
        }
    }

    public bool AlignContentToBottom
    {
        get
        {
            return _alignContentToBottom;
        }
        set
        {
            if (_alignContentToBottom != value)
            {
                _alignContentToBottom = value;
                contentViewport.style.justifyContent = (_alignContentToBottom ? ((StyleEnum<Justify>)Justify.FlexEnd) : ((StyleEnum<Justify>)StyleKeyword.Null));
            }
        }
    }

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
        control.verticalScroller.value = control.verticalScroller.lowValue;
        wantsToScrollToBottom = false;
    }

    public void ScrollToBottom()
    {
        wantsToScrollToBottom = true;
    }

    public GlazierScrollView_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
        control = new ScrollView();
        control.userData = this;
        control.AddToClassList("unturned-scroll-view");
        control.horizontalScroller.valueChanged += OnHorizontalValueChanged;
        control.verticalScroller.valueChanged += OnVerticalValueChanged;
        control.mouseWheelScrollSize = 600f * GlazierBase.ScrollViewSensitivityMultiplier;
        contentViewport = control.contentViewport;
        contentContainer = control.contentContainer;
        control.pickingMode = PickingMode.Ignore;
        control.Q(null, ScrollView.contentAndVerticalScrollUssClassName).pickingMode = PickingMode.Ignore;
        contentViewport.pickingMode = PickingMode.Ignore;
        VisualElement e = control.horizontalScroller.Q(null, "unity-base-slider__input").Q(null, "unity-base-slider__drag-container");
        horizontalTracker = e.Q(null, "unity-base-slider__tracker");
        horizontalDragger = e.Q(null, "unity-base-slider__dragger");
        VisualElement e2 = control.verticalScroller.Q(null, "unity-base-slider__input").Q(null, "unity-base-slider__drag-container");
        verticalTracker = e2.Q(null, "unity-base-slider__tracker");
        verticalDragger = e2.Q(null, "unity-base-slider__dragger");
        visualElement = control;
    }

    public override void Update()
    {
        base.Update();
        if (wantsToScrollToBottom)
        {
            wantsToScrollToBottom = false;
            control.verticalScroller.value = control.verticalScroller.highValue;
        }
    }

    internal override void SynchronizeColors()
    {
        horizontalTracker.style.unityBackgroundImageTintColor = _backgroundColor;
        horizontalDragger.style.unityBackgroundImageTintColor = _foregroundColor;
        verticalTracker.style.unityBackgroundImageTintColor = _backgroundColor;
        verticalDragger.style.unityBackgroundImageTintColor = _foregroundColor;
    }

    private void OnHorizontalValueChanged(float value)
    {
        this.OnNormalizedValueChanged?.Invoke(new Vector2(NormalizedHorizontalPosition, NormalizedVerticalPosition));
    }

    private void OnVerticalValueChanged(float value)
    {
        this.OnNormalizedValueChanged?.Invoke(new Vector2(NormalizedHorizontalPosition, NormalizedVerticalPosition));
    }

    private void SynchronizeContentContainerStyle()
    {
        if (_contentUseManualLayout)
        {
            contentContainer.style.position = StyleKeyword.Null;
            float num = (ContentScaleFactor - 1f) * 100f;
            contentContainer.style.right = Length.Percent(_scaleContentToWidth ? (0f - num) : 100f);
            contentContainer.style.bottom = Length.Percent(_scaleContentToHeight ? (0f - num) : 100f);
            contentContainer.style.marginRight = 0f - _contentSizeOffset.x;
            contentContainer.style.marginBottom = 0f - _contentSizeOffset.y;
        }
        else
        {
            contentContainer.style.position = Position.Relative;
            contentContainer.style.right = StyleKeyword.Initial;
            contentContainer.style.bottom = StyleKeyword.Initial;
            contentContainer.style.marginRight = StyleKeyword.Initial;
            contentContainer.style.marginBottom = StyleKeyword.Initial;
        }
    }
}
