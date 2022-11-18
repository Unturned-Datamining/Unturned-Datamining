using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

internal class GlazierSlider_uGUI : GlazierElementBase_uGUI, ISleekSlider, ISleekElement
{
    private ESleekOrientation _orientation = ESleekOrientation.VERTICAL;

    private SleekColor _backgroundColor;

    private SleekColor _foregroundColor;

    private Scrollbar scrollbarComponent;

    private Image backgroundImage;

    private Image handleImage;

    private RectTransform backgroundTransform;

    public ESleekOrientation orientation
    {
        get
        {
            return _orientation;
        }
        set
        {
            if (_orientation != value)
            {
                _orientation = value;
                UpdateOrientation();
            }
        }
    }

    public float size
    {
        get
        {
            return scrollbarComponent.size;
        }
        set
        {
            scrollbarComponent.size = value;
        }
    }

    public float state
    {
        get
        {
            return scrollbarComponent.value;
        }
        set
        {
            scrollbarComponent.SetValueWithoutNotify(value);
        }
    }

    public SleekColor backgroundColor
    {
        get
        {
            return _backgroundColor;
        }
        set
        {
            _backgroundColor = value;
            backgroundImage.color = _backgroundColor;
        }
    }

    public SleekColor foregroundColor
    {
        get
        {
            return _foregroundColor;
        }
        set
        {
            _foregroundColor = value;
            handleImage.color = _foregroundColor;
        }
    }

    public bool isInteractable
    {
        get
        {
            return scrollbarComponent.interactable;
        }
        set
        {
            scrollbarComponent.interactable = value;
            SynchronizeColors();
        }
    }

    public event Dragged onDragged;

    public GlazierSlider_uGUI(Glazier_uGUI glazier)
        : base(glazier)
    {
    }

    public override void ConstructNew()
    {
        base.ConstructNew();
        scrollbarComponent = base.gameObject.AddComponent<Scrollbar>();
        scrollbarComponent.onValueChanged.AddListener(OnSliderComponentValueChanged);
        GameObject gameObject = new GameObject("Background", typeof(RectTransform));
        backgroundTransform = gameObject.GetRectTransform();
        backgroundTransform.SetParent(base.transform, worldPositionStays: false);
        backgroundTransform.anchoredPosition = Vector2.zero;
        backgroundImage = gameObject.AddComponent<Image>();
        backgroundImage.type = Image.Type.Sliced;
        backgroundImage.raycastTarget = true;
        GameObject gameObject2 = new GameObject("Handle", typeof(RectTransform));
        RectTransform rectTransform = gameObject2.GetRectTransform();
        rectTransform.SetParent(base.transform, worldPositionStays: false);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        handleImage = gameObject2.AddComponent<Image>();
        handleImage.type = Image.Type.Sliced;
        handleImage.raycastTarget = true;
        scrollbarComponent.handleRect = rectTransform;
        scrollbarComponent.size = 0.25f;
        scrollbarComponent.transition = Selectable.Transition.SpriteSwap;
        scrollbarComponent.targetGraphic = handleImage;
        _orientation = ESleekOrientation.VERTICAL;
        UpdateOrientation();
        _backgroundColor = GlazierConst.DefaultSliderBackgroundColor;
        _foregroundColor = GlazierConst.DefaultSliderForegroundColor;
    }

    public override void SynchronizeColors()
    {
        Color color = _backgroundColor;
        Color color2 = _foregroundColor;
        if (!isInteractable)
        {
            color.a *= 0.25f;
            color2.a *= 0.25f;
        }
        backgroundImage.color = color;
        handleImage.color = color2;
    }

    public override void SynchronizeTheme()
    {
        backgroundImage.sprite = GlazierResources_uGUI.Theme.SliderBackgroundSprite;
        handleImage.sprite = GlazierResources_uGUI.Theme.BoxSprite;
        SpriteState spriteState = default(SpriteState);
        spriteState.disabledSprite = handleImage.sprite;
        spriteState.highlightedSprite = GlazierResources_uGUI.Theme.BoxHighlightedSprite;
        spriteState.selectedSprite = GlazierResources_uGUI.Theme.BoxSelectedSprite;
        spriteState.pressedSprite = GlazierResources_uGUI.Theme.BoxPressedSprite;
        scrollbarComponent.spriteState = spriteState;
    }

    private void UpdateOrientation()
    {
        switch (orientation)
        {
        case ESleekOrientation.HORIZONTAL:
            backgroundTransform.anchorMin = new Vector2(0f, 0.5f);
            backgroundTransform.anchorMax = new Vector2(1f, 0.5f);
            backgroundTransform.sizeDelta = new Vector2(-20f, 6f);
            scrollbarComponent.SetDirection(Scrollbar.Direction.LeftToRight, includeRectLayouts: false);
            break;
        case ESleekOrientation.VERTICAL:
            backgroundTransform.anchorMin = new Vector2(0.5f, 0f);
            backgroundTransform.anchorMax = new Vector2(0.5f, 1f);
            backgroundTransform.sizeDelta = new Vector2(6f, -20f);
            scrollbarComponent.SetDirection(Scrollbar.Direction.TopToBottom, includeRectLayouts: false);
            break;
        }
    }

    private void OnSliderComponentValueChanged(float value)
    {
        this.onDragged?.Invoke(this, value);
    }
}
