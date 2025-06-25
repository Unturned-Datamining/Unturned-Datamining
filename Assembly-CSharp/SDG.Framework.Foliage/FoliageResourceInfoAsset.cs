using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage;

public class FoliageResourceInfoAsset : FoliageInfoAsset
{
    private static readonly Collider[] OBSTRUCTION_COLLIDERS = new Collider[16];

    public AssetReference<ResourceAsset> resource;

    public float obstructionRadius;

    public override void bakeFoliage(FoliageBakeSettings bakeSettings, IFoliageSurface surface, Bounds bounds, float surfaceWeight, float collectionWeight)
    {
        if (bakeSettings.bakeResources && !bakeSettings.bakeClear)
        {
            base.bakeFoliage(bakeSettings, surface, bounds, surfaceWeight, collectionWeight);
        }
    }

    public override int getInstanceCountInVolume(IShapeVolume volume)
    {
        int num = 0;
        foreach (Vector2Int item in Regions.GetCoordinateBoundsInt(volume.worldBounds))
        {
            foreach (ResourceSpawnpoint item2 in LevelGround.GetTreesOrNullInRegion(item))
            {
                if (resource.isReferenceTo(item2.asset) && volume.containsPoint(item2.point))
                {
                    num++;
                }
            }
        }
        return num;
    }

    protected override void addFoliage(Vector3 position, Quaternion rotation, Vector3 scale, bool clearWhenBaked)
    {
        ResourceAsset resourceAsset = Assets.find(resource);
        if (resourceAsset != null)
        {
            LevelGround.addSpawn(position, resourceAsset.GUID, clearWhenBaked);
        }
    }

    protected override bool isPositionValid(Vector3 position, bool doCollisionChecks)
    {
        if (!VolumeManager<FoliageVolume, FoliageVolumeManager>.Get().IsPositionBakeable(position, instancedMeshes: false, resources: true, objects: false))
        {
            return false;
        }
        if (doCollisionChecks)
        {
            int num = Physics.OverlapSphereNonAlloc(position, obstructionRadius, OBSTRUCTION_COLLIDERS, RayMasks.BLOCK_RESOURCE);
            for (int i = 0; i < num; i++)
            {
                ObjectAsset asset = LevelObjects.getAsset(OBSTRUCTION_COLLIDERS[i].transform);
                if (asset != null && !asset.isSnowshoe)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        resource = p.data.ParseStruct<AssetReference<ResourceAsset>>("Resource");
        if (p.data.ContainsKey("Obstruction_Radius"))
        {
            obstructionRadius = p.data.ParseFloat("Obstruction_Radius");
        }
    }

    protected virtual void resetResource()
    {
        obstructionRadius = 4f;
    }

    public FoliageResourceInfoAsset()
    {
        resetResource();
    }
}
