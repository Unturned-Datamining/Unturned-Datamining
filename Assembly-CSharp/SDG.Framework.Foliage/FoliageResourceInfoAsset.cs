using System.Collections.Generic;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage;

public class FoliageResourceInfoAsset : FoliageInfoAsset
{
    private static readonly Collider[] OBSTRUCTION_COLLIDERS = new Collider[16];

    public AssetReference<ResourceAsset> resource;

    public float obstructionRadius;

    /// <summary>
    /// If true, ResourceAsset's legacy random rotation and scale properties are used.
    /// (As opposed to FoliageInfoAsset properties.)
    /// Defaults to true for backwards compatibility.
    /// </summary>
    public bool UsesLegacyRotationAndScale { get; set; }

    public override void bakeFoliage(FoliageBakeSettings bakeSettings, IFoliageSurface surface, Bounds bounds, float surfaceWeight, float collectionWeight)
    {
        if (bakeSettings.bakeResources && !bakeSettings.bakeClear)
        {
            base.bakeFoliage(bakeSettings, surface, bounds, surfaceWeight, collectionWeight);
        }
    }

    public override int getInstanceCountInVolume<T>(T volume)
    {
        int num = 0;
        foreach (Vector2Int item in Regions.GetCoordinateBoundsInt(volume.worldBounds))
        {
            List<ResourceSpawnpoint> treesOrNullInRegion = LevelGround.GetTreesOrNullInRegion(item);
            if (treesOrNullInRegion == null)
            {
                continue;
            }
            foreach (ResourceSpawnpoint item2 in treesOrNullInRegion)
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
            if (UsesLegacyRotationAndScale)
            {
                resourceAsset.GetLegacyRotationAndScale(position, out rotation, out scale);
            }
            LevelGround.addSpawn(position, rotation, scale, resourceAsset.GUID, clearWhenBaked);
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
        UsesLegacyRotationAndScale = p.data.ParseBool("Legacy_Rotation_and_Scale", defaultValue: true);
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
