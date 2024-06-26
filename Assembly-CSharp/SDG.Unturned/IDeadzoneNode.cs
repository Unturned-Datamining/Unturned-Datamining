namespace SDG.Unturned;

public interface IDeadzoneNode
{
    EDeadzoneType DeadzoneType { get; }

    /// <summary>
    /// Damage dealt to players while inside the volume if they *don't* have clothing matching the deadzone type.
    /// Could help prevent players from running in and out to grab a few items without dieing.
    /// </summary>
    float UnprotectedDamagePerSecond { get; }

    /// <summary>
    /// Damage dealt to players while inside the volume if they *do* have clothing matching the deadzone type.
    /// For example, an area could be so dangerous that even with protection they take a constant 0.1 DPS.
    /// </summary>
    float ProtectedDamagePerSecond { get; }

    /// <summary>
    /// Virus damage to players while inside the volume if they *don't* have clothing matching the deadzone type.
    /// Defaults to 6.25 to preserve behavior from before adding this property.
    /// </summary>
    float UnprotectedRadiationPerSecond { get; }

    /// <summary>
    /// Rate of depletion from gasmask filter's quality/durability.
    /// Defaults to 0.4 to preserve behavior from before adding this property.
    /// </summary>
    float MaskFilterDamagePerSecond { get; }
}
