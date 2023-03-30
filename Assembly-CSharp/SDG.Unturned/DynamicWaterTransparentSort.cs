using System;
using System.Collections.Generic;
using SDG.Framework.Water;
using UnityEngine;

namespace SDG.Unturned;

public class DynamicWaterTransparentSort : MonoBehaviour
{
    private class TransparentObject
    {
        public Transform transform;

        public Material material;

        public bool wasTransformUnderwater;

        public bool IsValid
        {
            get
            {
                if (transform != null)
                {
                    return material != null;
                }
                return false;
            }
        }

        public TransparentObject(Transform transform, Material material)
        {
            this.transform = transform;
            this.material = material;
        }

        public void UpdatePosition()
        {
            wasTransformUnderwater = WaterUtility.isPointUnderwater(transform.position);
        }

        public void UpdateRenderQueue(bool isCameraUnderwater)
        {
            if (wasTransformUnderwater)
            {
                if (isCameraUnderwater)
                {
                    material.renderQueue = 3100;
                }
                else
                {
                    material.renderQueue = 2900;
                }
            }
            else if (isCameraUnderwater)
            {
                material.renderQueue = 2900;
            }
            else
            {
                material.renderQueue = 3100;
            }
        }
    }

    private int updateIndex;

    private List<TransparentObject> managedObjects = new List<TransparentObject>();

    private static DynamicWaterTransparentSort instance;

    public static DynamicWaterTransparentSort Get()
    {
        if (instance == null)
        {
            GameObject obj = new GameObject("DynamicWaterTransparentSort");
            UnityEngine.Object.DontDestroyOnLoad(obj);
            obj.hideFlags = HideFlags.HideAndDontSave;
            instance = obj.AddComponent<DynamicWaterTransparentSort>();
        }
        return instance;
    }

    public object Register(Transform transform, Material material)
    {
        if (transform == null || material == null)
        {
            return null;
        }
        TransparentObject transparentObject = new TransparentObject(transform, material);
        transparentObject.UpdatePosition();
        transparentObject.UpdateRenderQueue(LevelLighting.isSea);
        managedObjects.Add(transparentObject);
        return transparentObject;
    }

    public void Unregister(object handle)
    {
        if (handle == null)
        {
            return;
        }
        for (int num = managedObjects.Count - 1; num >= 0; num--)
        {
            if (managedObjects[num] == handle)
            {
                managedObjects.RemoveAtFast(num);
                break;
            }
        }
    }

    private void HandleIsSeaChanged(bool isSea)
    {
        for (int num = managedObjects.Count - 1; num >= 0; num--)
        {
            TransparentObject transparentObject = managedObjects[num];
            if (transparentObject.IsValid)
            {
                transparentObject.UpdateRenderQueue(isSea);
            }
            else
            {
                managedObjects.RemoveAtFast(num);
            }
        }
    }

    private void Start()
    {
        LevelLighting.isSeaChanged += HandleIsSeaChanged;
        CommandLogMemoryUsage.OnExecuted = (Action<List<string>>)Delegate.Combine(CommandLogMemoryUsage.OnExecuted, new Action<List<string>>(OnLogMemoryUsage));
    }

    private void OnDestroy()
    {
        LevelLighting.isSeaChanged -= HandleIsSeaChanged;
        CommandLogMemoryUsage.OnExecuted = (Action<List<string>>)Delegate.Remove(CommandLogMemoryUsage.OnExecuted, new Action<List<string>>(OnLogMemoryUsage));
    }

    private void OnLogMemoryUsage(List<string> results)
    {
        results.Add($"Water transparent sort managed objects: {managedObjects.Count}");
    }

    private void Update()
    {
        if (managedObjects.Count >= 1)
        {
            updateIndex++;
            if (updateIndex >= managedObjects.Count)
            {
                updateIndex = 0;
            }
            TransparentObject transparentObject = managedObjects[updateIndex];
            if (transparentObject.IsValid)
            {
                transparentObject.UpdatePosition();
                transparentObject.UpdateRenderQueue(LevelLighting.isSea);
            }
            else
            {
                managedObjects.RemoveAtFast(updateIndex);
            }
        }
    }
}
