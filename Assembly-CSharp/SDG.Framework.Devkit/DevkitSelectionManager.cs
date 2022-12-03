using System.Collections.Generic;
using SDG.Framework.Devkit.Interactable;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class DevkitSelectionManager
{
    protected static List<IDevkitInteractableBeginSelectionHandler> beginSelectionHandlers = new List<IDevkitInteractableBeginSelectionHandler>();

    protected static List<IDevkitInteractableEndSelectionHandler> endSelectionHandlers = new List<IDevkitInteractableEndSelectionHandler>();

    public static HashSet<DevkitSelection> selection = new HashSet<DevkitSelection>();

    public static InteractionData data = new InteractionData();

    public static GameObject mostRecentGameObject = null;

    public static void select(DevkitSelection select)
    {
        if (InputEx.GetKey(KeyCode.LeftShift) || InputEx.GetKey(KeyCode.LeftControl))
        {
            if (selection.Contains(select))
            {
                remove(select);
            }
            else
            {
                add(select);
            }
        }
        else
        {
            clear();
            add(select);
        }
    }

    public static void add(DevkitSelection select)
    {
        if (!(select.gameObject == null) && !selection.Contains(select) && beginSelection(select))
        {
            selection.Add(select);
            mostRecentGameObject = select.gameObject;
        }
    }

    public static void remove(DevkitSelection select)
    {
        if (selection.Remove(select))
        {
            endSelection(select);
            if (select.gameObject == mostRecentGameObject)
            {
                mostRecentGameObject = null;
            }
        }
    }

    public static void clear()
    {
        foreach (DevkitSelection item in selection)
        {
            endSelection(item);
        }
        selection.Clear();
        mostRecentGameObject = null;
    }

    public static bool beginSelection(DevkitSelection select)
    {
        if (select.gameObject == null)
        {
            return false;
        }
        data.collider = select.collider;
        beginSelectionHandlers.Clear();
        select.gameObject.GetComponentsInChildren(beginSelectionHandlers);
        foreach (IDevkitInteractableBeginSelectionHandler beginSelectionHandler in beginSelectionHandlers)
        {
            beginSelectionHandler.beginSelection(data);
        }
        return beginSelectionHandlers.Count > 0;
    }

    public static bool endSelection(DevkitSelection select)
    {
        if (select.gameObject == null)
        {
            return false;
        }
        data.collider = select.collider;
        endSelectionHandlers.Clear();
        select.gameObject.GetComponentsInChildren(endSelectionHandlers);
        foreach (IDevkitInteractableEndSelectionHandler endSelectionHandler in endSelectionHandlers)
        {
            endSelectionHandler.endSelection(data);
        }
        return endSelectionHandlers.Count > 0;
    }
}
