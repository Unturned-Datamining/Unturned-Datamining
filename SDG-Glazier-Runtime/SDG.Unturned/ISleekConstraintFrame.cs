namespace SDG.Unturned;

public interface ISleekConstraintFrame : ISleekElement
{
    ESleekConstraint constraint { get; set; }

    float AspectRatio { get; set; }
}
