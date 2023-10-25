namespace SDG.Framework.Devkit.Interactable;

public interface IDevkitSelectionTransformableHandler
{
    /// <summary>
    /// Called when we position, rotate or scale this transform.
    /// </summary>
    void transformSelection();
}
