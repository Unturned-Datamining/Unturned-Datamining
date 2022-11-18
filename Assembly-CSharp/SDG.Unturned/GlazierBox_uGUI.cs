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

    public string text
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

    public string tooltipText
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

    public FontStyle fontStyle
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

    public TextAnchor fontAlignment
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

    public ESleekFontSize fontSize
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

    public ETextContrastContext shadowStyle
    {
        get
        {
            return _contrastContext;
        }
        set
        {
            _contrastContext = value;
            ETextContrastStyle eTextContrastStyle = SleekShadowStyle.ContextToStyle(value);
            textComponent.fontSharedMaterial = base.glazier.GetFontMaterial(eTextContrastStyle);
            textComponent.characterSpacing = GlazierUtils_uGUI.GetCharacterSpacing(eTextContrastStyle);
        }
    }

    public SleekColor textColor
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

    public bool enableRichText
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

    public SleekColor backgroundColor
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
        fontAlignment = TextAnchor.MiddleCenter;
        fontSize = ESleekFontSize.Default;
        shadowStyle = ETextContrastContext.Default;
        fontStyle = FontStyle.Normal;
        enableRichText = false;
    }

    protected override bool ReleaseIntoPool()
    {
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
