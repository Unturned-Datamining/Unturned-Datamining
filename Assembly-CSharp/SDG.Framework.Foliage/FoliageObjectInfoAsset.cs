using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage;

public class FoliageObjectInfoAsset : FoliageInfoAsset
{
    public AssetReference<ObjectAsset> obj;

    public float obstructionRadius;

    public override void bakeFoliage(FoliageBakeSettings bakeSettings, IFoliageSurface surface, Bounds bounds, float surfaceWeight, float collectionWeight)
    {
        if (bakeSettings.bakeObjects && !bakeSettings.bakeClear)
        {
            base.bakeFoliage(bakeSettings, surface, bounds, surfaceWeight, collectionWeight);
        }
    }

    public override int getInstanceCountInVolume(IShapeVolume volume)
    {
        Bounds worldBounds = volume.worldBounds;
        RegionBounds regionBounds = new RegionBounds(worldBounds);
        int num = 0;
        for (byte b = regionBounds.min.x; b <= regionBounds.max.x; b++)
        {
            for (byte b2 = regionBounds.min.y; b2 <= regionBounds.max.y; b2++)
            {
                foreach (LevelObject item in LevelObjects.objects[b, b2])
                {
                    if (obj.isReferenceTo(item.asset) && volume.containsPoint(item.transform.position))
                    {
                        num++;
                    }
                }
            }
        }
        return num;
    }

    protected override void addFoliage(Vector3 position, Quaternion rotation, Vector3 scale, bool clearWhenBaked)
    {
        ObjectAsset objectAsset = Assets.find(obj);
        if (objectAsset != null)
        {
            LevelObjects.addObject(position, rotation, scale, 0, objectAsset.GUID, clearWhenBaked ? ELevelObjectPlacementOrigin.GENERATED : ELevelObjectPlacementOrigin.PAINTED);
        }
    }

    protected override bool isPositionValid(Vector3 position)
    {
        if (!VolumeManager<FoliageVolume, FoliageVolumeManager>.Get().IsPositionBakeable(position, instancedMeshes: false, resources: false, objects: true))
        {
            return false;
        }
        return true;
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        obj = data.ParseStruct<AssetReference<ObjectAsset>>("Object");
        if (data.ContainsKey("Obstruction_Radius"))
        {
            obstructionRadius = data.ParseFloat("Obstruction_Radius");
        }
    }

    protected virtual void resetObject()
    {
        obstructionRadius = 4f;
    }

    public FoliageObjectInfoAsset()
    {
        resetObject();
    }
}
