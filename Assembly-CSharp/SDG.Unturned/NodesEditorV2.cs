using System.Collections.Generic;
using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

public class NodesEditorV2 : SelectionToolV2
{
    private TempNodeSystemBase _activeNodeSystem;

    public TempNodeSystemBase activeNodeSystem
    {
        get
        {
            return _activeNodeSystem;
        }
        set
        {
            DevkitSelectionManager.clear();
            _activeNodeSystem = value;
        }
    }

    protected override bool RaycastSelectableObjects(Ray ray, out RaycastHit hitInfo)
    {
        if (activeNodeSystem != null && Physics.Raycast(ray, out var hitInfo2))
        {
            GameObject gameObject = hitInfo2.transform.gameObject;
            if (gameObject != null && gameObject.GetComponent(activeNodeSystem.GetComponentType()) != null)
            {
                hitInfo = hitInfo2;
                return true;
            }
        }
        hitInfo = default(RaycastHit);
        return false;
    }

    protected override void RequestInstantiation(Vector3 position)
    {
        if (activeNodeSystem != null)
        {
            activeNodeSystem.Instantiate(position);
        }
    }

    protected override bool HasBoxSelectableObjects()
    {
        return activeNodeSystem != null;
    }

    protected override IEnumerable<GameObject> EnumerateBoxSelectableObjects()
    {
        return activeNodeSystem?.EnumerateGameObjects();
    }
}
