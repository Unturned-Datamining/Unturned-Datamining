using System;
using SDG.Framework.Foliage;
using UnityEngine;

namespace SDG.Unturned;

public class LandscapeMaterialAsset : Asset
{
    public ContentReference<Texture2D> texture;

    public ContentReference<Texture2D> mask;

    [Obsolete]
    public EPhysicsMaterial physicsMaterial;

    public string physicsMaterialName;

    public AssetReference<FoliageInfoCollectionAsset> foliage;

    public bool useAutoSlope;

    public float autoMinAngleBegin;

    public float autoMinAngleEnd;

    public float autoMaxAngleBegin;

    public float autoMaxAngleEnd;

    public bool useAutoFoundation;

    public float autoRayRadius;

    public float autoRayLength;

    public ERayMask autoRayMask;

    public AssetReference<LandscapeMaterialAsset> christmasRedirect;

    public AssetReference<LandscapeMaterialAsset> halloweenRedirect;

    public AssetReference<LandscapeMaterialAsset> aprilFoolsRedirect;

    private TerrainLayer layer;

    public override string FriendlyName
    {
        get
        {
            string text = name;
            if (name.EndsWith("_Material"))
            {
                text = text.Substring(0, text.Length - 9);
            }
            return text.Replace('_', ' ');
        }
    }

    public override EAssetType assetCategory => EAssetType.NONE;

    public AssetReference<LandscapeMaterialAsset> getHolidayRedirect()
    {
        return HolidayUtil.getActiveHoliday() switch
        {
            ENPCHoliday.CHRISTMAS => christmasRedirect, 
            ENPCHoliday.HALLOWEEN => halloweenRedirect, 
            ENPCHoliday.APRIL_FOOLS => aprilFoolsRedirect, 
            _ => AssetReference<LandscapeMaterialAsset>.invalid, 
        };
    }

    public TerrainLayer getOrCreateLayer()
    {
        if (layer == null)
        {
            layer = new TerrainLayer();
            layer.hideFlags = HideFlags.HideAndDontSave;
            layer.diffuseTexture = Assets.load(texture);
            if (layer.diffuseTexture == null)
            {
                layer.diffuseTexture = Texture2D.blackTexture;
            }
            layer.normalMapTexture = Assets.load(mask);
            if (layer.normalMapTexture == null)
            {
                layer.normalMapTexture = Texture2D.blackTexture;
            }
            layer.tileSize = new Vector2((float)layer.diffuseTexture.width / 4f, (float)layer.diffuseTexture.height / 4f);
        }
        return layer;
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        texture = data.ParseStruct<ContentReference<Texture2D>>("Texture");
        mask = data.ParseStruct<ContentReference<Texture2D>>("Mask");
        physicsMaterialName = data.GetString("Physics_Material");
        if (Enum.TryParse<EPhysicsMaterial>(physicsMaterialName, out physicsMaterial))
        {
            physicsMaterialName = PhysicsTool.GetNameOfLegacyMaterial(physicsMaterial);
        }
        foliage = data.ParseStruct<AssetReference<FoliageInfoCollectionAsset>>("Foliage");
        christmasRedirect = data.ParseStruct<AssetReference<LandscapeMaterialAsset>>("Christmas_Redirect");
        halloweenRedirect = data.ParseStruct<AssetReference<LandscapeMaterialAsset>>("Halloween_Redirect");
        aprilFoolsRedirect = data.ParseStruct<AssetReference<LandscapeMaterialAsset>>("AprilFools_Redirect");
        useAutoSlope = data.ParseBool("Use_Auto_Slope");
        autoMinAngleBegin = data.ParseFloat("Auto_Min_Angle_Begin");
        autoMinAngleEnd = data.ParseFloat("Auto_Min_Angle_End");
        autoMaxAngleBegin = data.ParseFloat("Auto_Max_Angle_Begin");
        autoMaxAngleEnd = data.ParseFloat("Auto_Max_Angle_End");
        useAutoFoundation = data.ParseBool("Use_Auto_Foundation");
        autoRayRadius = data.ParseFloat("Auto_Ray_Radius");
        autoRayLength = data.ParseFloat("Auto_Ray_Length");
        autoRayMask = data.ParseEnum("Auto_Ray_Mask", (ERayMask)0);
    }
}
