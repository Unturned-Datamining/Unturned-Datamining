using SDG.Framework.Devkit;
using SDG.Framework.Devkit.Interactable;

namespace SDG.Unturned;

public class TempNodeBase : DevkitHierarchyWorldItem, IDevkitInteractableBeginSelectionHandler, IDevkitInteractableEndSelectionHandler
{
    public bool isSelected;

    public void beginSelection(InteractionData data)
    {
        isSelected = true;
    }

    public void endSelection(InteractionData data)
    {
        isSelected = false;
    }

    internal virtual ISleekElement CreateMenu()
    {
        return null;
    }
}
