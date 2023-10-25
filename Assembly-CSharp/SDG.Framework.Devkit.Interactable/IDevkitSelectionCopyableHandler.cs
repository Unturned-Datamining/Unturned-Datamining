using UnityEngine;

namespace SDG.Framework.Devkit.Interactable;

public interface IDevkitSelectionCopyableHandler
{
    /// <returns>Identical to this object.</returns>
    GameObject copySelection();
}
