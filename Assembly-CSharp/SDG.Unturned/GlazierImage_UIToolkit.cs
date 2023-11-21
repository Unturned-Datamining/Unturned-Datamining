using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned;

internal class GlazierImage_UIToolkit : GlazierElementBase_UIToolkit, ISleekImage, ISleekElement
{
    private float _rotationAngle;

    private bool _canRotate;

    private SleekColor _color = ESleekTint.NONE;

    private VisualElement containerElement;

    private Image imageElement;

    private Clickable clickable;

    private Texture desiredTexture;

    public Texture Texture
    {
        get
        {
            return desiredTexture;
        }
        set
        {
            if (desiredTexture != value)
            {
                internalSetTexture(value, ShouldDestroyTexture);
            }
        }
    }

    public float RotationAngle
    {
        get
        {
            return _rotationAngle;
        }
        set
        {
            _rotationAngle = value;
            SynchronizeRotation();
        }
    }

    public bool CanRotate
    {
        get
        {
            return _canRotate;
        }
        set
        {
            _canRotate = value;
            SynchronizeRotation();
        }
    }

    public bool ShouldDestroyTexture { get; set; }

    public SleekColor TintColor
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
            imageElement.tintColor = _color;
        }
    }

    private event System.Action _onImageClicked;

    public event System.Action OnClicked
    {
        add
        {
            if (clickable == null)
            {
                CreateClickable();
            }
            _onImageClicked += value;
        }
        remove
        {
            _onImageClicked -= value;
        }
    }

    private event System.Action _onImageRightClicked;

    public event System.Action OnRightClicked
    {
        add
        {
            if (clickable == null)
            {
                CreateClickable();
            }
            _onImageRightClicked += value;
        }
        remove
        {
            _onImageRightClicked -= value;
        }
    }

    public void UpdateTexture(Texture2D newTexture)
    {
        if (desiredTexture != newTexture)
        {
            internalSetTexture(newTexture, ShouldDestroyTexture);
        }
    }

    public void SetTextureAndShouldDestroy(Texture2D newTexture, bool newShouldDestroyTexture)
    {
        if (desiredTexture != newTexture || ShouldDestroyTexture != newShouldDestroyTexture)
        {
            internalSetTexture(newTexture, newShouldDestroyTexture);
        }
    }

    public override void InternalDestroy()
    {
        if (ShouldDestroyTexture && desiredTexture != null)
        {
            UnityEngine.Object.Destroy(desiredTexture);
            desiredTexture = null;
        }
        base.InternalDestroy();
    }

    public GlazierImage_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
        containerElement = new VisualElement();
        containerElement.userData = this;
        containerElement.AddToClassList("unturned-empty");
        containerElement.pickingMode = PickingMode.Ignore;
        imageElement = new Image();
        imageElement.AddToClassList("unturned-image");
        imageElement.scaleMode = ScaleMode.StretchToFill;
        imageElement.pickingMode = PickingMode.Ignore;
        containerElement.Add(imageElement);
        visualElement = containerElement;
    }

    internal override void SynchronizeColors()
    {
        imageElement.tintColor = _color;
    }

    private void internalSetTexture(Texture newTexture, bool newShouldDestroyTexture)
    {
        if (ShouldDestroyTexture && desiredTexture != null)
        {
            UnityEngine.Object.Destroy(desiredTexture);
            desiredTexture = null;
        }
        desiredTexture = newTexture;
        ShouldDestroyTexture = newShouldDestroyTexture;
        imageElement.image = desiredTexture;
    }

    private void CreateClickable()
    {
        containerElement.pickingMode = PickingMode.Position;
        clickable = new Clickable(OnClickedWithEventInfo);
        GlazierUtils_UIToolkit.AddClickableActivators(clickable);
        containerElement.AddManipulator(clickable);
    }

    private void OnClickedWithEventInfo(EventBase eventBase)
    {
        if (eventBase is IMouseEvent { button: var button })
        {
            switch (button)
            {
            case 0:
                this._onImageClicked?.Invoke();
                break;
            case 1:
                this._onImageRightClicked?.Invoke();
                break;
            }
        }
    }

    private void SynchronizeRotation()
    {
        imageElement.transform.rotation = (_canRotate ? Quaternion.AngleAxis(_rotationAngle, Vector3.forward) : Quaternion.identity);
    }
}
