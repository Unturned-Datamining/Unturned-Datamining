using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage;

public abstract class FoliageInfoAsset : Asset
{
    protected delegate void BakeFoliageStepHandler(IFoliageSurface surface, Bounds bounds, float surfaceWeight, float collectionWeight);

    public float density;

    public float minNormalPositionOffset;

    public float maxNormalPositionOffset;

    public Vector3 normalRotationOffset;

    public float normalRotationAlignment;

    public float minSurfaceWeight;

    public float maxSurfaceWeight;

    public float minSurfaceAngle;

    public float maxSurfaceAngle;

    public Vector3 minRotation;

    public Vector3 maxRotation;

    public Vector3 minScale;

    public Vector3 maxScale;

    public virtual float randomNormalPositionOffset => Random.Range(minNormalPositionOffset, maxNormalPositionOffset);

    public virtual Quaternion randomRotation => Quaternion.Euler(new Vector3(Random.Range(minRotation.x, maxRotation.x), Random.Range(minRotation.y, maxRotation.y), Random.Range(minRotation.z, maxRotation.z)));

    public bool UniformScale { get; set; }

    public virtual Vector3 randomScale => new Vector3(Random.Range(minScale.x, maxScale.x), Random.Range(minScale.y, maxScale.y), Random.Range(minScale.z, maxScale.z));

    public virtual void bakeFoliage(FoliageBakeSettings bakeSettings, IFoliageSurface surface, Bounds bounds, float surfaceWeight, float collectionWeight)
    {
        if (isSurfaceWeightValid(surfaceWeight))
        {
            bakeFoliageSteps(surface, bounds, surfaceWeight, collectionWeight, handleBakeFoliageStep);
        }
    }

    public void addFoliageToSurface(Vector3 surfacePosition, Vector3 surfaceNormal, bool clearWhenBaked, bool followRules)
    {
        addFoliageToSurface(surfacePosition, surfaceNormal, clearWhenBaked, followRules, doCollisionChecks: true);
    }

    /// <param name="followRules">Should angle limits and subtractive volumes be respected? Disabled when manually placing individually.</param>
    /// <param name="doCollisionChecks">If true, trees do a sphere overlap to prevent placement inside objects.</param>
    public virtual void addFoliageToSurface(Vector3 surfacePosition, Vector3 surfaceNormal, bool clearWhenBaked, bool followRules, bool doCollisionChecks)
    {
        if (!followRules || isAngleValid(surfaceNormal))
        {
            Vector3 position = surfacePosition + surfaceNormal * randomNormalPositionOffset;
            if (!followRules || isPositionValid(position, doCollisionChecks))
            {
                Quaternion rotation = Quaternion.Lerp(MathUtility.IDENTITY_QUATERNION, Quaternion.FromToRotation(Vector3.up, surfaceNormal), normalRotationAlignment);
                rotation *= Quaternion.Euler(normalRotationOffset);
                rotation *= randomRotation;
                Vector3 scale = randomScale;
                addFoliage(position, rotation, scale, clearWhenBaked);
            }
        }
    }

    public abstract int getInstanceCountInVolume<T>(T volume) where T : IShapeVolume;

    protected abstract void addFoliage(Vector3 position, Quaternion rotation, Vector3 scale, bool clearWhenBaked);

    protected virtual void bakeFoliageSteps(IFoliageSurface surface, Bounds bounds, float surfaceWeight, float collectionWeight, BakeFoliageStepHandler callback)
    {
        float num = surfaceWeight * collectionWeight;
        float num2 = bounds.size.x * bounds.size.z / density * num;
        int num3 = Mathf.FloorToInt(num2);
        if (Random.value < num2 - (float)num3)
        {
            num3++;
        }
        for (int i = 0; i < num3; i++)
        {
            callback(surface, bounds, surfaceWeight, collectionWeight);
        }
    }

    /// <summary>
    /// Pick a point inside the bounds to test for foliage placement. The base implementation is completely random, but a blue noise implementation could be very nice.
    /// </summary>
    protected virtual Vector3 getTestPosition(Bounds bounds)
    {
        float x = Random.Range(-1f, 1f) * bounds.extents.x;
        float z = Random.Range(-1f, 1f) * bounds.extents.z;
        return bounds.center + new Vector3(x, bounds.extents.y, z);
    }

    protected virtual void handleBakeFoliageStep(IFoliageSurface surface, Bounds bounds, float surfaceWeight, float collectionWeight)
    {
        Vector3 testPosition = getTestPosition(bounds);
        if (surface.getFoliageSurfaceInfo(testPosition, out var surfacePosition, out var surfaceNormal))
        {
            addFoliageToSurface(surfacePosition, surfaceNormal, clearWhenBaked: true, followRules: true, doCollisionChecks: true);
        }
    }

    protected virtual bool isAngleValid(Vector3 surfaceNormal)
    {
        float num = Vector3.Angle(Vector3.up, surfaceNormal);
        if (num >= minSurfaceAngle)
        {
            return num <= maxSurfaceAngle;
        }
        return false;
    }

    protected abstract bool isPositionValid(Vector3 position, bool doCollisionChecks);

    protected virtual bool isSurfaceWeightValid(float surfaceWeight)
    {
        if (surfaceWeight >= minSurfaceWeight)
        {
            return surfaceWeight <= maxSurfaceWeight;
        }
        return false;
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        density = p.data.ParseFloat("Density");
        minNormalPositionOffset = p.data.ParseFloat("Min_Normal_Position_Offset");
        maxNormalPositionOffset = p.data.ParseFloat("Max_Normal_Position_Offset");
        normalRotationOffset = p.data.ParseVector3("Normal_Rotation_Offset");
        if (p.data.ContainsKey("Normal_Rotation_Alignment"))
        {
            normalRotationAlignment = p.data.ParseFloat("Normal_Rotation_Alignment");
        }
        else
        {
            normalRotationAlignment = 1f;
        }
        minSurfaceWeight = p.data.ParseFloat("Min_Weight");
        maxSurfaceWeight = p.data.ParseFloat("Max_Weight");
        minSurfaceAngle = p.data.ParseFloat("Min_Angle");
        maxSurfaceAngle = p.data.ParseFloat("Max_Angle");
        minRotation = p.data.ParseVector3("Min_Rotation");
        maxRotation = p.data.ParseVector3("Max_Rotation");
        UniformScale = p.data.ParseBool("Uniform_Scale");
        if (UniformScale)
        {
            float num = p.data.ParseFloat("Min_Scale");
            minScale = new Vector3(num, num, num);
            float num2 = p.data.ParseFloat("Max_Scale");
            maxScale = new Vector3(num2, num2, num2);
        }
        else
        {
            minScale = p.data.ParseVector3("Min_Scale");
            maxScale = p.data.ParseVector3("Max_Scale");
        }
    }

    protected virtual void resetFoliageInfo()
    {
        normalRotationAlignment = 1f;
        maxSurfaceWeight = 1f;
        minScale = Vector3.one;
        maxScale = Vector3.one;
    }

    public FoliageInfoAsset()
    {
        resetFoliageInfo();
    }
}
