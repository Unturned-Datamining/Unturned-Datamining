using SDG.Framework.Water;
using UnityEngine;

namespace SDG.Unturned;

public class WaterHeightTransparentSort : MonoBehaviour
{
    protected bool isUnderwater;

    protected Material material;

    internal void updateRenderQueue()
    {
        if (material == null)
        {
            return;
        }
        if (WaterUtility.isPointUnderwater(base.transform.position))
        {
            if (LevelLighting.isSea)
            {
                material.renderQueue = 3100;
            }
            else
            {
                material.renderQueue = 2900;
            }
        }
        else if (LevelLighting.isSea)
        {
            material.renderQueue = 2900;
        }
        else
        {
            material.renderQueue = 3100;
        }
    }

    protected void handleIsSeaChanged(bool isSea)
    {
        updateRenderQueue();
    }

    protected void Start()
    {
        material = HighlighterTool.getMaterialInstance(base.transform);
        if (material != null)
        {
            LevelLighting.isSeaChanged += handleIsSeaChanged;
            updateRenderQueue();
        }
    }

    protected void OnDestroy()
    {
        if (material != null)
        {
            LevelLighting.isSeaChanged -= handleIsSeaChanged;
            Object.DestroyImmediate(material);
            material = null;
        }
    }
}
