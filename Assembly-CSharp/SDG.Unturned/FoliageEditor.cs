using System;
using System.Collections.Generic;
using SDG.Framework.Devkit;
using SDG.Framework.Devkit.Tools;
using SDG.Framework.Devkit.Transactions;
using SDG.Framework.Foliage;
using SDG.Framework.Rendering;
using SDG.Framework.Utilities;
using UnityEngine;

namespace SDG.Unturned;

internal class FoliageEditor : IDevkitTool
{
    public enum EFoliageMode
    {
        PAINT,
        EXACT,
        BAKE
    }

    public FoliageInfoCollectionAsset selectedCollectionAsset;

    public FoliageInfoAsset selectedInstanceAsset;

    public EFoliageMode mode = EFoliageMode.BAKE;

    private Vector3 pointerWorldPosition;

    private Vector3 brushWorldPosition;

    private Vector3 changePlanePosition;

    private bool isPointerOnWorld;

    private bool isBrushVisible;

    private Dictionary<FoliageInfoAsset, float> addWeights = new Dictionary<FoliageInfoAsset, float>();

    private float removeWeight;

    private List<FoliagePreviewSample> previewSamples = new List<FoliagePreviewSample>();

    private bool isChangingBrushRadius;

    private bool isChangingBrushFalloff;

    private bool isChangingBrushStrength;

    public float brushRadius
    {
        get
        {
            return DevkitFoliageToolOptions.instance.brushRadius;
        }
        set
        {
            DevkitFoliageToolOptions.instance.brushRadius = value;
        }
    }

    public float brushFalloff
    {
        get
        {
            return DevkitFoliageToolOptions.instance.brushFalloff;
        }
        set
        {
            DevkitFoliageToolOptions.instance.brushFalloff = value;
        }
    }

    public float brushStrength
    {
        get
        {
            return DevkitFoliageToolOptions.instance.brushStrength;
        }
        set
        {
            DevkitFoliageToolOptions.instance.brushStrength = value;
        }
    }

    public uint maxPreviewSamples
    {
        get
        {
            return DevkitFoliageToolOptions.instance.maxPreviewSamples;
        }
        set
        {
            DevkitFoliageToolOptions.instance.maxPreviewSamples = value;
        }
    }

    private bool isChangingBrush
    {
        get
        {
            if (!isChangingBrushRadius && !isChangingBrushFalloff)
            {
                return isChangingBrushStrength;
            }
            return true;
        }
    }

    private void beginChangeHotkeyTransaction()
    {
        DevkitTransactionUtility.beginGenericTransaction();
        DevkitTransactionUtility.recordObjectDelta(DevkitFoliageToolOptions.instance);
    }

    private void endChangeHotkeyTransaction()
    {
        DevkitTransactionUtility.endGenericTransaction();
    }

    private void addFoliage(FoliageInfoAsset foliageAsset, float weightMultiplier)
    {
        if (foliageAsset == null)
        {
            return;
        }
        bool flag = false;
        float num = MathF.PI * brushRadius * brushRadius;
        float num2 = ((DevkitFoliageToolOptions.instance.densityTarget > 0.0001f) ? Mathf.Sqrt(foliageAsset.density / DevkitFoliageToolOptions.instance.densityTarget / MathF.PI) : 0f);
        if (!addWeights.TryGetValue(foliageAsset, out var value))
        {
            addWeights.Add(foliageAsset, 0f);
        }
        value += DevkitFoliageToolOptions.addSensitivity * num * brushStrength * weightMultiplier * Time.deltaTime;
        if (value > 1f)
        {
            previewSamples.Clear();
            int num3 = Mathf.FloorToInt(value);
            value -= (float)num3;
            for (int i = 0; i < num3; i++)
            {
                float num4 = brushRadius * UnityEngine.Random.value;
                float brushAlpha = getBrushAlpha(num4);
                if (UnityEngine.Random.value < brushAlpha)
                {
                    continue;
                }
                float f = MathF.PI * 2f * UnityEngine.Random.value;
                float x = Mathf.Cos(f) * num4;
                float z = Mathf.Sin(f) * num4;
                if (!Physics.Raycast(new Ray(brushWorldPosition + new Vector3(x, brushRadius, z), new Vector3(0f, -1f, 0f)), out var hitInfo, brushRadius * 2f, (int)DevkitFoliageToolOptions.instance.surfaceMask))
                {
                    continue;
                }
                if (num2 > 0.0001f)
                {
                    SphereVolume sphereVolume = new SphereVolume(hitInfo.point, num2);
                    if (foliageAsset.getInstanceCountInVolume(sphereVolume) > 0)
                    {
                        continue;
                    }
                }
                foliageAsset.addFoliageToSurface(hitInfo.point, hitInfo.normal, clearWhenBaked: false, followRules: true);
                flag = true;
            }
        }
        addWeights[foliageAsset] = value;
        if (flag)
        {
            LevelHierarchy.MarkDirty();
        }
    }

    private void removeInstances(FoliageTile foliageTile, FoliageInstanceList list, float sqrBrushRadius, float sqrBrushFalloffRadius, bool allowRemoveBaked, ref int sampleCount)
    {
        bool flag = false;
        for (int num = list.matrices.Count - 1; num >= 0; num--)
        {
            List<Matrix4x4> list2 = list.matrices[num];
            List<bool> list3 = list.clearWhenBaked[num];
            for (int num2 = list2.Count - 1; num2 >= 0; num2--)
            {
                if (!list3[num2] || allowRemoveBaked)
                {
                    Vector3 position = list2[num2].GetPosition();
                    float sqrMagnitude = (position - brushWorldPosition).sqrMagnitude;
                    if (sqrMagnitude < sqrBrushRadius)
                    {
                        bool flag2 = sqrMagnitude < sqrBrushFalloffRadius;
                        previewSamples.Add(new FoliagePreviewSample(position, flag2 ? Color.red : (Color.red / 2f)));
                        if (InputEx.GetKey(KeyCode.Mouse0) && flag2 && sampleCount > 0)
                        {
                            foliageTile.removeInstance(list, num, num2);
                            sampleCount--;
                            flag = true;
                        }
                    }
                }
            }
        }
        if (flag)
        {
            LevelHierarchy.MarkDirty();
        }
    }

    public void update()
    {
        Ray ray = EditorInteract.ray;
        isPointerOnWorld = Physics.Raycast(ray, out var hitInfo, 8192f, (int)DevkitFoliageToolOptions.instance.surfaceMask);
        pointerWorldPosition = hitInfo.point;
        previewSamples.Clear();
        if (!EditorInteract.isFlying && Glazier.Get().ShouldGameProcessInput)
        {
            if (InputEx.GetKeyDown(KeyCode.Q))
            {
                mode = EFoliageMode.PAINT;
            }
            if (InputEx.GetKeyDown(KeyCode.W))
            {
                mode = EFoliageMode.EXACT;
            }
            if (InputEx.GetKeyDown(KeyCode.E))
            {
                mode = EFoliageMode.BAKE;
            }
            if (mode == EFoliageMode.PAINT)
            {
                if (InputEx.GetKeyDown(KeyCode.B))
                {
                    isChangingBrushRadius = true;
                    beginChangeHotkeyTransaction();
                }
                if (InputEx.GetKeyDown(KeyCode.F))
                {
                    isChangingBrushFalloff = true;
                    beginChangeHotkeyTransaction();
                }
                if (InputEx.GetKeyDown(KeyCode.V))
                {
                    isChangingBrushStrength = true;
                    beginChangeHotkeyTransaction();
                }
            }
        }
        if (InputEx.GetKeyUp(KeyCode.B))
        {
            isChangingBrushRadius = false;
            endChangeHotkeyTransaction();
        }
        if (InputEx.GetKeyUp(KeyCode.F))
        {
            isChangingBrushFalloff = false;
            endChangeHotkeyTransaction();
        }
        if (InputEx.GetKeyUp(KeyCode.V))
        {
            isChangingBrushStrength = false;
            endChangeHotkeyTransaction();
        }
        if (isChangingBrush)
        {
            Plane plane = default(Plane);
            plane.SetNormalAndPosition(Vector3.up, brushWorldPosition);
            plane.Raycast(ray, out var enter);
            changePlanePosition = ray.origin + ray.direction * enter;
            if (isChangingBrushRadius)
            {
                brushRadius = (changePlanePosition - brushWorldPosition).magnitude;
            }
            if (isChangingBrushFalloff)
            {
                brushFalloff = Mathf.Clamp01((changePlanePosition - brushWorldPosition).magnitude / brushRadius);
            }
            if (isChangingBrushStrength)
            {
                brushStrength = (changePlanePosition - brushWorldPosition).magnitude / brushRadius;
            }
        }
        else
        {
            brushWorldPosition = pointerWorldPosition;
        }
        isBrushVisible = isPointerOnWorld || isChangingBrush;
        if (EditorInteract.isFlying || !Glazier.Get().ShouldGameProcessInput)
        {
            return;
        }
        if (mode == EFoliageMode.PAINT)
        {
            Bounds worldBounds = new Bounds(brushWorldPosition, new Vector3(brushRadius * 2f, 0f, brushRadius * 2f));
            float num = brushRadius * brushRadius;
            float num2 = num * brushFalloff * brushFalloff;
            float num3 = MathF.PI * brushRadius * brushRadius;
            bool key = InputEx.GetKey(KeyCode.LeftControl);
            bool flag = key || InputEx.GetKey(KeyCode.LeftAlt);
            if (key || flag || InputEx.GetKey(KeyCode.LeftShift))
            {
                removeWeight += DevkitFoliageToolOptions.removeSensitivity * num3 * brushStrength * Time.deltaTime;
                int sampleCount = 0;
                if (removeWeight > 1f)
                {
                    sampleCount = Mathf.FloorToInt(removeWeight);
                    removeWeight -= sampleCount;
                }
                FoliageBounds foliageBounds = new FoliageBounds(worldBounds);
                for (int i = foliageBounds.min.x; i <= foliageBounds.max.x; i++)
                {
                    for (int j = foliageBounds.min.y; j <= foliageBounds.max.y; j++)
                    {
                        FoliageTile tile = FoliageSystem.getTile(new FoliageCoord(i, j));
                        if (tile == null)
                        {
                            continue;
                        }
                        if (key)
                        {
                            if (selectedInstanceAsset != null)
                            {
                                if (tile.instances.TryGetValue(selectedInstanceAsset.getReferenceTo<FoliageInstancedMeshInfoAsset>(), out var value))
                                {
                                    removeInstances(tile, value, num, num2, flag, ref sampleCount);
                                }
                            }
                            else
                            {
                                if (selectedCollectionAsset == null)
                                {
                                    continue;
                                }
                                foreach (FoliageInfoCollectionAsset.FoliageInfoCollectionElement element in selectedCollectionAsset.elements)
                                {
                                    if (Assets.find(element.asset) is FoliageInstancedMeshInfoAsset foliageInstancedMeshInfoAsset && tile.instances.TryGetValue(foliageInstancedMeshInfoAsset.getReferenceTo<FoliageInstancedMeshInfoAsset>(), out var value2))
                                    {
                                        removeInstances(tile, value2, num, num2, flag, ref sampleCount);
                                    }
                                }
                            }
                            continue;
                        }
                        foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> instance in tile.instances)
                        {
                            FoliageInstanceList value3 = instance.Value;
                            removeInstances(tile, value3, num, num2, flag, ref sampleCount);
                        }
                    }
                }
                RegionBounds regionBounds = new RegionBounds(worldBounds);
                for (byte b = regionBounds.min.x; b <= regionBounds.max.x; b = (byte)(b + 1))
                {
                    for (byte b2 = regionBounds.min.y; b2 <= regionBounds.max.y; b2 = (byte)(b2 + 1))
                    {
                        List<ResourceSpawnpoint> list = LevelGround.trees[b, b2];
                        for (int num4 = list.Count - 1; num4 >= 0; num4--)
                        {
                            ResourceSpawnpoint resourceSpawnpoint = list[num4];
                            if (resourceSpawnpoint.isGenerated && !flag)
                            {
                                continue;
                            }
                            if (key)
                            {
                                if (selectedInstanceAsset != null)
                                {
                                    if (!(selectedInstanceAsset is FoliageResourceInfoAsset foliageResourceInfoAsset) || !foliageResourceInfoAsset.resource.isReferenceTo(resourceSpawnpoint.asset))
                                    {
                                        continue;
                                    }
                                }
                                else if (selectedCollectionAsset != null)
                                {
                                    bool flag2 = false;
                                    foreach (FoliageInfoCollectionAsset.FoliageInfoCollectionElement element2 in selectedCollectionAsset.elements)
                                    {
                                        if (Assets.find(element2.asset) is FoliageResourceInfoAsset foliageResourceInfoAsset2 && foliageResourceInfoAsset2.resource.isReferenceTo(resourceSpawnpoint.asset))
                                        {
                                            flag2 = true;
                                            break;
                                        }
                                    }
                                    if (!flag2)
                                    {
                                        continue;
                                    }
                                }
                            }
                            float sqrMagnitude = (resourceSpawnpoint.point - brushWorldPosition).sqrMagnitude;
                            if (sqrMagnitude < num)
                            {
                                bool flag3 = sqrMagnitude < num2;
                                previewSamples.Add(new FoliagePreviewSample(resourceSpawnpoint.point, flag3 ? Color.red : (Color.red / 2f)));
                                if (InputEx.GetKey(KeyCode.Mouse0) && flag3 && sampleCount > 0)
                                {
                                    resourceSpawnpoint.destroy();
                                    list.RemoveAt(num4);
                                    sampleCount--;
                                }
                            }
                        }
                        bool flag4 = false;
                        List<LevelObject> list2 = LevelObjects.objects[b, b2];
                        for (int num5 = list2.Count - 1; num5 >= 0; num5--)
                        {
                            LevelObject levelObject = list2[num5];
                            if (levelObject.placementOrigin != ELevelObjectPlacementOrigin.PAINTED)
                            {
                                continue;
                            }
                            if (key)
                            {
                                if (selectedInstanceAsset != null)
                                {
                                    if (!(selectedInstanceAsset is FoliageObjectInfoAsset foliageObjectInfoAsset) || !foliageObjectInfoAsset.obj.isReferenceTo(levelObject.asset))
                                    {
                                        continue;
                                    }
                                }
                                else if (selectedCollectionAsset != null)
                                {
                                    bool flag5 = false;
                                    foreach (FoliageInfoCollectionAsset.FoliageInfoCollectionElement element3 in selectedCollectionAsset.elements)
                                    {
                                        if (Assets.find(element3.asset) is FoliageObjectInfoAsset foliageObjectInfoAsset2 && foliageObjectInfoAsset2.obj.isReferenceTo(levelObject.asset))
                                        {
                                            flag5 = true;
                                            break;
                                        }
                                    }
                                    if (!flag5)
                                    {
                                        continue;
                                    }
                                }
                            }
                            float sqrMagnitude2 = (levelObject.transform.position - brushWorldPosition).sqrMagnitude;
                            if (sqrMagnitude2 < num)
                            {
                                bool flag6 = sqrMagnitude2 < num2;
                                previewSamples.Add(new FoliagePreviewSample(levelObject.transform.position, flag6 ? Color.red : (Color.red / 2f)));
                                if (InputEx.GetKey(KeyCode.Mouse0) && flag6 && sampleCount > 0)
                                {
                                    flag4 = true;
                                    LevelObjects.removeObject(levelObject.transform);
                                    sampleCount--;
                                }
                            }
                        }
                        if (flag4)
                        {
                            LevelHierarchy.MarkDirty();
                        }
                    }
                }
            }
            else
            {
                if (!InputEx.GetKey(KeyCode.Mouse0))
                {
                    return;
                }
                if (selectedInstanceAsset != null)
                {
                    addFoliage(selectedInstanceAsset, 1f);
                }
                else
                {
                    if (selectedCollectionAsset == null)
                    {
                        return;
                    }
                    foreach (FoliageInfoCollectionAsset.FoliageInfoCollectionElement element4 in selectedCollectionAsset.elements)
                    {
                        addFoliage(Assets.find(element4.asset), element4.weight);
                    }
                }
            }
        }
        else
        {
            if (mode != EFoliageMode.EXACT || !InputEx.GetKeyDown(KeyCode.Mouse0))
            {
                return;
            }
            if (selectedInstanceAsset != null)
            {
                if (selectedInstanceAsset != null)
                {
                    selectedInstanceAsset.addFoliageToSurface(hitInfo.point, hitInfo.normal, clearWhenBaked: false, followRules: false);
                    LevelHierarchy.MarkDirty();
                }
            }
            else if (selectedCollectionAsset != null)
            {
                FoliageInfoAsset foliageInfoAsset = Assets.find(selectedCollectionAsset.elements[UnityEngine.Random.Range(0, selectedCollectionAsset.elements.Count)].asset);
                if (foliageInfoAsset != null)
                {
                    foliageInfoAsset.addFoliageToSurface(hitInfo.point, hitInfo.normal, clearWhenBaked: false, followRules: false);
                    LevelHierarchy.MarkDirty();
                }
            }
        }
    }

    public void equip()
    {
        GLRenderer.render += handleGLRender;
    }

    public void dequip()
    {
        GLRenderer.render -= handleGLRender;
    }

    private float getBrushAlpha(float distance)
    {
        if (distance < brushFalloff)
        {
            return 1f;
        }
        return (1f - distance) / (1f - brushFalloff);
    }

    private void handleGLRender()
    {
        if (!isBrushVisible || !Glazier.Get().ShouldGameProcessInput)
        {
            return;
        }
        GLUtility.matrix = MathUtility.IDENTITY_MATRIX;
        if (previewSamples.Count <= maxPreviewSamples)
        {
            GLUtility.LINE_FLAT_COLOR.SetPass(0);
            GL.Begin(4);
            float num = Mathf.Lerp(0.25f, 1f, brushRadius / 256f);
            Vector3 size = new Vector3(num, num, num);
            foreach (FoliagePreviewSample previewSample in previewSamples)
            {
                GL.Color(previewSample.color);
                GLUtility.boxSolid(previewSample.position, size);
            }
            GL.End();
        }
        if (mode == EFoliageMode.PAINT)
        {
            GL.LoadOrtho();
            GLUtility.LINE_FLAT_COLOR.SetPass(0);
            GL.Begin(1);
            Color color = ((!isChangingBrushStrength) ? Color.yellow : Color.Lerp(Color.red, Color.green, brushStrength));
            Vector3 vector = MainCamera.instance.WorldToViewportPoint(brushWorldPosition);
            vector.z = 0f;
            Vector3 vector2 = MainCamera.instance.WorldToViewportPoint(brushWorldPosition + MainCamera.instance.transform.right * brushRadius);
            vector2.z = 0f;
            Vector3 vector3 = MainCamera.instance.WorldToViewportPoint(brushWorldPosition + MainCamera.instance.transform.up * brushRadius);
            vector3.z = 0f;
            Vector3 vector4 = MainCamera.instance.WorldToViewportPoint(brushWorldPosition + MainCamera.instance.transform.right * brushRadius * brushFalloff);
            vector4.z = 0f;
            Vector3 vector5 = MainCamera.instance.WorldToViewportPoint(brushWorldPosition + MainCamera.instance.transform.up * brushRadius * brushFalloff);
            vector5.z = 0f;
            GL.Color(color / 2f);
            GLUtility.circle(vector, 1f, vector2 - vector, vector3 - vector, 64f);
            GL.Color(color);
            GLUtility.circle(vector, 1f, vector4 - vector, vector5 - vector, 64f);
            GL.End();
        }
        else if (mode == EFoliageMode.EXACT)
        {
            GLUtility.matrix = Matrix4x4.TRS(brushWorldPosition, MathUtility.IDENTITY_QUATERNION, new Vector3(1f, 1f, 1f));
            GLUtility.LINE_FLAT_COLOR.SetPass(0);
            GL.Begin(1);
            GL.Color(Color.yellow);
            GLUtility.line(new Vector3(-1f, 0f, 0f), new Vector3(1f, 0f, 0f));
            GLUtility.line(new Vector3(0f, -1f, 0f), new Vector3(0f, 1f, 0f));
            GLUtility.line(new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, 1f));
            GL.End();
        }
    }
}
