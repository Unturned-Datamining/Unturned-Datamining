using UnityEngine;

namespace SDG.Unturned;

public class PropellerModel
{
    public Transform transform;

    /// <summary>
    /// Material on Model_0, the low-speed actual blade.
    /// </summary>
    public Material bladeMaterial;

    /// <summary>
    /// Material on Model_1, the high-speed blurred outline.
    /// </summary>
    public Material motionBlurMaterial;

    /// <summary>
    /// transform's localRotation when the vehicle was instantiated.
    /// </summary>
    public Quaternion baseLocationRotation;

    public object bladeTransparencySortHandle;

    public object motionBlurTransparencySortHandle;
}
