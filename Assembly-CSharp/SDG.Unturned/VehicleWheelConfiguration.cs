namespace SDG.Unturned;

internal class VehicleWheelConfiguration : IDatParseable
{
    /// <summary>
    /// If true, this configuration was created by <see cref="!:InteractableVehicle.BuildAutomaticWheelConfiguration" />.
    /// Otherwise, this configuration was loaded from the vehicle asset file.
    /// </summary>
    public bool wasAutomaticallyGenerated;

    /// <summary>
    /// Transform path relative to Vehicle prefab with WheelCollider component.
    /// </summary>
    public string wheelColliderPath;

    /// <summary>
    /// If true, WheelCollider's steerAngle is set according to steering wheel.
    /// </summary>
    public bool isColliderSteered;

    /// <summary>
    /// If true, WheelCollider's motorTorque is set according to accelerator input.
    /// </summary>
    public bool isColliderPowered;

    /// <summary>
    /// Transform path relative to Vehicle prefab. Animated to match WheelCollider state.
    /// </summary>
    public string modelPath;

    /// <summary>
    /// If true, model is animated according to steering input.
    /// Only kept for backwards compatibility. Prior to wheel configurations, only certain WheelColliders actually
    /// received steering input, while multiple models would appear to steer. For example, the APC's front 4 wheels
    /// appeared to rotate but only the front 2 actually affected physics.
    /// </summary>
    public bool isModelSteered;

    /// <summary>
    /// If true, model ignores isModelSteered and instead uses WheelCollider.GetWorldPose when simulating or the
    /// replicated state from the server when not simulating. Defaults to false.
    /// </summary>
    public bool modelUseColliderPose;

    /// <summary>
    /// If greater than zero, visual-only wheels (without a collider) like the extra wheels of the Snowmobile use
    /// this radius to calculate their rolling speed.
    /// </summary>
    public float modelRadius;

    /// <summary>
    /// If set, visual-only wheels without a collider (like the back wheels of the snowmobile) can copy RPM from
    /// a wheel that does have a collider. Requires modelRadius to also be set.
    /// </summary>
    public int copyColliderRpmIndex;

    public bool TryParse(IDatNode node)
    {
        if (node is DatDictionary datDictionary)
        {
            wheelColliderPath = datDictionary.GetString("WheelColliderPath");
            isColliderSteered = datDictionary.ParseBool("IsColliderSteered");
            isColliderPowered = datDictionary.ParseBool("IsColliderPowered");
            modelPath = datDictionary.GetString("ModelPath");
            isModelSteered = datDictionary.ParseBool("IsModelSteered");
            modelUseColliderPose = datDictionary.ParseBool("ModelUseColliderPose");
            modelRadius = datDictionary.ParseFloat("ModelRadius", -1f);
            copyColliderRpmIndex = datDictionary.ParseInt32("CopyColliderRpmIndex", -1);
            return true;
        }
        return false;
    }
}
