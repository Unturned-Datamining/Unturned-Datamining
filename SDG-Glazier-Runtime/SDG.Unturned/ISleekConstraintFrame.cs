namespace SDG.Unturned;

public interface ISleekConstraintFrame : ISleekElement
{
    ESleekConstraint Constraint { get; set; }

    float AspectRatio { get; set; }
}
