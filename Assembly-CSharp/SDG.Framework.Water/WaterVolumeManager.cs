using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Water;

public class WaterVolumeManager : VolumeManager<WaterVolume, WaterVolumeManager>
{
    public static WaterVolume seaLevelVolume;

    private EGraphicQuality oldWaterQuality;

    private bool wasPlanarReflectionEnabled;

    public static float worldSeaLevel
    {
        get
        {
            if (seaLevelVolume != null)
            {
                return seaLevelVolume.transform.TransformPoint(0f, 0.5f, 0f).y;
            }
            return -1024f;
        }
    }

    public WaterVolumeManager()
    {
        base.FriendlyName = "Water";
        SetDebugColor(new Color32(50, 200, 200, byte.MaxValue));
        if (!Dedicator.IsDedicatedServer)
        {
            oldWaterQuality = GraphicsSettings.waterQuality;
            wasPlanarReflectionEnabled = oldWaterQuality == EGraphicQuality.ULTRA;
            GraphicsSettings.graphicsSettingsApplied += OnGraphicsSettingsApplied;
        }
    }

    private void OnGraphicsSettingsApplied()
    {
        EGraphicQuality waterQuality = GraphicsSettings.waterQuality;
        if (oldWaterQuality == waterQuality)
        {
            return;
        }
        oldWaterQuality = waterQuality;
        foreach (WaterVolume allVolume in allVolumes)
        {
            allVolume.SyncWaterQuality();
        }
        bool flag = waterQuality == EGraphicQuality.ULTRA;
        if (wasPlanarReflectionEnabled == flag)
        {
            return;
        }
        wasPlanarReflectionEnabled = flag;
        foreach (WaterVolume allVolume2 in allVolumes)
        {
            allVolume2.SyncPlanarReflectionEnabled();
        }
    }
}
