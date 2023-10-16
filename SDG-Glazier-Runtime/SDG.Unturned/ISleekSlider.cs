namespace SDG.Unturned;

public interface ISleekSlider : ISleekElement
{
    ESleekOrientation Orientation { get; set; }

    float Value { get; set; }

    SleekColor BackgroundColor { get; set; }

    SleekColor ForegroundColor { get; set; }

    bool IsInteractable { get; set; }

    event Dragged OnValueChanged;
}
