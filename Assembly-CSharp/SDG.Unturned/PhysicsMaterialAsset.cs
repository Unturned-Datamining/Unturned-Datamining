using SDG.Framework.IO.FormattedFiles;

namespace SDG.Unturned;

public class PhysicsMaterialAsset : PhysicsMaterialAssetBase
{
    public string[] physicMaterialNames;

    public AssetReference<PhysicsMaterialAsset> fallbackRef;

    public AssetReference<EffectAsset> bulletImpactEffect;

    public EPhysicsMaterialCharacterFrictionMode characterFrictionMode;

    public bool? isArable;

    public bool? hasOil;

    public float? characterAccelerationMultiplier;

    public float? characterDecelerationMultiplier;

    public float? characterMaxSpeedMultiplier;

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        int num = reader.readArrayLength("UnityNames");
        if (num > 0)
        {
            physicMaterialNames = new string[num];
            for (int i = 0; i < num; i++)
            {
                physicMaterialNames[i] = reader.readValue(i);
            }
        }
        else
        {
            physicMaterialNames = new string[1] { reader.readValue("UnityName") };
        }
        fallbackRef = reader.readValue<AssetReference<PhysicsMaterialAsset>>("Fallback");
        bulletImpactEffect = reader.readValue<AssetReference<EffectAsset>>("WipDoNotUseTemp_BulletImpactEffect");
        if (reader.containsKey("Character_Friction_Mode"))
        {
            characterFrictionMode = reader.readValue<EPhysicsMaterialCharacterFrictionMode>("Character_Friction_Mode");
            if (characterFrictionMode != 0)
            {
                if (reader.containsKey("Character_Acceleration_Multiplier"))
                {
                    characterAccelerationMultiplier = reader.readValue<float>("Character_Acceleration_Multiplier");
                }
                if (reader.containsKey("Character_Deceleration_Multiplier"))
                {
                    characterDecelerationMultiplier = reader.readValue<float>("Character_Deceleration_Multiplier");
                }
                if (reader.containsKey("Character_Max_Speed_Multiplier"))
                {
                    characterMaxSpeedMultiplier = reader.readValue<float>("Character_Max_Speed_Multiplier");
                }
            }
        }
        if (reader.containsKey("IsArable"))
        {
            isArable = reader.readValue<bool>("IsArable");
        }
        if (reader.containsKey("HasOil"))
        {
            hasOil = reader.readValue<bool>("HasOil");
        }
        PhysicMaterialCustomData.RegisterAsset(this);
    }

    public PhysicsMaterialAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
    }
}
