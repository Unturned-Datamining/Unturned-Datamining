namespace SDG.Unturned;

internal enum EWheelSteeringMode
{
    /// <summary>
    /// Wheel does not affect steering.
    /// </summary>
    None,
    /// <summary>
    /// Set steering angle according to <see cref="P:SDG.Unturned.VehicleAsset.MaxSteeringAngleAtFullSpeed" /> and <see cref="P:SDG.Unturned.VehicleAsset.MaxSteeringAngle" />.
    /// </summary>
    SteeringAngle,
    /// <summary>
    /// Increase or decrease motor torque to rotate vehicle in-place. (Tanks)
    /// </summary>
    CrawlerTrack
}
