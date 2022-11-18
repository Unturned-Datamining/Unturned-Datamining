using System;
using SDG.Framework.Devkit.Transactions;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class DevkitTypeFactory
{
    public static void instantiate(Type type, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (Level.isEditor)
        {
            DevkitTransactionManager.beginTransaction("Spawn " + type.Name);
            IDevkitHierarchyItem devkitHierarchyItem;
            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                GameObject gameObject = new GameObject();
                gameObject.name = type.Name;
                gameObject.transform.position = position;
                gameObject.transform.rotation = rotation;
                gameObject.transform.localScale = scale;
                DevkitTransactionUtility.recordInstantiation(gameObject);
                devkitHierarchyItem = gameObject.AddComponent(type) as IDevkitHierarchyItem;
            }
            else
            {
                devkitHierarchyItem = Activator.CreateInstance(type) as IDevkitHierarchyItem;
            }
            if (devkitHierarchyItem != null)
            {
                LevelHierarchy.initItem(devkitHierarchyItem);
            }
            DevkitTransactionManager.endTransaction();
        }
    }
}
