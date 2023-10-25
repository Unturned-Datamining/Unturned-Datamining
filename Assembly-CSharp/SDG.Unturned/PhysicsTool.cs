using System;
using SDG.Framework.Landscapes;
using SDG.Framework.Water;
using UnityEngine;

namespace SDG.Unturned;

public class PhysicsTool
{
    internal const int NAME_LENGTH_BITS = 6;

    [Obsolete("Intended for backwards compatibility")]
    public static string GetNameOfLegacyMaterial(EPhysicsMaterial material)
    {
        switch (material)
        {
        default:
            return null;
        case EPhysicsMaterial.CLOTH_DYNAMIC:
        case EPhysicsMaterial.CLOTH_STATIC:
            return "Cloth";
        case EPhysicsMaterial.TILE_DYNAMIC:
        case EPhysicsMaterial.TILE_STATIC:
            return "Tile";
        case EPhysicsMaterial.CONCRETE_DYNAMIC:
        case EPhysicsMaterial.CONCRETE_STATIC:
            return "Concrete";
        case EPhysicsMaterial.FLESH_DYNAMIC:
            return "Flesh";
        case EPhysicsMaterial.GRAVEL_DYNAMIC:
        case EPhysicsMaterial.GRAVEL_STATIC:
            return "Gravel";
        case EPhysicsMaterial.METAL_DYNAMIC:
        case EPhysicsMaterial.METAL_STATIC:
        case EPhysicsMaterial.METAL_SLIP:
            return "Metal";
        case EPhysicsMaterial.WOOD_DYNAMIC:
        case EPhysicsMaterial.WOOD_STATIC:
            return "Wood";
        case EPhysicsMaterial.FOLIAGE_STATIC:
        case EPhysicsMaterial.FOLIAGE_DYNAMIC:
            return "Foliage";
        case EPhysicsMaterial.SNOW_STATIC:
            return "Snow";
        case EPhysicsMaterial.ICE_STATIC:
            return "Ice";
        case EPhysicsMaterial.WATER_STATIC:
            return "Water";
        case EPhysicsMaterial.ALIEN_DYNAMIC:
            return "Alien";
        case EPhysicsMaterial.SAND_STATIC:
            return "Sand";
        }
    }

    public static string GetTerrainMaterialName(Vector3 position)
    {
        if (Landscape.getSplatmapMaterial(position, out var materialAsset))
        {
            LandscapeMaterialAsset landscapeMaterialAsset = Assets.find(materialAsset);
            if (landscapeMaterialAsset != null)
            {
                return landscapeMaterialAsset.physicsMaterialName;
            }
        }
        return null;
    }

    [Obsolete("Replaced by GetTerrainMaterialName")]
    public static EPhysicsMaterial checkMaterial(Vector3 point)
    {
        if (Landscape.getSplatmapMaterial(point, out var materialAsset))
        {
            LandscapeMaterialAsset landscapeMaterialAsset = Assets.find(materialAsset);
            if (landscapeMaterialAsset != null)
            {
                return landscapeMaterialAsset.physicsMaterial;
            }
        }
        return EPhysicsMaterial.NONE;
    }

    [Obsolete("Network attachment removes the need for distinction between dynamic and static materials")]
    public static bool isMaterialDynamic(EPhysicsMaterial material)
    {
        return material switch
        {
            EPhysicsMaterial.CLOTH_DYNAMIC => true, 
            EPhysicsMaterial.TILE_DYNAMIC => true, 
            EPhysicsMaterial.CONCRETE_DYNAMIC => true, 
            EPhysicsMaterial.FLESH_DYNAMIC => true, 
            EPhysicsMaterial.GRAVEL_DYNAMIC => true, 
            EPhysicsMaterial.METAL_DYNAMIC => true, 
            EPhysicsMaterial.WOOD_DYNAMIC => true, 
            _ => false, 
        };
    }

    /// <summary>
    /// Get legacy enum corresponding to Unity physics material object name.
    /// Moved from obsolete <cref>checkMaterial</cref> method.
    /// </summary>
    [Obsolete("Intended for backwards compatibility")]
    public static EPhysicsMaterial GetLegacyMaterialByName(string name)
    {
        return name switch
        {
            "Cloth" => EPhysicsMaterial.CLOTH_STATIC, 
            "Cloth_Dynamic" => EPhysicsMaterial.CLOTH_DYNAMIC, 
            "Cloth_Static" => EPhysicsMaterial.CLOTH_STATIC, 
            "Tile" => EPhysicsMaterial.TILE_STATIC, 
            "Tile_Dynamic" => EPhysicsMaterial.TILE_DYNAMIC, 
            "Tile_Static" => EPhysicsMaterial.TILE_STATIC, 
            "Concrete" => EPhysicsMaterial.CONCRETE_STATIC, 
            "Concrete_Dynamic" => EPhysicsMaterial.CONCRETE_DYNAMIC, 
            "Concrete_Static" => EPhysicsMaterial.CONCRETE_STATIC, 
            "Flesh" => EPhysicsMaterial.FLESH_DYNAMIC, 
            "Flesh_Dynamic" => EPhysicsMaterial.FLESH_DYNAMIC, 
            "Flesh_Static" => EPhysicsMaterial.FLESH_DYNAMIC, 
            "Gravel" => EPhysicsMaterial.GRAVEL_STATIC, 
            "Gravel_Dynamic" => EPhysicsMaterial.GRAVEL_DYNAMIC, 
            "Gravel_Static" => EPhysicsMaterial.GRAVEL_STATIC, 
            "Metal" => EPhysicsMaterial.METAL_STATIC, 
            "Metal_Dynamic" => EPhysicsMaterial.METAL_DYNAMIC, 
            "Metal_Static" => EPhysicsMaterial.METAL_STATIC, 
            "Metal_Slip" => EPhysicsMaterial.METAL_SLIP, 
            "Wood" => EPhysicsMaterial.WOOD_STATIC, 
            "Wood_Dynamic" => EPhysicsMaterial.WOOD_DYNAMIC, 
            "Wood_Static" => EPhysicsMaterial.WOOD_STATIC, 
            "Foliage" => EPhysicsMaterial.FOLIAGE_STATIC, 
            "Foliage_Dynamic" => EPhysicsMaterial.FOLIAGE_DYNAMIC, 
            "Foliage_Static" => EPhysicsMaterial.FOLIAGE_STATIC, 
            "Water" => EPhysicsMaterial.WATER_STATIC, 
            "Water_Dynamic" => EPhysicsMaterial.WATER_STATIC, 
            "Water_Static" => EPhysicsMaterial.WATER_STATIC, 
            "Snow" => EPhysicsMaterial.SNOW_STATIC, 
            "Snow_Dynamic" => EPhysicsMaterial.SNOW_STATIC, 
            "Snow_Static" => EPhysicsMaterial.SNOW_STATIC, 
            "Ice" => EPhysicsMaterial.ICE_STATIC, 
            "Ice_Dynamic" => EPhysicsMaterial.ICE_STATIC, 
            "Ice_Static" => EPhysicsMaterial.ICE_STATIC, 
            "Sand" => EPhysicsMaterial.SAND_STATIC, 
            "Sand_Dynamic" => EPhysicsMaterial.SAND_STATIC, 
            "Sand_Static" => EPhysicsMaterial.SAND_STATIC, 
            _ => EPhysicsMaterial.NONE, 
        };
    }

    [Obsolete("Replaced by GetMaterialName")]
    public static EPhysicsMaterial checkMaterial(Collider collider)
    {
        if (collider.sharedMaterial == null)
        {
            return EPhysicsMaterial.NONE;
        }
        return GetLegacyMaterialByName(collider.sharedMaterial.name);
    }

    public static string GetMaterialName(Vector3 point, Transform transform, Collider collider)
    {
        if (WaterUtility.isPointUnderwater(point))
        {
            return "Water_Static";
        }
        if (transform != null && transform.CompareTag("Ground"))
        {
            return GetTerrainMaterialName(point);
        }
        return collider?.sharedMaterial?.name;
    }

    public static string GetMaterialName(RaycastHit hit)
    {
        return GetMaterialName(hit.point, hit.transform, hit.collider);
    }
}
