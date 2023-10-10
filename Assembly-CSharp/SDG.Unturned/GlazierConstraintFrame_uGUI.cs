using System;
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

internal class GlazierConstraintFrame_uGUI : GlazierElementBase_uGUI, ISleekConstraintFrame, ISleekElement
{
    private ESleekConstraint _constraint;

    private float _aspectRatio = 1f;

    private RectTransform contentTransform;

    private AspectRatioFitter aspectRatioFitter;

    public ESleekConstraint Constraint
    {
        get
        {
            return _constraint;
        }
        set
        {
            if (_constraint != 0)
            {
                throw new NotSupportedException();
            }
            _constraint = value;
            if (Constraint == ESleekConstraint.FitInParent)
            {
                aspectRatioFitter = contentTransform.gameObject.AddComponent<AspectRatioFitter>();
                aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                aspectRatioFitter.aspectRatio = _aspectRatio;
            }
        }
    }

    public float AspectRatio
    {
        get
        {
            return _aspectRatio;
        }
        set
        {
            _aspectRatio = value;
            if (aspectRatioFitter != null)
            {
                aspectRatioFitter.aspectRatio = value;
            }
        }
    }

    public override RectTransform AttachmentTransform => contentTransform;

    public GlazierConstraintFrame_uGUI(Glazier_uGUI glazier)
        : base(glazier)
    {
    }

    public override void ConstructNew()
    {
        base.ConstructNew();
        GameObject gameObject = new GameObject("Content", typeof(RectTransform));
        contentTransform = gameObject.GetRectTransform();
        contentTransform.SetParent(base.transform, worldPositionStays: false);
        contentTransform.anchorMin = new Vector2(0f, 0f);
        contentTransform.anchorMax = new Vector2(1f, 1f);
        contentTransform.anchoredPosition = Vector2.zero;
        contentTransform.sizeDelta = Vector2.zero;
    }
}
