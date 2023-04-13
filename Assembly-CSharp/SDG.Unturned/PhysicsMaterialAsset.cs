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

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        if (data.TryGetList("UnityNames", out var node))
        {
            physicMaterialNames = new string[node.Count];
            for (int i = 0; i < node.Count; i++)
            {
                physicMaterialNames[i] = node.GetString(i);
            }
        }
        else
        {
            physicMaterialNames = new string[1] { data.GetString("UnityName") };
        }
        fallbackRef = data.ParseStruct<AssetReference<PhysicsMaterialAsset>>("Fallback");
        bulletImpactEffect = data.ParseStruct<AssetReference<EffectAsset>>("WipDoNotUseTemp_BulletImpactEffect");
        if (data.ContainsKey("Character_Friction_Mode"))
        {
            characterFrictionMode = data.ParseEnum("Character_Friction_Mode", EPhysicsMaterialCharacterFrictionMode.ImmediatelyResponsive);
            if (characterFrictionMode != 0)
            {
                if (data.ContainsKey("Character_Acceleration_Multiplier"))
                {
                    characterAccelerationMultiplier = data.ParseFloat("Character_Acceleration_Multiplier");
                }
                if (data.ContainsKey("Character_Deceleration_Multiplier"))
                {
                    characterDecelerationMultiplier = data.ParseFloat("Character_Deceleration_Multiplier");
                }
                if (data.ContainsKey("Character_Max_Speed_Multiplier"))
                {
                    characterMaxSpeedMultiplier = data.ParseFloat("Character_Max_Speed_Multiplier");
                }
            }
        }
        if (data.ContainsKey("IsArable"))
        {
            isArable = data.ParseBool("IsArable");
        }
        if (data.ContainsKey("HasOil"))
        {
            hasOil = data.ParseBool("HasOil");
        }
        PhysicMaterialCustomData.RegisterAsset(this);
    }
}
