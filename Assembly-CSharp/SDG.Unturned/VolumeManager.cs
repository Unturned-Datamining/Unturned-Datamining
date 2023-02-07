using System.Collections.Generic;
using SDG.Framework.Devkit;
using SDG.Framework.Utilities;
using UnityEngine;

namespace SDG.Unturned;

public class VolumeManager<TVolume, TManager> : VolumeManagerBase where TVolume : LevelVolume<TVolume, TManager> where TManager : VolumeManager<TVolume, TManager>
{
    internal Color debugColor;

    internal Material solidMaterial;

    private ELevelVolumeVisibility visibility;

    protected bool allowInstantiation = true;

    protected bool supportsFalloff;

    internal List<TVolume> allVolumes;

    private static TManager instance;

    private static Shader solidShader = Shader.Find("Standard");

    public override ELevelVolumeVisibility Visibility
    {
        get
        {
            return visibility;
        }
        set
        {
            if (visibility == value)
            {
                return;
            }
            visibility = value;
            ConvenientSavedata.get().write("Visibility_" + typeof(TVolume).Name, (long)value);
            if (!Level.isEditor)
            {
                return;
            }
            foreach (TVolume allVolume in allVolumes)
            {
                allVolume.UpdateEditorVisibility(visibility);
            }
        }
    }

    protected virtual ELevelVolumeVisibility DefaultVisibility => ELevelVolumeVisibility.Wireframe;

    public static TManager Get()
    {
        return instance;
    }

    public IReadOnlyList<TVolume> GetAllVolumes()
    {
        return allVolumes;
    }

    public void ForceUpdateEditorVisibility()
    {
        foreach (TVolume allVolume in allVolumes)
        {
            allVolume.UpdateEditorVisibility(visibility);
        }
    }

    public TVolume GetFirstOverlappingVolume(Vector3 position)
    {
        foreach (TVolume allVolume in allVolumes)
        {
            if (allVolume.IsPositionInsideVolume(position))
            {
                return allVolume;
            }
        }
        return null;
    }

    public bool IsPositionInsideAnyVolume(Vector3 position)
    {
        return (Object)GetFirstOverlappingVolume(position) != (Object)null;
    }

    public override bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance)
    {
        TVolume hitVolume;
        return Raycast(ray, out hitInfo, out hitVolume, maxDistance);
    }

    public override void InstantiateVolume(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (allowInstantiation)
        {
            if (Visibility == ELevelVolumeVisibility.Hidden)
            {
                Visibility = ELevelVolumeVisibility.Wireframe;
            }
            DevkitTypeFactory.instantiate(typeof(TVolume), position, rotation, scale);
        }
    }

    public override IEnumerable<VolumeBase> EnumerateAllVolumes()
    {
        return allVolumes;
    }

    public bool Raycast(Ray ray, out RaycastHit hitInfo, out TVolume hitVolume, float maxDistance)
    {
        hitInfo = default(RaycastHit);
        hitVolume = null;
        float num = maxDistance + 10f;
        foreach (TVolume allVolume in allVolumes)
        {
            if (allVolume.volumeCollider.Raycast(ray, out var hitInfo2, maxDistance) && hitInfo2.distance < num)
            {
                hitVolume = allVolume;
                num = hitInfo2.distance;
                hitInfo = hitInfo2;
            }
        }
        return (Object)hitVolume != (Object)null;
    }

    public virtual void AddVolume(TVolume volume)
    {
        if (Level.isEditor)
        {
            volume.UpdateEditorVisibility(visibility);
        }
        allVolumes.Add(volume);
    }

    public virtual void RemoveVolume(TVolume volume)
    {
        allVolumes.RemoveFast(volume);
    }

    public VolumeManager()
    {
        instance = (TManager)this;
        VolumeManagerBase.allManagers.Add(this);
        base.FriendlyName = typeof(TVolume).Name;
        allVolumes = new List<TVolume>();
        solidMaterial = new Material(solidShader);
        solidMaterial.hideFlags = HideFlags.HideAndDontSave;
        if (ConvenientSavedata.get().read("Visibility_" + typeof(TVolume).Name, out long value))
        {
            visibility = (ELevelVolumeVisibility)value;
        }
        else
        {
            visibility = DefaultVisibility;
        }
        TimeUtility.updated += PrivateOnUpdateGizmos;
    }

    protected virtual void OnUpdateGizmos(RuntimeGizmos runtimeGizmos)
    {
        foreach (TVolume allVolume in allVolumes)
        {
            Color color = (allVolume.isSelected ? Color.yellow : debugColor);
            switch (allVolume.Shape)
            {
            case ELevelVolumeShape.Box:
                RuntimeGizmos.Get().Box(allVolume.transform.localToWorldMatrix, Vector3.one, color);
                break;
            case ELevelVolumeShape.Sphere:
                RuntimeGizmos.Get().Sphere(allVolume.transform.localToWorldMatrix, 0.5f, color);
                break;
            }
        }
        if (!supportsFalloff)
        {
            return;
        }
        foreach (TVolume allVolume2 in allVolumes)
        {
            if (!(allVolume2.falloffDistance < 0.0001f))
            {
                Color color2 = (allVolume2.isSelected ? Color.yellow : debugColor);
                color2.a *= 0.25f;
                Matrix4x4 localToWorldMatrix = allVolume2.transform.localToWorldMatrix;
                switch (allVolume2.Shape)
                {
                case ELevelVolumeShape.Box:
                {
                    Vector3 localInnerBoxSize = allVolume2.GetLocalInnerBoxSize();
                    Vector3 vector = new Vector3(0.5f, 0.5f, 0.5f);
                    Vector3 vector2 = localInnerBoxSize * 0.5f;
                    RuntimeGizmos.Get().Box(localToWorldMatrix, localInnerBoxSize, color2);
                    RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(vector2.x, vector2.y, vector2.z)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(vector.x, vector.y, vector.z)), color2);
                    RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f - vector2.x, vector2.y, vector2.z)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f - vector.x, vector.y, vector.z)), color2);
                    RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(vector2.x, vector2.y, 0f - vector2.z)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(vector.x, vector.y, 0f - vector.z)), color2);
                    RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f - vector2.x, vector2.y, 0f - vector2.z)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f - vector.x, vector.y, 0f - vector.z)), color2);
                    RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(vector2.x, 0f - vector2.y, vector2.z)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(vector.x, 0f - vector.y, vector.z)), color2);
                    RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f - vector2.x, 0f - vector2.y, vector2.z)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f - vector.x, 0f - vector.y, vector.z)), color2);
                    RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(vector2.x, 0f - vector2.y, 0f - vector2.z)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(vector.x, 0f - vector.y, 0f - vector.z)), color2);
                    RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f - vector2.x, 0f - vector2.y, 0f - vector2.z)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f - vector.x, 0f - vector.y, 0f - vector.z)), color2);
                    break;
                }
                case ELevelVolumeShape.Sphere:
                {
                    float num = 0.5f;
                    float localInnerSphereRadius = allVolume2.GetLocalInnerSphereRadius();
                    RuntimeGizmos.Get().Sphere(localToWorldMatrix, localInnerSphereRadius, color2);
                    RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(localInnerSphereRadius, 0f, 0f)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(num, 0f, 0f)), color2);
                    RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f - localInnerSphereRadius, 0f, 0f)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f - num, 0f, 0f)), color2);
                    RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, localInnerSphereRadius, 0f)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, num, 0f)), color2);
                    RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f - localInnerSphereRadius, 0f)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f - num, 0f)), color2);
                    RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f, localInnerSphereRadius)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f, num)), color2);
                    RuntimeGizmos.Get().Line(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f, 0f - localInnerSphereRadius)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, 0f, 0f - num)), color2);
                    break;
                }
                }
            }
        }
    }

    protected void SetDebugColor(Color debugColor)
    {
        this.debugColor = debugColor;
        solidMaterial.SetColor("_Color", debugColor);
    }

    private void PrivateOnUpdateGizmos()
    {
        if (visibility != 0 && Level.isEditor)
        {
            OnUpdateGizmos(RuntimeGizmos.Get());
        }
    }
}
