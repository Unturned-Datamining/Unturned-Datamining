using System.Collections.Generic;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage;

public class FoliageInstancedMeshInfoAsset : FoliageInfoAsset
{
    public ContentReference<Mesh> mesh;

    public ContentReference<Material> material;

    public bool castShadows;

    public bool tileDither;

    public int drawDistance;

    public AssetReference<FoliageInstancedMeshInfoAsset>? christmasRedirect;

    public AssetReference<FoliageInstancedMeshInfoAsset>? halloweenRedirect;

    public AssetReference<FoliageInstancedMeshInfoAsset>? getHolidayRedirect()
    {
        return HolidayUtil.getActiveHoliday() switch
        {
            ENPCHoliday.CHRISTMAS => christmasRedirect, 
            ENPCHoliday.HALLOWEEN => halloweenRedirect, 
            _ => null, 
        };
    }

    public override void bakeFoliage(FoliageBakeSettings bakeSettings, IFoliageSurface surface, Bounds bounds, float surfaceWeight, float collectionWeight)
    {
        if (bakeSettings.bakeInstancesMeshes && !bakeSettings.bakeClear)
        {
            base.bakeFoliage(bakeSettings, surface, bounds, surfaceWeight, collectionWeight);
        }
    }

    public override int getInstanceCountInVolume(IShapeVolume volume)
    {
        Bounds worldBounds = volume.worldBounds;
        FoliageBounds foliageBounds = new FoliageBounds(worldBounds);
        int num = 0;
        for (int i = foliageBounds.min.x; i <= foliageBounds.max.x; i++)
        {
            for (int j = foliageBounds.min.y; j <= foliageBounds.max.y; j++)
            {
                FoliageTile tile = FoliageSystem.getTile(new FoliageCoord(i, j));
                if (tile == null || tile.instances == null || !tile.instances.TryGetValue(getReferenceTo<FoliageInstancedMeshInfoAsset>(), out var value))
                {
                    continue;
                }
                foreach (List<Matrix4x4> matrix in value.matrices)
                {
                    foreach (Matrix4x4 item in matrix)
                    {
                        if (volume.containsPoint(item.GetPosition()))
                        {
                            num++;
                        }
                    }
                }
            }
        }
        return num;
    }

    protected override void addFoliage(Vector3 position, Quaternion rotation, Vector3 scale, bool clearWhenBaked)
    {
        FoliageSystem.addInstance(getReferenceTo<FoliageInstancedMeshInfoAsset>(), position, rotation, scale, clearWhenBaked);
    }

    protected override bool isPositionValid(Vector3 position)
    {
        if (!VolumeManager<FoliageVolume, FoliageVolumeManager>.Get().IsPositionBakeable(position, instancedMeshes: true, resources: false, objects: false))
        {
            return false;
        }
        return true;
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        mesh = data.ParseStruct<ContentReference<Mesh>>("Mesh");
        material = data.ParseStruct<ContentReference<Material>>("Material");
        if (data.ContainsKey("Cast_Shadows"))
        {
            castShadows = data.ParseBool("Cast_Shadows");
        }
        else
        {
            castShadows = false;
        }
        if (data.ContainsKey("Tile_Dither"))
        {
            tileDither = data.ParseBool("Tile_Dither");
        }
        else
        {
            tileDither = true;
        }
        if (data.ContainsKey("Draw_Distance"))
        {
            drawDistance = data.ParseInt32("Draw_Distance");
        }
        else
        {
            drawDistance = -1;
        }
        if (data.ContainsKey("Christmas_Redirect"))
        {
            christmasRedirect = data.ParseStruct<AssetReference<FoliageInstancedMeshInfoAsset>>("Christmas_Redirect");
        }
        if (data.ContainsKey("Halloween_Redirect"))
        {
            halloweenRedirect = data.ParseStruct<AssetReference<FoliageInstancedMeshInfoAsset>>("Halloween_Redirect");
        }
    }

    protected virtual void resetInstancedMeshInfo()
    {
        tileDither = true;
        drawDistance = -1;
    }

    public FoliageInstancedMeshInfoAsset()
    {
        resetInstancedMeshInfo();
    }
}
