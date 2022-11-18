using System;
using SDG.Framework.Foliage;
using SDG.Framework.IO.FormattedFiles;
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

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        texture = reader.readValue<ContentReference<Texture2D>>("Texture");
        mask = reader.readValue<ContentReference<Texture2D>>("Mask");
        physicsMaterialName = reader.readValue("Physics_Material");
        if (Enum.TryParse<EPhysicsMaterial>(physicsMaterialName, out physicsMaterial))
        {
            physicsMaterialName = PhysicsTool.GetNameOfLegacyMaterial(physicsMaterial);
        }
        foliage = reader.readValue<AssetReference<FoliageInfoCollectionAsset>>("Foliage");
        christmasRedirect = reader.readValue<AssetReference<LandscapeMaterialAsset>>("Christmas_Redirect");
        halloweenRedirect = reader.readValue<AssetReference<LandscapeMaterialAsset>>("Halloween_Redirect");
        aprilFoolsRedirect = reader.readValue<AssetReference<LandscapeMaterialAsset>>("AprilFools_Redirect");
        useAutoSlope = reader.readValue<bool>("Use_Auto_Slope");
        autoMinAngleBegin = reader.readValue<float>("Auto_Min_Angle_Begin");
        autoMinAngleEnd = reader.readValue<float>("Auto_Min_Angle_End");
        autoMaxAngleBegin = reader.readValue<float>("Auto_Max_Angle_Begin");
        autoMaxAngleEnd = reader.readValue<float>("Auto_Max_Angle_End");
        useAutoFoundation = reader.readValue<bool>("Use_Auto_Foundation");
        autoRayRadius = reader.readValue<float>("Auto_Ray_Radius");
        autoRayLength = reader.readValue<float>("Auto_Ray_Length");
        autoRayMask = reader.readValue<ERayMask>("Auto_Ray_Mask");
    }

    protected override void writeAsset(IFormattedFileWriter writer)
    {
        base.writeAsset(writer);
        writer.writeValue("Texture", texture);
        writer.writeValue("Mask", mask);
        writer.writeValue("Physics_Material", physicsMaterialName);
        writer.writeValue("Foliage", foliage);
        writer.writeValue("Christmas_Redirect", christmasRedirect);
        writer.writeValue("Halloween_Redirect", halloweenRedirect);
        writer.writeValue("AprilFools_Redirect", aprilFoolsRedirect);
        writer.writeValue("Use_Auto_Slope", useAutoSlope);
        writer.writeValue("Auto_Min_Angle_Begin", autoMinAngleBegin);
        writer.writeValue("Auto_Min_Angle_End", autoMinAngleEnd);
        writer.writeValue("Auto_Max_Angle_Begin", autoMaxAngleBegin);
        writer.writeValue("Auto_Max_Angle_End", autoMaxAngleEnd);
        writer.writeValue("Use_Auto_Foundation", useAutoFoundation);
        writer.writeValue("Auto_Ray_Radius", autoRayRadius);
        writer.writeValue("Auto_Ray_Length", autoRayLength);
        writer.writeValue("Auto_Ray_Mask", autoRayMask);
    }

    public LandscapeMaterialAsset()
    {
    }

    public LandscapeMaterialAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
    }
}
