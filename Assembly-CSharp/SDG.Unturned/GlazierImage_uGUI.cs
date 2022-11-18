using System;
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

internal class GlazierImage_uGUI : GlazierElementBase_uGUI, ISleekImage, ISleekElement
{
    public class ImagePoolData : PoolData
    {
        public RectTransform pivotTransform;

        public RawImage rawImageComponent;
    }

    private float _angle;

    private bool _isAngled;

    private SleekColor _color = ESleekTint.NONE;

    private RectTransform pivotTransform;

    private Texture desiredTexture;

    private RawImage rawImageComponent;

    private ButtonEx buttonComponent;

    public Texture texture
    {
        get
        {
            return desiredTexture;
        }
        set
        {
            if (desiredTexture != value)
            {
                internalSetTexture(value, shouldDestroyTexture);
            }
        }
    }

    public float angle
    {
        get
        {
            return _angle;
        }
        set
        {
            _angle = value;
            pivotTransform.localRotation = Quaternion.Euler(0f, 0f, 0f - _angle);
        }
    }

    public bool isAngled
    {
        get
        {
            return _isAngled;
        }
        set
        {
            _isAngled = value;
            if (_isAngled)
            {
                pivotTransform.localRotation = Quaternion.Euler(0f, 0f, 0f - _angle);
            }
            else
            {
                pivotTransform.localRotation = Quaternion.identity;
            }
        }
    }

    public bool shouldDestroyTexture { get; set; }

    public SleekColor color
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
            SynchronizeColors();
        }
    }

    private event System.Action _onImageClicked;

    public event System.Action onImageClicked
    {
        add
        {
            if (buttonComponent == null)
            {
                CreateButton();
            }
            _onImageClicked += value;
        }
        remove
        {
            _onImageClicked -= value;
        }
    }

    private event System.Action _onImageRightClicked;

    public event System.Action onImageRightClicked
    {
        add
        {
            if (buttonComponent == null)
            {
                CreateButton();
            }
            _onImageRightClicked += value;
        }
        remove
        {
            _onImageRightClicked -= value;
        }
    }

    public void updateTexture(Texture2D newTexture)
    {
        if (desiredTexture != newTexture)
        {
            internalSetTexture(newTexture, shouldDestroyTexture);
        }
    }

    public void setTextureAndShouldDestroy(Texture2D newTexture, bool newShouldDestroyTexture)
    {
        if (desiredTexture != newTexture || shouldDestroyTexture != newShouldDestroyTexture)
        {
            internalSetTexture(newTexture, newShouldDestroyTexture);
        }
    }

    public override void destroy()
    {
        if (shouldDestroyTexture && desiredTexture != null)
        {
            UnityEngine.Object.Destroy(desiredTexture);
            desiredTexture = null;
        }
        base.destroy();
    }

    protected override bool ReleaseIntoPool()
    {
        if (buttonComponent == null)
        {
            rawImageComponent.enabled = false;
            ImagePoolData imagePoolData = new ImagePoolData();
            PopulateBasePoolData(imagePoolData);
            imagePoolData.pivotTransform = pivotTransform;
            pivotTransform = null;
            imagePoolData.rawImageComponent = rawImageComponent;
            rawImageComponent = null;
            base.glazier.ReleaseImageToPool(imagePoolData);
            return true;
        }
        return false;
    }

    public GlazierImage_uGUI(Glazier_uGUI glazier)
        : base(glazier)
    {
    }

    public override void ConstructNew()
    {
        base.ConstructNew();
        GameObject gameObject = new GameObject("Pivot", typeof(RectTransform));
        pivotTransform = gameObject.GetRectTransform();
        pivotTransform.SetParent(base.transform, worldPositionStays: false);
        pivotTransform.anchorMin = Vector2.zero;
        pivotTransform.anchorMax = Vector2.one;
        pivotTransform.anchoredPosition = Vector2.zero;
        pivotTransform.sizeDelta = Vector2.zero;
        rawImageComponent = gameObject.AddComponent<RawImage>();
        rawImageComponent.enabled = true;
        rawImageComponent.raycastTarget = false;
        rawImageComponent.texture = (Texture2D)GlazierResources.PixelTexture;
    }

    public void ConstructFromImagePool(ImagePoolData poolData)
    {
        ConstructFromPool(poolData);
        pivotTransform = poolData.pivotTransform;
        rawImageComponent = poolData.rawImageComponent;
        pivotTransform.localRotation = Quaternion.identity;
        rawImageComponent.texture = (Texture2D)GlazierResources.PixelTexture;
    }

    public override void SynchronizeColors()
    {
        if (desiredTexture != null)
        {
            rawImageComponent.color = _color;
            rawImageComponent.enabled = true;
        }
        else if (rawImageComponent.raycastTarget)
        {
            rawImageComponent.color = ColorEx.BlackZeroAlpha;
            rawImageComponent.enabled = true;
        }
        else
        {
            rawImageComponent.enabled = false;
        }
    }

    protected override void EnableComponents()
    {
        rawImageComponent.enabled = desiredTexture != null || rawImageComponent.raycastTarget;
    }

    private void CreateButton()
    {
        rawImageComponent.raycastTarget = true;
        buttonComponent = base.gameObject.AddComponent<ButtonEx>();
        buttonComponent.transition = Selectable.Transition.None;
        buttonComponent.onClick.AddListener(OnUnityClick);
        buttonComponent.onRightClick.AddListener(OnUnityRightClick);
        SynchronizeColors();
    }

    private void internalSetTexture(Texture newTexture, bool newShouldDestroyTexture)
    {
        if (rawImageComponent == null)
        {
            if (newShouldDestroyTexture && newTexture != null)
            {
                UnityEngine.Object.Destroy(newTexture);
            }
            return;
        }
        if (shouldDestroyTexture && desiredTexture != null)
        {
            UnityEngine.Object.Destroy(desiredTexture);
            desiredTexture = null;
        }
        desiredTexture = newTexture;
        shouldDestroyTexture = newShouldDestroyTexture;
        rawImageComponent.texture = ((desiredTexture != null) ? desiredTexture : ((Texture2D)GlazierResources.PixelTexture));
        SynchronizeColors();
    }

    private void OnUnityClick()
    {
        this._onImageClicked?.Invoke();
    }

    private void OnUnityRightClick()
    {
        this._onImageRightClicked?.Invoke();
    }
}
