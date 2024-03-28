using System;
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

internal class GlazierScrollView_uGUI : GlazierElementBase_uGUI, ISleekScrollView, ISleekElement
{
    private bool _scaleContentToWidth;

    private bool _scaleContentToHeight;

    private float _contentScaleFactor = 1f;

    private bool _reduceWidthWhenScrollbarVisible = true;

    private ESleekScrollbarVisibility _verticalScrollbarVisibility;

    private SleekColor _backgroundColor;

    private SleekColor _foregroundColor;

    protected bool _contentUseManualLayout = true;

    protected bool _alignContentToBottom;

    private ScrollRectEx scrollRectComponent;

    private RectTransform contentTransform;

    private Image contentImage;

    private Image horizontalScrollbarBackgroundImage;

    private Image horizontalScrollbarHandleImage;

    private Scrollbar horizontalScrollbarComponent;

    private Image verticalScrollbarBackgroundImage;

    private Image verticalScrollbarHandleImage;

    private Scrollbar verticalScrollbarComponent;

    public bool ScaleContentToWidth
    {
        get
        {
            return _scaleContentToWidth;
        }
        set
        {
            _scaleContentToWidth = value;
            contentTransform.anchorMax = new Vector2(_scaleContentToWidth ? _contentScaleFactor : 0f, 1f);
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
            contentTransform.anchorMin = new Vector2(0f, _scaleContentToHeight ? (1f - _contentScaleFactor) : 1f);
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
            contentTransform.anchorMin = new Vector2(0f, _scaleContentToHeight ? (1f - _contentScaleFactor) : 1f);
            contentTransform.anchorMax = new Vector2(_scaleContentToWidth ? ContentScaleFactor : 0f, 1f);
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
            scrollRectComponent.verticalScrollbarVisibility = ((!value) ? ScrollRect.ScrollbarVisibility.AutoHide : ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport);
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
            if (_verticalScrollbarVisibility != value)
            {
                _verticalScrollbarVisibility = value;
                verticalScrollbarBackgroundImage.gameObject.SetActive(_verticalScrollbarVisibility != ESleekScrollbarVisibility.Hidden);
                verticalScrollbarHandleImage.gameObject.SetActive(_verticalScrollbarVisibility != ESleekScrollbarVisibility.Hidden);
            }
        }
    }

    public Vector2 ContentSizeOffset
    {
        get
        {
            return contentTransform.sizeDelta;
        }
        set
        {
            contentTransform.sizeDelta = value;
            scrollRectComponent.Rebuild(CanvasUpdate.PostLayout);
            ClampScrollBars();
        }
    }

    public Vector2 NormalizedStateCenter
    {
        get
        {
            Rect absoluteRect = scrollRectComponent.viewport.GetAbsoluteRect();
            Rect absoluteRect2 = contentTransform.GetAbsoluteRect();
            Vector2 center = absoluteRect.center;
            return new Vector2((center.x - absoluteRect2.xMin) / absoluteRect2.width, (center.y - absoluteRect2.yMin) / absoluteRect2.height);
        }
        set
        {
            Rect absoluteRect = scrollRectComponent.viewport.GetAbsoluteRect();
            Rect absoluteRect2 = contentTransform.GetAbsoluteRect();
            contentTransform.anchoredPosition = new Vector2(value.x * (0f - absoluteRect2.width) + absoluteRect.width * 0.5f, value.y * absoluteRect2.height - absoluteRect.height * 0.5f);
        }
    }

    public bool HandleScrollWheel
    {
        get
        {
            return scrollRectComponent.HandleScrollWheel;
        }
        set
        {
            scrollRectComponent.HandleScrollWheel = value;
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
            horizontalScrollbarBackgroundImage.color = _backgroundColor;
            verticalScrollbarBackgroundImage.color = _backgroundColor;
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
            horizontalScrollbarHandleImage.color = _foregroundColor;
            verticalScrollbarHandleImage.color = _foregroundColor;
        }
    }

    public float NormalizedVerticalPosition => 1f - scrollRectComponent.verticalNormalizedPosition;

    public float NormalizedViewportHeight => scrollRectComponent.verticalScrollbar.size;

    public bool ContentUseManualLayout
    {
        get
        {
            return _contentUseManualLayout;
        }
        set
        {
            if (_contentUseManualLayout != value)
            {
                _contentUseManualLayout = value;
                if (_contentUseManualLayout)
                {
                    contentTransform.DestroyComponentIfExists<VerticalLayoutGroup>();
                    contentTransform.DestroyComponentIfExists<ContentSizeFitter>();
                }
                else
                {
                    contentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
                    contentTransform.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }
            }
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
            _alignContentToBottom = value;
            contentTransform.pivot = new Vector2(0f, _alignContentToBottom ? 0f : 1f);
        }
    }

    public bool IsRaycastTarget
    {
        get
        {
            return contentImage.raycastTarget;
        }
        set
        {
            contentImage.raycastTarget = value;
        }
    }

    public override RectTransform AttachmentTransform => contentTransform;

    public event Action<Vector2> OnNormalizedValueChanged;

    public void ScrollToTop()
    {
        scrollRectComponent.verticalNormalizedPosition = 1f;
    }

    public void ScrollToBottom()
    {
        scrollRectComponent.verticalNormalizedPosition = 0f;
    }

    public GlazierScrollView_uGUI(Glazier_uGUI glazier)
        : base(glazier)
    {
    }

    public override void ConstructNew()
    {
        base.ConstructNew();
        scrollRectComponent = base.gameObject.AddComponent<ScrollRectEx>();
        scrollRectComponent.movementType = ScrollRect.MovementType.Clamped;
        scrollRectComponent.inertia = false;
        scrollRectComponent.onValueChanged.AddListener(OnUnityValueChanged);
        scrollRectComponent.scrollSensitivity = 40f * GlazierBase.ScrollViewSensitivityMultiplier;
        GameObject obj = new GameObject("Viewport", typeof(RectTransform));
        RectTransform rectTransform = obj.GetRectTransform();
        rectTransform.SetParent(base.transform, worldPositionStays: false);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        obj.AddComponent<RectMask2D>();
        scrollRectComponent.viewport = rectTransform;
        GameObject gameObject = new GameObject("Content", typeof(RectTransform));
        contentTransform = gameObject.GetRectTransform();
        contentTransform.SetParent(rectTransform, worldPositionStays: false);
        contentTransform.pivot = new Vector2(0f, 1f);
        contentTransform.anchorMin = new Vector2(0f, 1f);
        contentTransform.anchorMax = new Vector2(0f, 1f);
        contentTransform.anchoredPosition = Vector2.zero;
        contentTransform.sizeDelta = Vector2.zero;
        scrollRectComponent.content = contentTransform;
        contentImage = gameObject.AddComponent<Image>();
        contentImage.color = new Color(1f, 1f, 1f, 0f);
        GameObject gameObject2 = new GameObject("Horizontal Scrollbar", typeof(RectTransform));
        RectTransform rectTransform2 = gameObject2.GetRectTransform();
        rectTransform2.SetParent(base.transform, worldPositionStays: false);
        rectTransform2.pivot = new Vector2(0f, 0f);
        rectTransform2.anchorMin = new Vector2(0f, 0f);
        rectTransform2.anchorMax = new Vector2(1f, 0f);
        rectTransform2.anchoredPosition = Vector2.zero;
        rectTransform2.sizeDelta = new Vector2(0f, 20f);
        GameObject gameObject3 = new GameObject("Background", typeof(RectTransform));
        RectTransform rectTransform3 = gameObject3.GetRectTransform();
        rectTransform3.SetParent(rectTransform2, worldPositionStays: false);
        rectTransform3.anchorMin = new Vector2(0f, 0.5f);
        rectTransform3.anchorMax = new Vector2(1f, 0.5f);
        rectTransform3.anchoredPosition = Vector2.zero;
        rectTransform3.sizeDelta = new Vector2(-20f, 6f);
        horizontalScrollbarBackgroundImage = gameObject3.AddComponent<Image>();
        horizontalScrollbarBackgroundImage.type = Image.Type.Sliced;
        horizontalScrollbarBackgroundImage.raycastTarget = true;
        RectTransform rectTransform4 = new GameObject("Handle Padding", typeof(RectTransform)).GetRectTransform();
        rectTransform4.SetParent(rectTransform2, worldPositionStays: false);
        rectTransform4.anchorMin = Vector2.zero;
        rectTransform4.anchorMax = Vector2.one;
        rectTransform4.anchoredPosition = Vector2.zero;
        rectTransform4.sizeDelta = new Vector2(-20f, 0f);
        GameObject gameObject4 = new GameObject("Handle", typeof(RectTransform));
        RectTransform rectTransform5 = gameObject4.GetRectTransform();
        rectTransform5.SetParent(rectTransform4, worldPositionStays: false);
        rectTransform5.anchoredPosition = Vector2.zero;
        rectTransform5.sizeDelta = new Vector2(20f, 0f);
        horizontalScrollbarHandleImage = gameObject4.AddComponent<Image>();
        horizontalScrollbarHandleImage.type = Image.Type.Sliced;
        horizontalScrollbarHandleImage.raycastTarget = true;
        horizontalScrollbarComponent = gameObject2.AddComponent<Scrollbar>();
        horizontalScrollbarComponent.SetDirection(Scrollbar.Direction.LeftToRight, includeRectLayouts: false);
        horizontalScrollbarComponent.handleRect = rectTransform5;
        horizontalScrollbarComponent.transition = Selectable.Transition.SpriteSwap;
        horizontalScrollbarComponent.targetGraphic = horizontalScrollbarHandleImage;
        scrollRectComponent.horizontalScrollbarSpacing = 10f;
        scrollRectComponent.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRectComponent.horizontalScrollbar = horizontalScrollbarComponent;
        GameObject gameObject5 = new GameObject("Vertical Scrollbar", typeof(RectTransform));
        RectTransform rectTransform6 = gameObject5.GetRectTransform();
        rectTransform6.SetParent(base.transform, worldPositionStays: false);
        rectTransform6.pivot = new Vector2(1f, 1f);
        rectTransform6.anchorMin = new Vector2(1f, 0f);
        rectTransform6.anchorMax = new Vector2(1f, 1f);
        rectTransform6.anchoredPosition = Vector2.zero;
        rectTransform6.sizeDelta = new Vector2(20f, 0f);
        GameObject gameObject6 = new GameObject("Background", typeof(RectTransform));
        RectTransform rectTransform7 = gameObject6.GetRectTransform();
        rectTransform7.SetParent(rectTransform6, worldPositionStays: false);
        rectTransform7.anchorMin = new Vector2(0.5f, 0f);
        rectTransform7.anchorMax = new Vector2(0.5f, 1f);
        rectTransform7.anchoredPosition = Vector2.zero;
        rectTransform7.sizeDelta = new Vector2(6f, -20f);
        verticalScrollbarBackgroundImage = gameObject6.AddComponent<Image>();
        verticalScrollbarBackgroundImage.type = Image.Type.Sliced;
        verticalScrollbarBackgroundImage.raycastTarget = true;
        RectTransform rectTransform8 = new GameObject("Handle Padding", typeof(RectTransform)).GetRectTransform();
        rectTransform8.SetParent(rectTransform6, worldPositionStays: false);
        rectTransform8.anchorMin = Vector2.zero;
        rectTransform8.anchorMax = Vector2.one;
        rectTransform8.anchoredPosition = Vector2.zero;
        rectTransform8.sizeDelta = new Vector2(0f, -20f);
        GameObject gameObject7 = new GameObject("Handle", typeof(RectTransform));
        RectTransform rectTransform9 = gameObject7.GetRectTransform();
        rectTransform9.SetParent(rectTransform8, worldPositionStays: false);
        rectTransform9.anchoredPosition = Vector2.zero;
        rectTransform9.sizeDelta = new Vector2(0f, 20f);
        verticalScrollbarHandleImage = gameObject7.AddComponent<Image>();
        verticalScrollbarHandleImage.type = Image.Type.Sliced;
        verticalScrollbarHandleImage.raycastTarget = true;
        verticalScrollbarComponent = gameObject5.AddComponent<Scrollbar>();
        verticalScrollbarComponent.SetDirection(Scrollbar.Direction.BottomToTop, includeRectLayouts: false);
        verticalScrollbarComponent.handleRect = rectTransform9;
        verticalScrollbarComponent.transition = Selectable.Transition.SpriteSwap;
        verticalScrollbarComponent.targetGraphic = verticalScrollbarHandleImage;
        scrollRectComponent.verticalScrollbarSpacing = 10f;
        scrollRectComponent.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRectComponent.verticalScrollbar = verticalScrollbarComponent;
        HandleScrollWheel = true;
        _backgroundColor = GlazierConst.DefaultScrollViewBackgroundColor;
        _foregroundColor = GlazierConst.DefaultScrollViewForegroundColor;
    }

    public override void SynchronizeColors()
    {
        horizontalScrollbarBackgroundImage.color = _backgroundColor;
        horizontalScrollbarHandleImage.color = _foregroundColor;
        verticalScrollbarBackgroundImage.color = _backgroundColor;
        verticalScrollbarHandleImage.color = _foregroundColor;
    }

    public override void SynchronizeTheme()
    {
        SpriteState spriteState = default(SpriteState);
        spriteState.disabledSprite = GlazierResources_uGUI.Theme.BoxSprite;
        spriteState.highlightedSprite = GlazierResources_uGUI.Theme.BoxHighlightedSprite;
        spriteState.selectedSprite = GlazierResources_uGUI.Theme.BoxSelectedSprite;
        spriteState.pressedSprite = GlazierResources_uGUI.Theme.BoxPressedSprite;
        horizontalScrollbarBackgroundImage.sprite = GlazierResources_uGUI.Theme.SliderBackgroundSprite;
        horizontalScrollbarHandleImage.sprite = GlazierResources_uGUI.Theme.BoxSprite;
        horizontalScrollbarComponent.spriteState = spriteState;
        verticalScrollbarBackgroundImage.sprite = GlazierResources_uGUI.Theme.SliderBackgroundSprite;
        verticalScrollbarHandleImage.sprite = GlazierResources_uGUI.Theme.BoxSprite;
        verticalScrollbarComponent.spriteState = spriteState;
    }

    private void ClampScrollBars()
    {
        Vector2 normalizedPosition = scrollRectComponent.normalizedPosition;
        normalizedPosition = MathfEx.Clamp01(normalizedPosition);
        scrollRectComponent.normalizedPosition = normalizedPosition;
    }

    private void OnUnityValueChanged(Vector2 value)
    {
        value.y = 1f - value.y;
        this.OnNormalizedValueChanged?.Invoke(value);
    }
}
