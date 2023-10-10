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

    public ESleekOrientation Orientation
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

    public float Value
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

    public SleekColor BackgroundColor
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

    public SleekColor ForegroundColor
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

    public bool IsInteractable
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

    public event Dragged OnValueChanged;

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
        if (!IsInteractable)
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
        switch (Orientation)
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
        this.OnValueChanged?.Invoke(this, value);
    }
}
