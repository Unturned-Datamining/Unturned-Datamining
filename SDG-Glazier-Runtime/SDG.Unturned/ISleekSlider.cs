namespace SDG.Unturned;

public interface ISleekSlider : ISleekElement
{
    ESleekOrientation orientation { get; set; }

    float size { get; set; }

    float state { get; set; }

    SleekColor backgroundColor { get; set; }

    SleekColor foregroundColor { get; set; }

    bool isInteractable { get; set; }

    event Dragged onDragged;
}
