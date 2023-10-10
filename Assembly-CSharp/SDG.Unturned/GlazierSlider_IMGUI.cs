using UnityEngine;

namespace SDG.Unturned;

internal class GlazierSlider_IMGUI : GlazierElementBase_IMGUI, ISleekSlider, ISleekElement
{
    private const float NormalizedHandleSize = 0.25f;

    private float scroll;

    private float _state;

    public ESleekOrientation Orientation { get; set; } = ESleekOrientation.VERTICAL;


    public float Value
    {
        get
        {
            return _state;
        }
        set
        {
            _state = value;
            scroll = Value * 0.75f;
        }
    }

    public SleekColor BackgroundColor { get; set; } = GlazierConst.DefaultSliderBackgroundColor;


    public SleekColor ForegroundColor { get; set; } = GlazierConst.DefaultSliderForegroundColor;


    public bool IsInteractable { get; set; } = true;


    public event Dragged OnValueChanged;

    public override void OnGUI()
    {
        bool enabled = GUI.enabled;
        GUI.enabled = IsInteractable;
        float num = GlazierUtils_IMGUI.drawSlider(drawRect, Orientation, scroll, 0.25f, BackgroundColor);
        GUI.enabled = enabled;
        if (num != scroll)
        {
            _state = num / 0.75f;
            if (Value < 0f)
            {
                Value = 0f;
            }
            else if (Value > 1f)
            {
                Value = 1f;
            }
            this.OnValueChanged?.Invoke(this, Value);
        }
        scroll = num;
        ChildrenOnGUI();
    }
}
