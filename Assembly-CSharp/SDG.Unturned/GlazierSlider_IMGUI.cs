using UnityEngine;

namespace SDG.Unturned;

internal class GlazierSlider_IMGUI : GlazierElementBase_IMGUI, ISleekSlider, ISleekElement
{
    private float scroll;

    private float _state;

    public ESleekOrientation orientation { get; set; } = ESleekOrientation.VERTICAL;


    public float size { get; set; } = 0.25f;


    public float state
    {
        get
        {
            return _state;
        }
        set
        {
            _state = value;
            scroll = state * (1f - size);
        }
    }

    public SleekColor backgroundColor { get; set; } = GlazierConst.DefaultSliderBackgroundColor;


    public SleekColor foregroundColor { get; set; } = GlazierConst.DefaultSliderForegroundColor;


    public bool isInteractable { get; set; } = true;


    public event Dragged onDragged;

    public override void OnGUI()
    {
        bool enabled = GUI.enabled;
        GUI.enabled = isInteractable;
        float num = GlazierUtils_IMGUI.drawSlider(drawRect, orientation, scroll, size, backgroundColor);
        GUI.enabled = enabled;
        if (num != scroll)
        {
            _state = num / (1f - size);
            if (state < 0f)
            {
                state = 0f;
            }
            else if (state > 1f)
            {
                state = 1f;
            }
            this.onDragged?.Invoke(this, state);
        }
        scroll = num;
        ChildrenOnGUI();
    }
}
