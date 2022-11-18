using UnityEngine;

namespace SDG.Unturned;

public class CartographyVolume : LevelVolume<CartographyVolume, CartographyVolumeManager>
{
    public void GetSatelliteCaptureTransform(out Vector3 position, out Quaternion rotation)
    {
        position = base.transform.TransformPoint(new Vector3(0f, 0.5f, 0f));
        rotation = base.transform.rotation * Quaternion.Euler(90f, 0f, 0f);
    }

    protected override void Awake()
    {
        supportsSphereShape = false;
        base.Awake();
    }
}
