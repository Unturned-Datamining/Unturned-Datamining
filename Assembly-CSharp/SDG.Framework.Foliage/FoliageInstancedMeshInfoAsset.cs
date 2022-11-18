using System.Collections.Generic;
using SDG.Framework.IO.FormattedFiles;
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

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        mesh = reader.readValue<ContentReference<Mesh>>("Mesh");
        material = reader.readValue<ContentReference<Material>>("Material");
        if (reader.containsKey("Cast_Shadows"))
        {
            castShadows = reader.readValue<bool>("Cast_Shadows");
        }
        else
        {
            castShadows = false;
        }
        if (reader.containsKey("Tile_Dither"))
        {
            tileDither = reader.readValue<bool>("Tile_Dither");
        }
        else
        {
            tileDither = true;
        }
        if (reader.containsKey("Draw_Distance"))
        {
            drawDistance = reader.readValue<int>("Draw_Distance");
        }
        else
        {
            drawDistance = -1;
        }
        if (reader.containsKey("Christmas_Redirect"))
        {
            christmasRedirect = reader.readValue<AssetReference<FoliageInstancedMeshInfoAsset>>("Christmas_Redirect");
        }
        if (reader.containsKey("Halloween_Redirect"))
        {
            halloweenRedirect = reader.readValue<AssetReference<FoliageInstancedMeshInfoAsset>>("Halloween_Redirect");
        }
    }

    protected override void writeAsset(IFormattedFileWriter writer)
    {
        base.writeAsset(writer);
        writer.writeValue("Mesh", mesh);
        writer.writeValue("Material", material);
        writer.writeValue("Cast_Shadows", castShadows);
        writer.writeValue("Tile_Dither", tileDither);
        writer.writeValue("Draw_Distance", drawDistance);
        if (christmasRedirect.HasValue)
        {
            writer.writeValue("Christmas_Redirect", christmasRedirect.Value);
        }
        if (halloweenRedirect.HasValue)
        {
            writer.writeValue("Halloween_Redirect", halloweenRedirect.Value);
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

    public FoliageInstancedMeshInfoAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
        resetInstancedMeshInfo();
    }
}
