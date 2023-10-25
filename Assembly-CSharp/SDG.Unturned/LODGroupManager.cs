using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal class LODGroupManager
{
    private class ComponentData
    {
        public LODGroupAdditionalData extensionComponent;

        public LODGroup unityComponent;

        public LOD[] originalLODs;

        public LOD[] modifiedLODs;
    }

    private static LODGroupManager instance = new LODGroupManager();

    private List<ComponentData> components = new List<ComponentData>();

    private float cachedLODBias = 1f;

    public static LODGroupManager Get()
    {
        return instance;
    }

    public void Register(LODGroupAdditionalData component)
    {
        if (component.LODBiasOverride != 0)
        {
            LODGroup component2 = component.GetComponent<LODGroup>();
            if (component2 == null)
            {
                UnturnedLog.warn("Additional Data without LOD Group: {0}", component.GetSceneHierarchyPath());
                return;
            }
            ComponentData componentData = components.AddDefaulted();
            componentData.extensionComponent = component;
            componentData.unityComponent = component2;
            componentData.originalLODs = componentData.unityComponent.GetLODs();
            componentData.modifiedLODs = componentData.unityComponent.GetLODs();
            UpdateComponent(componentData);
        }
    }

    public void Unregister(LODGroupAdditionalData component)
    {
        for (int num = components.Count - 1; num >= 0; num--)
        {
            if (components[num].extensionComponent == component)
            {
                components.RemoveAtFast(num);
                break;
            }
        }
    }

    /// <summary>
    /// Called after lod bias may have changed.
    /// </summary>
    public void SynchronizeLODBias()
    {
        float lodBias = QualitySettings.lodBias;
        if (MathfEx.IsNearlyEqual(cachedLODBias, lodBias))
        {
            return;
        }
        cachedLODBias = lodBias;
        foreach (ComponentData component in components)
        {
            UpdateComponent(component);
        }
    }

    private void UpdateComponent(ComponentData data)
    {
        for (int i = 0; i < data.originalLODs.Length; i++)
        {
            ref LOD reference = ref data.originalLODs[i];
            data.modifiedLODs[i].screenRelativeTransitionHeight = reference.screenRelativeTransitionHeight * cachedLODBias;
        }
        for (int j = 1; j < data.originalLODs.Length; j++)
        {
            ref LOD reference2 = ref data.modifiedLODs[j - 1];
            ref LOD reference3 = ref data.modifiedLODs[j];
            reference3.screenRelativeTransitionHeight = MathfEx.Min(0.999f, reference3.screenRelativeTransitionHeight, reference2.screenRelativeTransitionHeight - 0.001f);
        }
        data.unityComponent.SetLODs(data.modifiedLODs);
    }
}
