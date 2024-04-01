using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

internal class GlazierBox_uGUI : GlazierElementBase_uGUI, ISleekBox, ISleekLabel, ISleekElement, ISleekWithTooltip
{
    public class BoxPoolData : PoolData
    {
        public Image imageComponent;

        public TextMeshProUGUI textComponent;
    }

    private GlazieruGUITooltip tooltipComponent;

    private FontStyle _fontStyle;

    private TextAnchor _fontAlignment;

    private ESleekFontSize _fontSize;

    private ETextContrastContext _contrastContext;

    private SleekColor _textColor = GlazierConst.DefaultBoxForegroundColor;

    private SleekColor _backgroundColor = GlazierConst.DefaultBoxBackgroundColor;

    private Image imageComponent;

    private TextMeshProUGUI textComponent;

    public string Text
    {
        get
        {
            return textComponent.text;
        }
        set
        {
            textComponent.text = value;
        }
    }

    public string TooltipText
    {
        get
        {
            if (!(tooltipComponent != null))
            {
                return null;
            }
            return tooltipComponent.text;
        }
        set
        {
            if (tooltipComponent == null)
            {
                tooltipComponent = base.gameObject.AddComponent<GlazieruGUITooltip>();
                tooltipComponent.color = _textColor;
            }
            tooltipComponent.text = value;
        }
    }

    public FontStyle FontStyle
    {
        get
        {
            return _fontStyle;
        }
        set
        {
            _fontStyle = value;
            textComponent.fontStyle = GlazierUtils_uGUI.GetFontStyleFlags(_fontStyle);
        }
    }

    public TextAnchor TextAlignment
    {
        get
        {
            return _fontAlignment;
        }
        set
        {
            _fontAlignment = value;
            textComponent.alignment = GlazierUtils_uGUI.TextAnchorToTMP(_fontAlignment);
        }
    }

    public ESleekFontSize FontSize
    {
        get
        {
            return _fontSize;
        }
        set
        {
            _fontSize = value;
            textComponent.fontSize = GlazierUtils_uGUI.GetFontSize(_fontSize);
        }
    }

    public ETextContrastContext TextContrastContext
    {
        get
        {
            return _contrastContext;
        }
        set
        {
            _contrastContext = value;
            ETextContrastStyle shadowStyle = SleekShadowStyle.ContextToStyle(value);
            textComponent.fontSharedMaterial = base.glazier.GetFontMaterial(shadowStyle);
            textComponent.characterSpacing = GlazierUtils_uGUI.GetCharacterSpacing(shadowStyle);
        }
    }

    public SleekColor TextColor
    {
        get
        {
            return _textColor;
        }
        set
        {
            _textColor = value;
            textComponent.color = _textColor;
            if (tooltipComponent != null)
            {
                tooltipComponent.color = _textColor;
            }
        }
    }

    public bool AllowRichText
    {
        get
        {
            return textComponent.richText;
        }
        set
        {
            textComponent.richText = value;
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
            imageComponent.color = _backgroundColor;
        }
    }

    private void PostConstructBox()
    {
        TextAlignment = TextAnchor.MiddleCenter;
        FontSize = ESleekFontSize.Default;
        TextContrastContext = ETextContrastContext.Default;
        FontStyle = FontStyle.Normal;
        AllowRichText = false;
    }

    protected override bool ReleaseIntoPool()
    {
        if (imageComponent == null || textComponent == null)
        {
            return false;
        }
        if (tooltipComponent != null)
        {
            Object.Destroy(tooltipComponent);
            tooltipComponent = null;
        }
        imageComponent.enabled = false;
        textComponent.enabled = false;
        BoxPoolData boxPoolData = new BoxPoolData();
        PopulateBasePoolData(boxPoolData);
        boxPoolData.imageComponent = imageComponent;
        imageComponent = null;
        boxPoolData.textComponent = textComponent;
        textComponent = null;
        base.glazier.ReleaseBoxToPool(boxPoolData);
        return true;
    }

    protected override void EnableComponents()
    {
        imageComponent.enabled = true;
        textComponent.enabled = true;
    }

    public GlazierBox_uGUI(Glazier_uGUI glazier)
        : base(glazier)
    {
    }

    public override void ConstructNew()
    {
        base.ConstructNew();
        imageComponent = base.gameObject.AddComponent<Image>();
        imageComponent.enabled = false;
        imageComponent.type = Image.Type.Sliced;
        imageComponent.raycastTarget = true;
        GameObject gameObject = new GameObject("BoxText", typeof(RectTransform));
        gameObject.transform.SetParent(base.transform, worldPositionStays: false);
        gameObject.GetRectTransform().reset();
        textComponent = gameObject.AddComponent<TextMeshProUGUI>();
        textComponent.enabled = false;
        textComponent.raycastTarget = false;
        textComponent.font = GlazierResources_uGUI.Font;
        textComponent.overflowMode = TextOverflowModes.Truncate;
        textComponent.margin = GlazierConst_uGUI.DefaultTextMargin;
        textComponent.extraPadding = true;
        PostConstructBox();
    }

    public void ConstructFromBoxPool(BoxPoolData poolData)
    {
        ConstructFromPool(poolData);
        imageComponent = poolData.imageComponent;
        textComponent = poolData.textComponent;
        textComponent.rectTransform.reset();
        textComponent.text = string.Empty;
        PostConstructBox();
    }

    public override void SynchronizeColors()
    {
        imageComponent.color = _backgroundColor;
        textComponent.color = _textColor;
        if (tooltipComponent != null)
        {
            tooltipComponent.color = _textColor;
        }
    }

    public override void SynchronizeTheme()
    {
        imageComponent.sprite = GlazierResources_uGUI.Theme.BoxSprite;
    }
}
