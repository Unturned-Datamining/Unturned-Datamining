namespace SDG.Unturned;

/// <summary>
/// Expands upon Unity physics material properties for gameplay features.
/// </summary>
public class PhysicsMaterialAsset : PhysicsMaterialAssetBase
{
    /// <summary>
    /// Originally considered assets for each legacy material with fallback to main material, but the fallback
    /// would mean a failed lookup for every property in the vast majority of cases.
    /// </summary>
    public string[] physicMaterialNames;

    public AssetReference<PhysicsMaterialAsset> fallbackRef;

    public AssetReference<EffectAsset> bulletImpactEffect;

    public AssetReference<EffectAsset> tireMotionEffect;

    public EPhysicsMaterialCharacterFrictionMode characterFrictionMode;

    /// <summary>
    /// If true, crops can be planted on this material.
    /// </summary>
    public bool? isArable;

    /// <summary>
    /// If true, oil drills can be placed on this material.
    /// </summary>
    public bool? hasOil;

    /// <summary>
    /// For custom friction mode, multiplies character acceleration.
    /// </summary>
    public float? characterAccelerationMultiplier;

    /// <summary>
    /// For custom friction mode, multiplies character deceleration.
    /// </summary>
    public float? characterDecelerationMultiplier;

    /// <summary>
    /// For custom friction mode, multiplies character max speed.
    /// </summary>
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
        tireMotionEffect = data.ParseStruct<AssetReference<EffectAsset>>("TireMotionEffect");
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
