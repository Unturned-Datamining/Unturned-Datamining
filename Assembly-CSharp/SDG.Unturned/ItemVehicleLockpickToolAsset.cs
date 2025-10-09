namespace SDG.Unturned;

public class ItemVehicleLockpickToolAsset : ItemToolAsset
{
    private CachingBcAssetRef _failureEffectRef;

    public CachingBcAssetRef FailureEffect
    {
        get
        {
            return _failureEffectRef;
        }
        set
        {
            _failureEffectRef = value;
        }
    }

    /// <summary>
    /// If greater than zero, picking the lock can fail.
    /// </summary>
    public float FailureProbability { get; set; }

    public EffectAsset FindFailureEffect()
    {
        return _failureEffectRef.Get<EffectAsset>();
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        p.data.TryParseBcAssetRef("FailureEffect", EAssetType.EFFECT, out _failureEffectRef);
        FailureProbability = p.data.ParseFloat("FailureProbability");
    }
}
