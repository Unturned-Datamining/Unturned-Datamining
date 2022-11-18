using UnityEngine;

namespace SDG.Unturned;

public class CartographyVolumeManager : VolumeManager<CartographyVolume, CartographyVolumeManager>
{
    public CartographyVolume GetMainVolume()
    {
        return allVolumes.HeadOrDefault();
    }

    protected override void OnUpdateGizmos(RuntimeGizmos runtimeGizmos)
    {
        base.OnUpdateGizmos(runtimeGizmos);
        foreach (CartographyVolume allVolume in allVolumes)
        {
            Color color = (allVolume.isSelected ? Color.yellow : debugColor);
            runtimeGizmos.Arrow(allVolume.transform.position, allVolume.transform.forward, 1f, color);
        }
    }

    public CartographyVolumeManager()
    {
        base.FriendlyName = "Cartography (GPS/Chart)";
        SetDebugColor(new Color32(150, 125, 100, byte.MaxValue));
    }
}
