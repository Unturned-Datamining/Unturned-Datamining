using System;
using System.Collections.Generic;
using SDG.Framework.Water;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace SDG.Unturned;

public sealed class SkyFogRenderer : PostProcessEffectRenderer<SkyFog>
{
    private Shader shader;

    private int fogColorId;

    private int skyColorId;

    private int equatorColorId;

    private int groundColorId;

    private int inverseProjectionMatrixId;

    private int cameraToWorldMatrixId;

    private int waterColorId;

    private int isCameraUnderwaterId;

    private int waterCountId;

    private int waterMatricesId;

    private const int MAX_WATER_COUNT = 3;

    private static Matrix4x4[] waterMatrices = new Matrix4x4[3];

    private static List<VolumeAlphaPair<WaterVolume>> relevantWaterVolumes = new List<VolumeAlphaPair<WaterVolume>>();

    private static Comparison<VolumeAlphaPair<WaterVolume>> volumeComparison = CompareVolumes;

    public override void Init()
    {
        base.Init();
        shader = Shader.Find("Hidden/Custom/SkyFog");
        fogColorId = Shader.PropertyToID("_FogColor");
        skyColorId = Shader.PropertyToID("_SkyColor");
        equatorColorId = Shader.PropertyToID("_EquatorColor");
        groundColorId = Shader.PropertyToID("_GroundColor");
        inverseProjectionMatrixId = Shader.PropertyToID("_InverseProjectionMatrix");
        cameraToWorldMatrixId = Shader.PropertyToID("_CameraToWorld");
        waterColorId = Shader.PropertyToID("_WaterColor");
        isCameraUnderwaterId = Shader.PropertyToID("_IsCameraUnderwater");
        waterCountId = Shader.PropertyToID("_WaterCount");
        waterMatricesId = Shader.PropertyToID("_WaterMatrices");
    }

    public override void Render(PostProcessRenderContext context)
    {
        PropertySheet propertySheet = context.propertySheets.Get(shader);
        propertySheet.properties.SetColor(fogColorId, RenderSettings.fogColor);
        propertySheet.properties.SetColor(skyColorId, RenderSettings.skybox.GetColor(skyColorId));
        propertySheet.properties.SetColor(equatorColorId, RenderSettings.skybox.GetColor(equatorColorId));
        propertySheet.properties.SetColor(groundColorId, RenderSettings.skybox.GetColor(groundColorId));
        propertySheet.properties.SetMatrix(inverseProjectionMatrixId, context.camera.projectionMatrix.inverse);
        propertySheet.properties.SetMatrix(cameraToWorldMatrixId, context.camera.cameraToWorldMatrix);
        FindRelevantWaterVolumes(context.camera.transform.position);
        int num = (LevelLighting.enableUnderwaterEffects ? Mathf.Min(relevantWaterVolumes.Count, 3) : 0);
        bool flag = LevelLighting.isSea && num > 0;
        propertySheet.properties.SetColor(waterColorId, LevelLighting.getSeaColor("_BaseColor"));
        propertySheet.properties.SetFloat(isCameraUnderwaterId, flag ? 1f : 0f);
        propertySheet.properties.SetInt(waterCountId, num);
        for (int i = 0; i < num; i++)
        {
            waterMatrices[i] = relevantWaterVolumes[i].volume.transform.worldToLocalMatrix;
        }
        propertySheet.properties.SetMatrixArray(waterMatricesId, waterMatrices);
        context.command.BlitFullscreenTriangle(context.source, context.destination, propertySheet, 0);
    }

    private void FindRelevantWaterVolumes(Vector3 viewPosition)
    {
        relevantWaterVolumes.Clear();
        List<WaterVolume> list = VolumeManager<WaterVolume, WaterVolumeManager>.Get().InternalGetAllVolumes();
        if (list.Count > 3)
        {
            foreach (WaterVolume item in list)
            {
                Vector3 closestWorldPosition = item.GetClosestWorldPosition(viewPosition);
                float sqrMagnitude = (viewPosition - closestWorldPosition).sqrMagnitude;
                if (sqrMagnitude < 4f)
                {
                    relevantWaterVolumes.Add(new VolumeAlphaPair<WaterVolume>(item, sqrMagnitude));
                }
            }
            relevantWaterVolumes.Sort(volumeComparison);
            return;
        }
        foreach (WaterVolume item2 in list)
        {
            relevantWaterVolumes.Add(new VolumeAlphaPair<WaterVolume>(item2, 0f));
        }
    }

    private static int CompareVolumes(VolumeAlphaPair<WaterVolume> lhs, VolumeAlphaPair<WaterVolume> rhs)
    {
        return lhs.alpha.CompareTo(rhs.alpha);
    }
}
