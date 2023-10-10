using TMPro;
using UnityEngine;

namespace SDG.Unturned;

internal class GlazierLabel_uGUI : GlazierElementBase_uGUI, ISleekLabel, ISleekElement
{
    public class LabelPoolData : PoolData
    {
        public TextMeshProUGUI textComponent;
    }

    private FontStyle _fontStyle;

    private TextAnchor _fontAlignment;

    private ESleekFontSize _fontSize;

    private ETextContrastContext _contrastContext;

    private SleekColor _textColor = GlazierConst.DefaultLabelForegroundColor;

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

    private void PostConstructLabel()
    {
        TextAlignment = TextAnchor.MiddleCenter;
        FontSize = ESleekFontSize.Default;
        TextContrastContext = ETextContrastContext.Default;
        FontStyle = FontStyle.Normal;
        AllowRichText = false;
    }

    protected override bool ReleaseIntoPool()
    {
        textComponent.enabled = false;
        LabelPoolData labelPoolData = new LabelPoolData();
        PopulateBasePoolData(labelPoolData);
        labelPoolData.textComponent = textComponent;
        textComponent = null;
        base.glazier.ReleaseLabelToPool(labelPoolData);
        return true;
    }

    protected override void EnableComponents()
    {
        textComponent.enabled = true;
    }

    public GlazierLabel_uGUI(Glazier_uGUI glazier)
        : base(glazier)
    {
    }

    public override void ConstructNew()
    {
        base.ConstructNew();
        textComponent = base.gameObject.AddComponent<TextMeshProUGUI>();
        textComponent.raycastTarget = false;
        textComponent.font = GlazierResources_uGUI.Font;
        textComponent.overflowMode = TextOverflowModes.Truncate;
        textComponent.margin = GlazierConst_uGUI.DefaultTextMargin;
        textComponent.extraPadding = true;
        PostConstructLabel();
    }

    public void ConstructFromLabelPool(LabelPoolData poolData)
    {
        ConstructFromPool(poolData);
        textComponent = poolData.textComponent;
        textComponent.text = string.Empty;
        PostConstructLabel();
    }

    public override void SynchronizeColors()
    {
        textComponent.color = _textColor;
    }
}
