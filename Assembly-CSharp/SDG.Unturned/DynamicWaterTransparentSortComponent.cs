using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class DynamicWaterTransparentSortComponent : MonoBehaviour
{
    private struct ManagedMaterial
    {
        public Transform rendererTransform;

        public Material instantiatedMaterial;

        public object handle;
    }

    public Renderer[] renderers;

    private List<ManagedMaterial> managedMaterials;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }
        managedMaterials = new List<ManagedMaterial>(renderers.Length);
        Renderer[] array = renderers;
        foreach (Renderer obj in array)
        {
            Transform rendererTransform = obj.transform;
            Material[] materials = obj.materials;
            foreach (Material instantiatedMaterial in materials)
            {
                ManagedMaterial item = default(ManagedMaterial);
                item.rendererTransform = rendererTransform;
                item.instantiatedMaterial = instantiatedMaterial;
                managedMaterials.Add(item);
            }
        }
    }

    private void OnEnable()
    {
        if (managedMaterials != null)
        {
            DynamicWaterTransparentSort dynamicWaterTransparentSort = DynamicWaterTransparentSort.Get();
            for (int num = managedMaterials.Count - 1; num >= 0; num--)
            {
                ManagedMaterial value = managedMaterials[num];
                value.handle = dynamicWaterTransparentSort.Register(value.rendererTransform, value.instantiatedMaterial);
                managedMaterials[num] = value;
            }
        }
    }

    private void OnDisable()
    {
        if (managedMaterials == null)
        {
            return;
        }
        DynamicWaterTransparentSort dynamicWaterTransparentSort = DynamicWaterTransparentSort.Get();
        foreach (ManagedMaterial managedMaterial in managedMaterials)
        {
            dynamicWaterTransparentSort.Unregister(managedMaterial.handle);
        }
    }

    private void OnDestroy()
    {
        if (managedMaterials == null)
        {
            return;
        }
        foreach (ManagedMaterial managedMaterial in managedMaterials)
        {
            Object.Destroy(managedMaterial.instantiatedMaterial);
        }
        managedMaterials = null;
    }
}
