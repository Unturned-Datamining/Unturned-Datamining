using System;
using UnityEngine;

namespace SDG.Unturned;

public class SleekTagButton : SleekWrapper
{
    private CachingAssetRef _tagRef;

    private SleekButtonIcon iconButton;

    public CachingAssetRef TagRef
    {
        get
        {
            return _tagRef;
        }
        set
        {
            if (_tagRef != value)
            {
                _tagRef = value;
                TagAsset tagAsset = _tagRef.Get<TagAsset>();
                if (tagAsset != null)
                {
                    iconButton.icon = tagAsset.Icon;
                    iconButton.iconColor = (tagAsset.ShouldTintIcon ? ESleekTint.FOREGROUND : ESleekTint.NONE);
                    iconButton.textColor = tagAsset.NameColorOrPreferredFontColor;
                    iconButton.tooltip = tagAsset.PlainTextName;
                    iconButton.text = (EnableLabel ? tagAsset.PlainTextName : string.Empty);
                }
                else
                {
                    iconButton.icon = null;
                    iconButton.tooltip = string.Empty;
                    iconButton.text = string.Empty;
                }
            }
        }
    }

    public bool EnableLabel { get; set; }

    public event Action<CachingAssetRef> OnClicked;

    public SleekTagButton()
    {
        iconButton = new SleekButtonIcon(null, 40);
        iconButton.SizeScale_X = 1f;
        iconButton.SizeScale_Y = 1f;
        iconButton.iconColor = ESleekTint.FOREGROUND;
        iconButton.onClickedButton += OnClickedInternalButton;
        iconButton.TextAlignment = TextAnchor.MiddleLeft;
        iconButton.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        AddChild(iconButton);
    }

    private void OnClickedInternalButton(ISleekElement internalButton)
    {
        this.OnClicked?.Invoke(_tagRef);
    }
}
