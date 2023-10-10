using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned;

internal class GlazierSprite_UIToolkit : GlazierElementBase_UIToolkit, ISleekSprite, ISleekElement
{
    private Sprite _sprite;

    private SleekColor _color = ESleekTint.NONE;

    private ESleekSpriteType _drawMethod;

    private Vector2Int _tileRepeat;

    private Image control;

    private Clickable clickable;

    private VisualElement tiledImagesContainer;

    private List<Image> hackTiledImages;

    public Sprite Sprite
    {
        get
        {
            return _sprite;
        }
        set
        {
            _sprite = value;
            SynchronizeImage();
        }
    }

    public SleekColor TintColor
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
            control.tintColor = _color;
            control.style.unityBackgroundImageTintColor = _color;
        }
    }

    public ESleekSpriteType DrawMethod
    {
        get
        {
            return _drawMethod;
        }
        set
        {
            _drawMethod = value;
            SynchronizeImage();
        }
    }

    public bool IsRaycastTarget
    {
        get
        {
            throw new NotImplementedException();
        }
        set
        {
            throw new NotImplementedException();
        }
    }

    public Vector2Int TileRepeatHintForUITK
    {
        get
        {
            return _tileRepeat;
        }
        set
        {
            if (_tileRepeat != value)
            {
                _tileRepeat = value;
                SynchronizeImage();
            }
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

    public GlazierSprite_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
        control = new Image();
        control.userData = this;
        control.AddToClassList("unturned-sprite");
        control.pickingMode = PickingMode.Ignore;
        control.scaleMode = ScaleMode.StretchToFill;
        visualElement = control;
    }

    internal override void SynchronizeColors()
    {
        control.tintColor = _color;
        control.style.unityBackgroundImageTintColor = _color;
        if (hackTiledImages == null)
        {
            return;
        }
        foreach (Image hackTiledImage in hackTiledImages)
        {
            hackTiledImage.tintColor = _color;
        }
    }

    private void CreateClickable()
    {
        control.pickingMode = PickingMode.Position;
        clickable = new Clickable(OnClickedWithEventInfo);
        GlazierUtils_UIToolkit.AddClickableActivators(clickable);
        control.AddManipulator(clickable);
    }

    private void SynchronizeImage()
    {
        switch (_drawMethod)
        {
        case ESleekSpriteType.Regular:
            control.sprite = _sprite;
            control.style.backgroundImage = StyleKeyword.Null;
            DestroyTiledImages();
            break;
        case ESleekSpriteType.Tiled:
            control.sprite = null;
            control.style.backgroundImage = StyleKeyword.Null;
            UpdateTiledImages();
            break;
        case ESleekSpriteType.Sliced:
            control.sprite = null;
            control.style.backgroundImage = _sprite?.texture;
            DestroyTiledImages();
            break;
        }
    }

    private void DestroyTiledImages()
    {
        if (tiledImagesContainer != null)
        {
            tiledImagesContainer.RemoveFromHierarchy();
            tiledImagesContainer = null;
        }
        hackTiledImages = null;
    }

    private void UpdateTiledImages()
    {
        int num = _tileRepeat.x * _tileRepeat.y;
        if (num < 1 || _sprite == null)
        {
            if (tiledImagesContainer != null)
            {
                tiledImagesContainer.RemoveFromHierarchy();
            }
            return;
        }
        if (tiledImagesContainer == null)
        {
            tiledImagesContainer = new VisualElement();
            tiledImagesContainer.AddToClassList("unturned-empty");
            tiledImagesContainer.pickingMode = PickingMode.Ignore;
            tiledImagesContainer.style.position = Position.Absolute;
            tiledImagesContainer.style.left = 0f;
            tiledImagesContainer.style.right = 0f;
            tiledImagesContainer.style.top = 0f;
            tiledImagesContainer.style.bottom = 0f;
        }
        if (tiledImagesContainer.parent != visualElement)
        {
            visualElement.Add(tiledImagesContainer);
            tiledImagesContainer.SendToBack();
        }
        if (hackTiledImages == null)
        {
            hackTiledImages = new List<Image>(num);
        }
        else
        {
            hackTiledImages.Capacity = Mathf.Max(hackTiledImages.Capacity, num);
        }
        if (hackTiledImages.Count > num)
        {
            for (int num2 = hackTiledImages.Count - 1; num2 >= num; num2--)
            {
                hackTiledImages[num2].RemoveFromHierarchy();
            }
        }
        else if (hackTiledImages.Count < num)
        {
            for (int i = hackTiledImages.Count; i < num; i++)
            {
                Image image = new Image();
                image.AddToClassList("unturned-sprite");
                image.style.position = Position.Absolute;
                image.pickingMode = PickingMode.Ignore;
                image.scaleMode = ScaleMode.StretchToFill;
                tiledImagesContainer.Add(image);
                hackTiledImages.Add(image);
            }
        }
        float num3 = 100f / (float)_tileRepeat.x;
        float num4 = 100f / (float)_tileRepeat.y;
        for (int j = 0; j < num; j++)
        {
            Image image2 = hackTiledImages[j];
            if (image2.parent == null)
            {
                tiledImagesContainer.Add(image2);
            }
            image2.sprite = _sprite;
            image2.tintColor = _color;
            int num5 = j / _tileRepeat.x;
            int num6 = j % _tileRepeat.x;
            image2.style.left = Length.Percent((float)num6 * num3);
            image2.style.top = Length.Percent((float)num5 * num4);
            image2.style.width = Length.Percent(num3);
            image2.style.height = Length.Percent(num4);
        }
    }

    private void OnClickedWithEventInfo(EventBase eventBase)
    {
        this._onImageClicked?.Invoke();
    }
}
